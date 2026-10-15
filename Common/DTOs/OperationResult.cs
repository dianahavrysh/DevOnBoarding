using Common.Enums;

namespace Common.DTOs;

public sealed record OperationResult<T>(OperationStatus Status, T? Value = default) {
    public bool IsSuccess => Status == OperationStatus.Success;

    public static OperationResult<T> Success(T value) => new(OperationStatus.Success, value);
    public static OperationResult<T> NotFound() => new(OperationStatus.NotFound);
    public static OperationResult<T> Forbidden() => new(OperationStatus.Forbidden);
}
