using System;
using Common.Enums;
using Common.Interfaces;

namespace Common.Contexts;
/// <inheritdoc />
public sealed class CurrentUserContext : ICurrentUserContext {
    private (Guid UserPK, Role Role)? _value;
    /// <inheritdoc />
    public bool IsInitialized => _value is not null;
    /// <inheritdoc />
    public Guid UserPK => Value.UserPK;
    /// <inheritdoc />
    public Role Role => Value.Role;

    /// <summary>
    /// Sets the current user context for the ongoing request.
    /// </summary>
    /// <param name="userPK">The primary key of the user.</param>
    /// <param name="role">The role of the user.</param>
    public void Set(Guid userPK, Role role) => _value = (userPK, role);

    private (Guid UserPK, Role Role) Value =>
        _value ?? throw new InvalidOperationException(
            "Current user is not initialized for this request.");
}
