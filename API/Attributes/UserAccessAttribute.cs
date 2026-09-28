using API.Filters;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Attributes;

public class UserAccessAttribute : TypeFilterAttribute {
    public UserAccessAttribute(UserOperation operation) : base(typeof(UserAccessFilter)) {
        Arguments = new object[] { operation };
    }
}
