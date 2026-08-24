namespace MyPost.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
