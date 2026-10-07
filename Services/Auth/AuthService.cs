using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Common.Caching;
using Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Services.Auth;

internal class AuthService : IAuthService {
    private readonly IUsersManager _usersManager;
    private readonly IUserRoleCacheService _roleCache;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsersManager usersManager,
        IUserRoleCacheService roleCache,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        ILogger<AuthService> logger) {
        _usersManager = usersManager;
        _roleCache = roleCache;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<string?> AuthenticateAsync(string email, string password) {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) {
            _logger.LogWarning("Authentication attempt with empty username or password.");
            return null;
        }

        var user = await _usersManager.GetByEmailAsync(email);

        if (user is null) {
            _logger.LogWarning("Authentication failed: User not found for username '{Username}'.", email);
            return null;
        }

        if (user.Password != password) {
            _logger.LogWarning("Authentication failed: Invalid password for username '{Username}'.", email);
            return null;
        }

        if (!user.ActiveStatus) {
            _logger.LogWarning("Authentication failed: User '{Username}' is inactive.", email);
            return null;
        }

        if (string.IsNullOrWhiteSpace(user.RoleName)) {
            _logger.LogError("Authentication failed: User '{Username}' has no role assigned.", email);
            return null;
        }

        await _roleCache.SetAsync(user.UserPK, _mapper.Map<CachedUserRole>(user));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserPK.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.RoleName)
        };

        var token = _jwtTokenService.GenerateToken(claims);

        _logger.LogInformation("User '{Username}' authenticated successfully.", email);
        return token;
    }
}
