using System.Security.Cryptography;
using MyPost.Application.Abstractions;

namespace MyPost.Infrastructure;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

internal sealed class TrackingCodeGenerator : ITrackingCodeGenerator
{
    public string Create(DateTimeOffset nowUtc) =>
        $"MP-{nowUtc:yyMMdd}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(4))}";
}
