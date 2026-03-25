namespace AbbaFleet.Shared;

public sealed class Result<T>
{
    private Result(T value) { Value = value; }

    private Result(string error) { Error = error; }

    public T? Value { get; }
    public string? Error { get; }
    public bool Succeeded => Error is null;

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }

    public static implicit operator Result<T>(string error)
    {
        return new Result<T>(error);
    }

    public static implicit operator T(Result<T> result)
    {
        return result.Value!;
    }
}
