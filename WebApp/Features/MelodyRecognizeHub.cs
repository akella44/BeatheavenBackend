﻿using System.Text.Json;
using System.Text.Json.Serialization;
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
        public List<string> Result { get; set; } = new List<string>();
    }

    public class RecognizeResponse
    {
        [JsonPropertyName("tracks")] 
        public List<Track> Tracks { get; set; } = new List<Track>();
    }

    public record RecognizeCommand(byte[] Bytes) : IRequest<Result<RecognizeResponse, Error>>;

    public class RecognizeCommandHandler : IRequestHandler<RecognizeCommand, Result<RecognizeResponse, Error>>
    {
        private readonly HttpClient _httpClient;
        private readonly ISender _sender;

        public RecognizeCommandHandler(HttpClient httpClient, ISender sender)
        {
            _httpClient = httpClient;
            _sender = sender;
        }

        public async Task<Result<RecognizeResponse, Error>> Handle(RecognizeCommand request, CancellationToken cancellationToken)
        {
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
            foreach (var id in nnRecognizeResult.Result)
            {
                GetTrack.GetTrackQuery query = new GetTrack.GetTrackQuery(id);
                Result<Track, Error> getTrackResult = await _sender.Send(query, cancellationToken);
                if (getTrackResult.IsOk)
                {
                    recognizeResponse.Tracks.Add(getTrackResult.Value);
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
        await _audioStreamsRepository.AddBytesToAudioStream(Context.ConnectionId, bytes);
    }
    
    public async Task GetRecognizeResult()
    {
        var bytes = await _audioStreamsRepository.GetAudioStreamBytes(Context.ConnectionId);
        var recognizeCommand =
            new RecognizeMelody.RecognizeCommand(bytes);
        Result<RecognizeMelody.RecognizeResponse, Error> recognizeResult = await _sender.Send(recognizeCommand);

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