using System;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Common.Enums;

namespace Common.Auth;

/// <summary>
/// Authorization requirement for evaluating user access to perform specific operations.
/// </summary>
public class UserAccessRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the operation to be performed.
    /// </summary>
    public UserOperation Operation { get; }

    /// <summary>
    /// Creates a new authorization requirement for the specified operation.
    /// </summary>
    public UserAccessRequirement(UserOperation operation) => Operation = operation;
}
