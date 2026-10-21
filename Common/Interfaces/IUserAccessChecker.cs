using Common.DTOs;
using Common.Enums;

namespace Common.Interfaces;

/// <summary>
/// Decides whether the current user may perform an operation on a target user.
/// </summary>
public interface IUserAccessChecker {
    bool IsAllowed(TargetUserInfo target, UserOperation operation);
}
