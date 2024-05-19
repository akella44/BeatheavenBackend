using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Repositories;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using WebApp.Wrappers;

namespace WebApp.Features;

public static class RecognizeMelody
{
    private static readonly string NnApiHost = Environment.GetEnvironmentVariable("NN_HOST")!;
    private static readonly int NnApiPort = Convert.ToInt32(Environment.GetEnvironmentVariable("NN_PORT")!);

    public class RecognizeResult
    {
        [JsonPropertyName("result")]
        public List<TrackMetadata> Result { get; set; } = new List<TrackMetadata>();
    }
    public class TrackMetadata
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("relevance")]
        public float Relevance { get; set; }
    }
    public class RecognizeResponse
    {
        [JsonPropertyName("tracks")] public List<RecognizeResponseDto> Tracks { get; set; } = new List<RecognizeResponseDto>();
    }

    public class RecognizeResponseDto
    {
        public string Name { get; set; }
        public Artist Artist { get; set; }
        public Album Album { get; set; }
        public string YoutubeUrl { get; set; }
    }
    public class TrackProfile : Profile
        {
            public TrackProfile()
            {
                CreateMap<Track, RecognizeResponseDto>();
            }
        }
    public record RecognizeCommand(byte[] Bytes) : IRequest<Result<RecognizeResponse, Error>>;

    public class RecognizeCommandHandler : IRequestHandler<RecognizeCommand, Result<RecognizeResponse, Error>>
    {
        private readonly HttpClient _httpClient;
        private readonly ISender _sender;
        private readonly IMapper _mapper;
        private readonly ILogger<RecognizeCommandHandler> _logger;

        public RecognizeCommandHandler(HttpClient httpClient, ISender sender, IMapper mapper, ILogger<RecognizeCommandHandler> logger)
        {
            _httpClient = httpClient;
            _sender = sender;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<RecognizeResponse, Error>> Handle(RecognizeCommand request, CancellationToken cancellationToken)
        {
            if (request.Bytes.Length == 0)
                return new Error("Request.Empty", "Melody have not been provided");
            
            await File.WriteAllBytesAsync($"{Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower()}.mp3", request.Bytes, cancellationToken);

            var fileContent = new ByteArrayContent(request.Bytes);
            var formData = new MultipartFormDataContent();

            formData.Add(fileContent, "file", $"{Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower()}.mp3");

            UriBuilder nnUriBuilder = new UriBuilder();
            nnUriBuilder.Scheme = "http";
            nnUriBuilder.Port = NnApiPort;
            nnUriBuilder.Host = NnApiHost;
            nnUriBuilder.Path = "find_similar";

            var response = await _httpClient.PostAsync(nnUriBuilder.Uri, formData, cancellationToken);

            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

            if (responseData is null)
            {
                return new Error("Request.NotFound", "Track not found");
            }

            var nnRecognizeResult = JsonSerializer.Deserialize<RecognizeResult>(responseData)!;
            var recognizeResponse = new RecognizeResponse();
            
            _logger.LogInformation($"Recognized metric {JsonSerializer.Serialize(nnRecognizeResult)}");
            foreach (var metadata in nnRecognizeResult.Result)
            {
                GetTrack.GetTrackQuery query = new GetTrack.GetTrackQuery(metadata.Id);
                Result<Track, Error> getTrackResult = await _sender.Send(query, cancellationToken);
                if (getTrackResult.IsOk)
                {
                    recognizeResponse.Tracks.Add(_mapper.Map<RecognizeResponseDto>(getTrackResult.Value));
                    continue;
                }

                return new Error(getTrackResult.Error.Code, getTrackResult.Error.Description);
            }

            return recognizeResponse;
        }
    }
}

public class MelodyRecognizeHub : Hub
{
    private readonly AudioStreamsRepository _audioStreamsRepository;
    private readonly ISender _sender;
    private readonly ILogger<MelodyRecognizeHub> _logger;

    public MelodyRecognizeHub(AudioStreamsRepository audioStreamsRepository, ISender sender, ILogger<MelodyRecognizeHub> logger)
    {
        _audioStreamsRepository = audioStreamsRepository;
        _sender = sender;
        _logger = logger;
    }
    
    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"New connection with id {Context.ConnectionId}");
        return Task.CompletedTask;
    }
    
    public async Task SendBytes(byte[] bytes)
    {
        _logger.LogInformation($"New bytes from connection {Context.ConnectionId}");
        await _audioStreamsRepository.AddBytesToAudioStream(Context.ConnectionId, bytes);
    }
    
    public async Task GetRecognizedResult()
    {
        _logger.LogInformation($"New get result command from connection {Context.ConnectionId}");
        var bytes = await _audioStreamsRepository.GetAudioStreamBytes(Context.ConnectionId);
        var recognizeCommand =
            new RecognizeMelody.RecognizeCommand(bytes);
        Result<RecognizeMelody.RecognizeResponse, Error> recognizeResult = await _sender.Send(recognizeCommand);
        
        if(recognizeResult.IsOk)
            _logger.LogInformation($"Recognize results for {Context.ConnectionId} {JsonSerializer.Serialize(recognizeResult.Value.Tracks.Select(t => t.Name))}");
        
        await Clients.Caller.SendAsync("RecognizedResults", JsonSerializer.Serialize(recognizeResult.Match<object>(
            success: value => (value),
            failure: error => (new { Error = error.Description })
        ), 
            new JsonSerializerOptions
        {
            WriteIndented = false
        }));
    }

    public async Task Ping(){
        _logger.LogInformation($"Ping from {Context.ConnectionId}");
        await Clients.Caller.SendAsync("Ping", "TEST");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _audioStreamsRepository.ClearAudioStream(Context.ConnectionId);
        
        _logger.LogInformation($"Сonnection with id {Context.ConnectionId} closed");
    }
}