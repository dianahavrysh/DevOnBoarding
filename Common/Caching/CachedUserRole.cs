namespace Common.Caching;

/// <summary>Cached part of a user needed for authorization: the role id.</summary>
public sealed record CachedUserRole(byte RolePK);
