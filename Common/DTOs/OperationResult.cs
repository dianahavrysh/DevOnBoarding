using Common.Enums;

namespace Common.DTOs;

/// <summary>
/// Represents the result of an operation with a status and an optional value.
/// </summary>
/// <param name="Status">The status of the operation.</param>
/// <param name="Value">The optional value associated with the operation result.</param>
public sealed record OperationResult<T>(OperationStatus Status, T? Value = default) {
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => Status == OperationStatus.Success;

    /// <summary>
    /// Creates an OperationResult<T> that represents a successful operation and contains the specified value.
    /// </summary>
    /// <param name="value">The value to include in the successful OperationResult.</param>
    /// <returns>An OperationResult<T> representing a successful operation that contains the specified value.</returns>
    public static OperationResult<T> Success(T value) => new(OperationStatus.Success, value);
    /// <summary>
    /// Creates an OperationResult<T> representing a NotFound status.
    /// </summary>
    /// <remarks>Use to indicate that the requested resource was not found; no result value is
    /// provided.</remarks>
    /// <returns>An OperationResult<T> whose Status is OperationStatus.NotFound and which contains no value.</returns>
    public static OperationResult<T> NotFound() => new(OperationStatus.NotFound);
    /// <summary>
    /// Creates an OperationResult<T> representing a Forbidden status.
    /// </summary>
    /// <returns>An OperationResult<T> whose Status is OperationStatus.Forbidden and which contains no value.</returns>
    public static OperationResult<T> Forbidden() => new(OperationStatus.Forbidden);
}
