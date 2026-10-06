using System;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Extensions;
using Common.Auth.Authorization;
using Common.DTOs;
using Common.Enums;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Filters;

/// <summary>
/// Action filter that authorizes user-management operations before the action runs.
/// Create: the target role comes from the request DTO.
/// Edit/Delete: the target user is fetched by id and its current role is used for the check.
/// Edit with a role change: the new role is additionally checked as <see cref="UserOperation.AssignRole"/>.
/// </summary>
public class UserAccessFilter : IAsyncActionFilter {
    private readonly UserOperation _operation;

    public UserAccessFilter(UserOperation operation) {
        _operation = operation;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next) {
        var authorization = context.HttpContext.RequestServices
            .GetRequiredService<IAuthorizationService>();
        var principal = context.HttpContext.User;

        TargetUserInfo target;

        if (_operation == UserOperation.Create) {
            if (!TryGetDto(context, out var createDto)) {
                context.Result = new BadRequestResult();
                return;
            }

            target = new TargetUserInfo(Guid.Empty, createDto.RolePK);
        }
        else {
            if (!TryResolveTargetId(context, out var targetId)) {
                context.Result = new BadRequestResult();
                return;
            }

            var service = context.HttpContext.RequestServices
                .GetRequiredService<IUsersService>();

            var existing = await service.GetByPKAsync(targetId);

            if (existing is null) {
                context.Result = new NotFoundResult();
                return;
            }

            target = new TargetUserInfo(existing.UserPK, existing.RolePK);
            context.HttpContext.SetTargetUser(existing);
        }

        if (!await IsAllowedAsync(authorization, principal, target, _operation)) {
            context.Result = new ForbidResult();
            return;
        }

        // Role change on edit: the requester must also be allowed to assign the NEW role.
        // Without this, a user could edit themselves and set RolePK = Administrator.
        if (_operation == UserOperation.Edit
            && TryGetDto(context, out var editDto)
            && editDto.RolePK != target.RolePK) {
            var newRoleTarget = target with { RolePK = editDto.RolePK };

            if (!await IsAllowedAsync(authorization, principal, newRoleTarget, UserOperation.AssignRole)) {
                context.Result = new ForbidResult();
                return;
            }
        }

        await next();
    }

    private static async Task<bool> IsAllowedAsync(
        IAuthorizationService authorization,
        ClaimsPrincipal principal,
        TargetUserInfo target,
        UserOperation operation) {
        var result = await authorization.AuthorizeAsync(
            principal,
            target,
            new UserAccessRequirement(operation));

        return result.Succeeded;
    }

    private static bool TryGetDto(
        ActionExecutingContext context,
        out UserCreateUpdateDTO dto) {
        if (context.ActionArguments.TryGetValue("dto", out var obj)
            && obj is UserCreateUpdateDTO value) {
            dto = value;
            return true;
        }

        dto = null!;
        return false;
    }

    /// <summary>
    /// Resolves the id of the user being acted on. When both a route id and a DTO are present,
    /// they must refer to the same user, otherwise the check could be done for one user
    /// while the service modifies another.
    /// </summary>
    private static bool TryResolveTargetId(
        ActionExecutingContext context,
        out Guid id) {
        var hasRouteId = context.ActionArguments.TryGetValue("id", out var idObj)
            && idObj is Guid;
        var hasDto = TryGetDto(context, out var dto);

        if (hasRouteId && hasDto && (Guid)idObj! != dto.UserPK) {
            id = default;
            return false;
        }

        if (hasRouteId) {
            id = (Guid)idObj!;
            return true;
        }

        if (hasDto) {
            id = dto.UserPK;
            return true;
        }

        id = default;
        return false;
    }
}
