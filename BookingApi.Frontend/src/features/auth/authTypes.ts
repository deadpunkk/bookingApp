export type UserRole = 'Admin' | 'Moderator' | 'Viewer';

export type ApiUserRole = UserRole | 0 | 1 | 2;

export type ApiUser = {
  id: number;
  login: string;
  role: ApiUserRole;
};

export type AuthUser = {
  id: number;
  login: string;
  role: UserRole;
};

export type LoginRequest = {
  login: string;
  password: string;
};

export type LoginResponse = {
  accessToken: string;
  user: ApiUser;
};

export type LoginResult = {
  token: string;
  user: AuthUser;
};
