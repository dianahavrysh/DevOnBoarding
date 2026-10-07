using System;
using Common.Enums;

namespace Common.Interfaces;
/// <summary>
/// Represents the current user in the application, providing access to their primary key and role.
/// </summary>
public interface ICurrentUserContext {
    /// <summary>
    /// Indicates whether the current user context has been initialized for the request.
    /// </summary>
    bool IsInitialized { get; }
    /// <summary>
    /// Gets the primary key of the current user.
    /// </summary>
    Guid UserPK { get; }
    /// <summary>
    /// Gets the role of the current user.
    /// </summary>
    Role Role { get; }
}
