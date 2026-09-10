import { obtener_api_url } from "@/config/debug";

const api_url = obtener_api_url();

function construir_url(endpoint: string): string {
    if (!api_url) throw new Error("No se configuró la URL de la API.");
    return `${api_url}/${String(endpoint).replace(/^\/+/, "")}`;
}

export async function ajax_get(endpoint: string): Promise<any> {
    const url = construir_url(endpoint);
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 8000);

    try {
        const response = await fetch(url, {
            signal: controller.signal
        });
        clearTimeout(timeoutId);

        if (!response.ok) {
            const errorBody = await response.json().catch(() => ({}));
            throw { status: response.status, ...errorBody };
        }
        return await response.json();
    } catch (error: any) {
        clearTimeout(timeoutId);
        if (error.name === "AbortError") {
            throw new Error("La solicitud tardó demasiado en responder.");
        }
        throw error;
    }
}

export async function ajax_post(endpoint: string, data: any): Promise<any> {
    const url = construir_url(endpoint);
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 8000);

    try {
        const response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data),
            signal: controller.signal
        });
        clearTimeout(timeoutId);

        if (!response.ok) {
            const errorBody = await response.json().catch(() => ({}));
            throw { status: response.status, ...errorBody };
        }
        return await response.json();
    } catch (error: any) {
        clearTimeout(timeoutId);
        if (error.name === "AbortError") {
            throw new Error("La solicitud tardó demasiado en responder.");
        }
        throw error;
    }
}

export async function ajax_put(endpoint: string, data: any): Promise<any> {
    const url = construir_url(endpoint);
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 8000);

    try {
        const response = await fetch(url, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data),
            signal: controller.signal
        });
        clearTimeout(timeoutId);

        if (!response.ok) {
            const errorBody = await response.json().catch(() => ({}));
            throw { status: response.status, ...errorBody };
        }
        return await response.json();
    } catch (error: any) {
        clearTimeout(timeoutId);
        if (error.name === "AbortError") {
            throw new Error("La solicitud tardó demasiado en responder.");
        }
        throw error;
    }
}

export async function ajax_delete(endpoint: string): Promise<any> {
    const url = construir_url(endpoint);
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 8000);

    try {
        const response = await fetch(url, {
            method: "DELETE",
            signal: controller.signal
        });
        clearTimeout(timeoutId);

        if (!response.ok) {
            const errorBody = await response.json().catch(() => ({}));
            throw { status: response.status, ...errorBody };
        }
        return await response.json();
    } catch (error: any) {
        clearTimeout(timeoutId);
        if (error.name === "AbortError") {
            throw new Error("La solicitud tardó demasiado en responder.");
        }
        throw error;
    }
}