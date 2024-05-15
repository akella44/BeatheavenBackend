using AutoMapper;
using FluentValidation;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApp.Wrappers;

namespace WebApp.Features;

public static class CreateTrack
{
    public class TrackDto
    {
        public string Name { get; set; }
        public string? Mbid { get; set; }
        public string Url { get; set; }
        public string Duration { get; set; }
        public Infrastructure.Data.Entities.Stream Stream { get; set; }
        public string Listeners { get; set; }
        public string Playcount { get; set; }
        public Artist Artist { get; set; }
        public Album Album { get; set; }
        public Tag Tag { get; set; }
        public Wiki Wiki { get; set; }
        public string YoutubeUrl { get; set; }
    }
    
    public class TrackProfile : Profile
    {
        public TrackProfile()
        {
            CreateMap<CreateTrack.TrackDto, Track>();
        }
    }
    public class Validator : AbstractValidator<CreateTrackCommand>
    {
        public Validator()
        {
            RuleFor(tc => tc.TrackModel).SetValidator(new TrackModelValidator())
                .WithErrorCode("Request.EmptyRequiredFields")
                .WithMessage("Required fields cant be empty");
        }
    }
    public class TrackModelValidator : AbstractValidator<TrackDto>
    {
        public TrackModelValidator()
        {
            RuleFor(t => t.Name).NotEmpty();
            RuleFor(t => t.Artist).NotEmpty();
            RuleFor(t => t.YoutubeUrl).NotEmpty();
        }
    }
    public record CreateTrackCommand(TrackDto TrackModel) : IRequest<Result<string, Error>>;
    
    public class CreateTrackCommandHandler : IRequestHandler<CreateTrackCommand,Result<string, Error>>
    {
        private readonly TracksRepository _tracksRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateTrackCommand> _validator;

        public CreateTrackCommandHandler(TracksRepository tracksRepository, IMapper mapper, IValidator<CreateTrackCommand> validator)
        {
            _tracksRepository = tracksRepository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result<string, Error>> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
        {
            var valdationResult = await _validator.ValidateAsync(request, cancellationToken);
            
            if (!valdationResult.IsValid)
                return new Error(valdationResult.Errors.FirstOrDefault()!.ErrorCode,
                    valdationResult.Errors.FirstOrDefault()!.ErrorMessage);
            
            Track track = _mapper.Map<Track>(request.TrackModel);
            return await _tracksRepository.CreateTrack(track);
        }
    }
}
public class CreateTrackEndpoint
{
    public static void Map(WebApplication application)
    {
        application.MapPost("api/v1/track", async ([FromBody] CreateTrack.TrackDto track, ISender sender) =>
        {
            Result<string, Error> trackInsertResult = await sender.Send(new CreateTrack.CreateTrackCommand(track));
            return trackInsertResult.Match(
                success: value => Results.Ok(new { TrackId = value }),
                failure: error => Results.BadRequest(new { Error = error.Description })
            );
        });
    }
}