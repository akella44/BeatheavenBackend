using FluentValidation;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApp.Wrappers;

namespace WebApp.Features;

public static class GetTrack
{
    public sealed class Validator : AbstractValidator<GetTrackQuery>
    {
        public Validator()
        {
            RuleFor(tq => tq.TrackId).NotEmpty()
                .WithErrorCode("Request.EmptyField")
                .WithMessage("Track id cant be empty");
        }
    }
    public record GetTrackQuery(string TrackId) : IRequest<Result<Track, Error>>;
    
    public class GetTrackQueryHandler : IRequestHandler<GetTrackQuery, Result<Track, Error>>
    {
        private TracksRepository _tracksRepository;
        private IValidator<GetTrackQuery> _validator;

        public GetTrackQueryHandler(TracksRepository tracksRepository, IValidator<GetTrackQuery> validator)
        {
            _tracksRepository = tracksRepository;
            _validator = validator;
        }

        public async Task<Result<Track, Error>> Handle(GetTrackQuery request, CancellationToken cancellationToken)
        {
            var valdationResult = await _validator.ValidateAsync(request, cancellationToken);
            
            if (!valdationResult.IsValid)
                return new Error(valdationResult.Errors.FirstOrDefault()!.ErrorCode,
                    valdationResult.Errors.FirstOrDefault()!.ErrorMessage);
            
            Track? track = await _tracksRepository.GetById(request.TrackId);

            if (track is null)
            {
                return new Error("Request.NotFound", "Track not found");
            }

            return track;
        }
    }
}
public class GetTrackEndpoint
{
    public static void Map(WebApplication application)
    {
        application.MapGet("api/v1/track", async ([FromQuery] string trackId, ISender sender) =>
        {
            GetTrack.GetTrackQuery getTrackQuery = new GetTrack.GetTrackQuery(trackId);
            Result<Track, Error> trackResult = await sender.Send(getTrackQuery);
            return trackResult.Match(
                success: value => Results.Ok(new { Track = value }),
                failure: error => Results.BadRequest(new { Error = error.Description })
            );
        });
    }
}