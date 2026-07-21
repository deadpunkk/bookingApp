const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080').replace(/\/$/, '');

export class ApiError extends Error{
    constructor(
        message: string,
        status: number
    ){
        super(message);
        this.name = "ApiError";
    }
}

type ApiClientOptions = RequestInit & {
    accessToken?: string;
};

export async function ApiClient<TResponse>(
    path: string,
    options: ApiClientOptions = {},
    ) : Promise<TResponse>{

        const {accessToken, headers, body, ...fetchOptions} = options;

        const requestHeaders = new Headers(headers);

        if (body && !requestHeaders.has('Content-Type')){
            requestHeaders.set('Content-Type', 'application/json');
        }

        if (accessToken){
            requestHeaders.set('Authorization', `Bearer ${accessToken}`);
        }

        const response = await fetch(`${API_BASE_URL}${path}`, {
            ...fetchOptions,
            body,
            headers: requestHeaders
        });

        if (!response.ok){
            const message = await readErrorMessage(response);
            throw new ApiError(message, response.status);
        }

        if (response.status === 204){
            return undefined as TResponse;
        }

        return await response.json() as TResponse;
    }
    
    async function readErrorMessage(response: Response) : Promise<string>{
        const contentType = response.headers.get('content-type');

        if (contentType?.includes('application/json')){
            const body = await response.json().catch(() => null) as {
                error?: string;
                message?: string;
                title?: string;
            } | null;

            return body?.message
            ?? body?.error
            ?? body?.title
            ?? `HTTP error ${response.status}`;
        }
     
        const text = await response.text().catch(() => '');

        return text || `HTTP error ${response.status}`;
    }

 
    

