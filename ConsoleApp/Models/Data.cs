namespace ConsoleApp.Models;

public abstract class Data<TKey>
{
    public TKey Key { get; set; } = default!;
}
