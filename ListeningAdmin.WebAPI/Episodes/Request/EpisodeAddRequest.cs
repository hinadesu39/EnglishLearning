﻿using CommonHelper;
using FluentValidation;
using ListeningInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace ListeningAdmin.WebAPI.Episodes.Request
{
    public record EpisodeAddRequest(MultilingualString Name, Guid AlbumId,
        Uri AudioUrl, double DurationInSecond, string Subtitle, string SubtitleType);

    public class EpisodeAddRequestValidator : AbstractValidator<EpisodeAddRequest>
    {
        public EpisodeAddRequestValidator(ListeningDbContext ctx)
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name.Chinese).NotNull().Length(1, 200);
            RuleFor(x => x.Name.English).NotNull().Length(1, 200);
            //验证AlbumId是否存在
            RuleFor(x => x.AlbumId).MustAsync(async (aId, ct) => await ctx.Albums.AnyAsync(a => a.Id == aId))
                .WithMessage(c => $"AlbumId={c.AlbumId}不存在");
            RuleFor(x => x.AudioUrl).NotEmptyUri().Length(1, 1000);
            RuleFor(x => x.DurationInSecond).GreaterThan(0);
            RuleFor(x => x.SubtitleType).NotEmpty().Length(1, 10);
            RuleFor(x => x.Subtitle).NotEmpty().NotEmpty();
        }
    }
}
