import { useGlobalState } from "../store/global_state";

class ApiService {
    private baseUri: string;

    constructor(baseUri: string) {
        this.baseUri = baseUri;
    }

    private async request(method: string, endpoint: string, payload?: any, raw: boolean = false) {
        const url = `${this.baseUri}/${endpoint}`;
        const headers: HeadersInit = new Headers();
        headers.set('Accept', 'application/json'); // Expect JSON by default, but allow other types

        if (payload) {
            headers.set('Content-Type', 'application/json');
        }

        const options: RequestInit = {
            method: method,
            headers: headers,
            body: payload ? JSON.stringify(payload) : null
        };

        try {
            const response = await fetch(url, options);
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }

            if (raw) {
                return response; // Return raw response for streaming
            }

            // Check the Content-Type of the response to determine how to parse it
            const contentType = response.headers.get('Content-Type');
            if (contentType?.includes('application/json')) {
                return response.json(); // Parse as JSON
            } else if (contentType?.includes('text/plain')) {
                return response.text(); // Parse as text/plain
            } else {
                return response.blob(); // Fallback or for binary data
            }
        } catch (error: any) {
            // Centralized error handling logic
            console.error(error);
            useGlobalState.getState().addAlert(error.message || 'An error occurred', 'error');
            throw error;
        }
    }

    public async get(endpoint: string) {
        return await this.request('GET', endpoint);
    }

    public async post(endpoint: string, data: any, raw: boolean = false) {
        return await this.request('POST', endpoint, data, raw);
    }

    public async delete(endpoint: string) {
        return await this.request('DELETE', endpoint);
    }
}

export const ouranos_api = new ApiService(`${process.env.NEXT_PUBLIC_API_HOST}`);