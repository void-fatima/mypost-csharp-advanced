namespace MyPost.Application.Abstractions;

public interface ITrackingCodeGenerator
{
    string Create(DateTimeOffset nowUtc);
}
