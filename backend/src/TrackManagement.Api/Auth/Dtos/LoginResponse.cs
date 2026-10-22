namespace TrackManagement.Api.Auth.Dtos;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);
