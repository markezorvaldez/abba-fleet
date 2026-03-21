namespace AbbaFleet.Shared;

public sealed class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool Succeeded => Error is null;

    private Result(T value) { Value = value; }
    private Result(string error) { Error = error; }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(string error) => new(error);
}
