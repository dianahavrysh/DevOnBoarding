using Common.DTOs;
using Microsoft.AspNetCore.Http;

namespace API.Extensions;

public static class HttpContextExtensions {
    private const string TargetUserKey = "TargetUser";

    public static void SetTargetUser(this HttpContext context, UserDTO user) =>
        context.Items[TargetUserKey] = user;

    public static UserDTO? GetTargetUser(this HttpContext context) =>
        context.Items.TryGetValue(TargetUserKey, out var value) ? value as UserDTO : null;
}
