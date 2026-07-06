import type {LoginRequest, LoginResponse} from './authTypes';
import {mapApiUserToAuthUser} from './roleMapper';

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL|| 'http://localhost:5065').replace(/\/$/, '');

export async function login(request:LoginRequest) {
    const response = await fetch(`${API_BASE_URL}/users/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    });

    if (!response.ok){
        const message = await readErrorMessage(response);
        throw new Error(message);
    }

    const data = (await response.json()) as LoginResponse;

    console.log('LOGIN RESPONSE:', data);

    return{
        token: data.accessToken,
        user: mapApiUserToAuthUser(data.user)
    };
}

async function readErrorMessage(response: Response) : Promise<string> {
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