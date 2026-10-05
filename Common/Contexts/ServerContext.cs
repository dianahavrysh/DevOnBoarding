using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Common.Contexts;

/// <summary>
/// Ambient information about the user who issued the current request.
/// Backed by AsyncLocal, so values never leak between concurrent requests.
/// Populated only by UserCachingMiddleware.
/// </summary>
public static class ServerContext {
    private static readonly AsyncLocal<CurrentUser?> Holder = new();
    /// <summary>
    /// Indicates whether the current request has been authenticated and the ServerContext has been initialized.
    /// </summary>
    public static bool IsAuthenticated => Holder.Value is not null;
    /// <summary>
    /// Gets the primary key of the user who issued the current request.
    /// </summary>
    public static Guid UserPK => Current.UserPK;
    /// <summary>
    /// Gets the role primary key of the user who issued the current request.
    /// </summary>
    public static byte RolePK => Current.RolePK;

    private static CurrentUser Current =>
        Holder.Value ?? throw new InvalidOperationException(
            "ServerContext is not initialized for the current request.");

    /// <summary>Sets the current user until the returned scope is disposed.</summary>
    public static IDisposable Begin(Guid userPK, byte rolePK) {
        var previous = Holder.Value;
        Holder.Value = new CurrentUser(userPK, rolePK);
        return new Scope(previous);
    }

    private sealed record CurrentUser(Guid UserPK, byte RolePK);

    private sealed class Scope : IDisposable {
        private readonly CurrentUser? _previous;

        public Scope(CurrentUser? previous) => _previous = previous;

        public void Dispose() => Holder.Value = _previous;
    }
}
