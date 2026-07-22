const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080')
  .replace(/\/$/, '');

export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

type ApiClientOptions = RequestInit & {
  accessToken?: string;
};

export async function apiClient<TResponse>(
  path: string,
  options: ApiClientOptions = {},
): Promise<TResponse> {
  const { accessToken, headers, body, ...fetchOptions } = options;
  const requestHeaders = new Headers(headers);

  if (body !== undefined && body !== null && !requestHeaders.has('Content-Type')) {
    requestHeaders.set('Content-Type', 'application/json');
  }

  if (accessToken) {
    requestHeaders.set('Authorization', `Bearer ${accessToken}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...fetchOptions,
    body,
    headers: requestHeaders,
  });

  if (!response.ok) {
    throw new ApiError(await readErrorMessage(response), response.status);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return response.json() as Promise<TResponse>;
}

async function readErrorMessage(response: Response): Promise<string> {
  const responseText = await response.text().catch(() => '');

  if (!responseText) {
    return `HTTP error ${response.status}`;
  }

  try {
    const body = JSON.parse(responseText) as {
      error?: unknown;
      message?: unknown;
      title?: unknown;
    };

    for (const value of [body.message, body.error, body.title]) {
      if (typeof value === 'string' && value.trim()) {
        return value;
      }
    }
  } catch {
    return responseText;
  }

  return `HTTP error ${response.status}`;
}
