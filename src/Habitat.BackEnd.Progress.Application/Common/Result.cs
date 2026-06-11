namespace Habitat.BackEnd.Progress.Application.Common;

public class Result
{
    protected Result(ResultStatus status, Error? error = null)
    {
        Status = status;
        Error = error;
    }

    public ResultStatus Status { get; }
    public Error? Error { get; }
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Success() => new(ResultStatus.Success);
    public static Result Validation(string code, string message) => new(ResultStatus.ValidationError, new Error(code, message));
    public static Result Unauthorized(string code = "auth.unauthorized", string message = "Authentication is required to access this resource.") => new(ResultStatus.Unauthorized, new Error(code, message));
    public static Result Forbidden(string code = "auth.forbidden", string message = "You do not have permission to access this resource.") => new(ResultStatus.Forbidden, new Error(code, message));
    public static Result NotFound(string code = "resource.not_found", string message = "The requested resource was not found.") => new(ResultStatus.NotFound, new Error(code, message));
    public static Result Conflict(string code, string message) => new(ResultStatus.Conflict, new Error(code, message));
}

public sealed class Result<T> : Result
{
    private Result(ResultStatus status, T? value = default, Error? error = null) : base(status, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(ResultStatus.Success, value);
    public static new Result<T> Validation(string code, string message) => new(ResultStatus.ValidationError, error: new Error(code, message));
    public static new Result<T> Unauthorized(string code = "auth.unauthorized", string message = "Authentication is required to access this resource.") => new(ResultStatus.Unauthorized, error: new Error(code, message));
    public static new Result<T> Forbidden(string code = "auth.forbidden", string message = "You do not have permission to access this resource.") => new(ResultStatus.Forbidden, error: new Error(code, message));
    public static new Result<T> NotFound(string code = "resource.not_found", string message = "The requested resource was not found.") => new(ResultStatus.NotFound, error: new Error(code, message));
    public static new Result<T> Conflict(string code, string message) => new(ResultStatus.Conflict, error: new Error(code, message));
}
