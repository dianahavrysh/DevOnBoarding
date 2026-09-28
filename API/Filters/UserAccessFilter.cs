using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Common.DTOs;
using Common.Enums;
using Common.Interfaces;
using Common.Auth.Authorization;

namespace API.Filters;

/// <summary>
/// Action filter that authorizes user-management operations before the action runs.
/// For Create, the target role comes from the request DTO.
/// For Edit/Delete, the target user is fetched by id and its role is used for the check.
/// </summary>
public class UserAccessFilter : IAsyncActionFilter {
    private readonly UserOperation _operation;

    public UserAccessFilter(UserOperation operation) {
        _operation = operation;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next) {
        var authorizationService = context.HttpContext.RequestServices
            .GetRequiredService<IAuthorizationService>();

        TargetUserInfo target;

        if (_operation == UserOperation.Create) {
            if (!context.ActionArguments.TryGetValue("dto", out var dtoObj)
                || dtoObj is not UserCreateUpdateDTO createDto) {
                context.Result = new BadRequestResult();
                return;
            }

            target = new TargetUserInfo(
                Guid.Empty,
                createDto.RoleId);
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

            target = new TargetUserInfo(
                existing.UserPK,
                existing.RoleId);

            context.HttpContext.Items["TargetUser"] = existing;
        }

        var authResult = await authorizationService.AuthorizeAsync(
            context.HttpContext.User,
            target,
            new UserAccessRequirement(_operation));

        if (!authResult.Succeeded) {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }

    private static bool TryResolveTargetId(
        ActionExecutingContext context,
        out Guid id) {
        if (context.ActionArguments.TryGetValue("id", out var idObj)
            && idObj is Guid routeId) {
            id = routeId;
            return true;
        }

        if (context.ActionArguments.TryGetValue("dto", out var dtoObj)
            && dtoObj is UserCreateUpdateDTO dto) {
            id = dto.UserPK;
            return true;
        }

        id = default;
        return false;
    }
}
