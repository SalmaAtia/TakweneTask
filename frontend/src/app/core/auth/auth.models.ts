// Mirrors TrackManagement.Api/Auth/Dtos.

/** POST /api/auth/login. */
export interface LoginRequest {
  username: string;
  password: string;
}

/** POST /api/auth/login response. expiresAt is ISO 8601. */
export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}
