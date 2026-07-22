import { apiClient } from '../../shared/api/apiClient';

import type { LoginRequest, LoginResponse, LoginResult } from './authTypes';
import { mapApiUserToAuthUser } from './roleMapper';

export async function login(request: LoginRequest): Promise<LoginResult> {
  const data = await apiClient<LoginResponse>('/users/login', {
    method: 'POST',
    body: JSON.stringify(request),
  });

  if (typeof data?.accessToken !== 'string' || !data.accessToken.trim()) {
    throw new Error('Backend did not return access token');
  }

  if (!data.user) {
    throw new Error('Backend did not return user');
  }

  return {
    token: data.accessToken.trim(),
    user: mapApiUserToAuthUser(data.user),
  };
}
