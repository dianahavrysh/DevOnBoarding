using System;
using System.Threading;
using Common.Enums;

namespace Common.Contexts;
/// <summary>
/// Provides access to the current user's context (user PK and role) for the duration of a request.
/// </summary>
public static class ServerContext {
    private static readonly AsyncLocal<CurrentUser?> Holder = new();
    /// <summary>
    /// Indicates whether the current request has an authenticated user context.
    /// </summary>
    public static bool IsAuthenticated => Holder.Value is not null;
    /// <summary>
    /// Returns the current user's PK, or throws if the context is not initialized.
    /// </summary>
    public static Guid UserPK => Current.UserPK;
    /// <summary>
    /// Returns the current user's role, or throws if the context is not initialized.
    /// </summary>
    public static Role Role => Current.Role;
    /// <summary>Raw role id, for DAL calls that still take a byte.</summary>
    public static byte RolePK => (byte)Current.Role;

    private static CurrentUser Current =>
        Holder.Value ?? throw new InvalidOperationException(
            "ServerContext is not initialized for the current request.");
    /// <summary>
    /// Begins a new user context for the current request, returning an 
    /// IDisposable that restores the previous context when disposed.
    /// </summary>
    /// <param name="userPK"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public static IDisposable Begin(Guid userPK, Role role) {
        var previous = Holder.Value;
        Holder.Value = new CurrentUser(userPK, role);
        return new Scope(previous);
    }

    private sealed record CurrentUser(Guid UserPK, Role Role);

    private sealed class Scope : IDisposable {
        private readonly CurrentUser? _previous;
        public Scope(CurrentUser? previous) => _previous = previous;
        public void Dispose() => Holder.Value = _previous;
    }
}
