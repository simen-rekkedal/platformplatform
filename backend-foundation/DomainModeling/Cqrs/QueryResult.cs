using JetBrains.Annotations;
using PlatformPlatform.Foundation.DomainModeling.Validation;

namespace PlatformPlatform.Foundation.DomainModeling.Cqrs;

/// <summary>
///     All queries should return a <see cref="QueryResult{T}" />. This is used to indicate if the query was successful
///     or not. If the query was successful, the <see cref="QueryResult{T}" /> will contain the result of the query.
///     If the query was not successful, it will contain a <see cref="ErrorMessage" />
/// </summary>
public readonly struct QueryResult<T>
{
    private QueryResult(bool isSuccess, T value, ErrorMessage errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public T Value { get; }

    public ErrorMessage ErrorMessage { get; }

    /// <summary>
    ///     Use this to indicate a error when doing a query.
    /// </summary>
    public static QueryResult<T> Failure(string message)
    {
        return new QueryResult<T>(false, default!, new ErrorMessage(message));
    }

    /// <summary>
    ///     Use this to indicate a successful query. There is a implicit conversion from T to
    ///     <see cref="QueryResult{T}" />, so you can also just return T from a Query handler.
    /// </summary>
    [UsedImplicitly]
    public static QueryResult<T> Success(T value)
    {
        return new QueryResult<T>(true, value, default!);
    }

    /// <summary>
    ///     This is the implicit conversion from T to <see cref="QueryResult{T}" />. This is used to easily return a
    ///     successful <see cref="QueryResult{T}" /> from a query handler.
    /// </summary>
    public static implicit operator QueryResult<T>(T value)
    {
        return Success(value);
    }
}