import { obtener_api_url } from "@/config/debug";
import { use_sesion_store } from "@/stores/sesion_store";
import { SecureStorage } from '@aparajita/capacitor-secure-storage';

const CLAVE_TOKEN = 'tudominio_token';

function construir_url(endpoint: string): string {
    const base = obtener_api_url().replace(/\/+$/, "");
    let limpio = String(endpoint).replace(/^\/+/, "");
    
    if (limpio.startsWith("api/")) {
        limpio = limpio.substring(4);
    }
    
    const baseClean = base.endsWith("/api") ? base.slice(0, -4) : base;
    return `${baseClean}/api/${limpio}`;
}

async function obtener_token_seguro(): Promise<string> {
    try {
        const sesion = use_sesion_store();
        if (sesion && sesion.token) {
            let t = String(sesion.token).trim().replace(/^["']|["']$/g, '');
            if (t.length > 10) return t;
        }
    } catch (e) {}

  
    try {
        const claves = ['token', 'tudominio_token', 'access_token', 'auth_token'];
        for (const clave of claves) {
            const val = localStorage.getItem(clave);
            if (val) {
                let t = val.trim().replace(/^["']|["']$/g, '');
                if (t.startsWith("{") || t.startsWith("[")) {
                    const parsed = JSON.parse(t);
                    t = parsed.token || parsed.value || parsed.access_token || t;
                }
                if (typeof t === 'string' && t.length > 10) {
                    console.log(`[DEBUG AJAX] Token obtenido de localStorage con clave: '${clave}'`);
                    return t;
                }
            }
        }
    } catch (e) {}

    console.warn("[DEBUG AJAX] Token recibido: ¡ESTÁ VACÍO!");
    return '';
}

async function obtener_headers(incluirBody: boolean = false): Promise<Record<string, string>> {
    const headers: Record<string, string> = {};
    if (incluirBody) {
        headers["Content-Type"] = "application/json";
    }

    const token = await obtener_token_seguro();
    console.log("[DEBUG AJAX] Token recuperado para la petición:", token ? token.substring(0, 15) + "..." : "¡ESTÁ VACÍO!");
    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    } else {
        console.warn("[AJAX] ⚠️ Advertencia: No hay token disponible para inyectar.");
    }

    return headers;
}

export async function ajax_get(endpoint: string): Promise<any> {
    const url = construir_url(endpoint);
    const headers = await obtener_headers(false);
    const response = await fetch(url, { headers });
    if (!response.ok) {
        const errorBody = await response.json().catch(() => ({}));
        throw { status: response.status, ...errorBody };
    }
    return await response.json();
}

export async function ajax_post(endpoint: string, data: any): Promise<any> {
    const url = construir_url(endpoint);
    const headers = await obtener_headers(true);
    const response = await fetch(url, {
        method: "POST",
        headers,
        body: JSON.stringify(data)
    });
    if (!response.ok) {
        const errorBody = await response.json().catch(() => ({}));
        throw { status: response.status, ...errorBody };
    }
    return await response.json();
}

export async function ajax_put(endpoint: string, data: any): Promise<any> {
    const url = construir_url(endpoint);
    const headers = await obtener_headers(true);
    const response = await fetch(url, {
        method: "PUT",
        headers,
        body: JSON.stringify(data)
    });
    if (!response.ok) {
        const errorBody = await response.json().catch(() => ({}));
        throw { status: response.status, ...errorBody };
    }
    return await response.json();
}

export async function ajax_patch(endpoint: string, data: any): Promise<any> {
    const url = construir_url(endpoint);
    const headers = await obtener_headers(true);
    const response = await fetch(url, {
        method: "PATCH",
        headers,
        body: JSON.stringify(data)
    });
    if (!response.ok) {
        const errorBody = await response.json().catch(() => ({}));
        throw { status: response.status, ...errorBody };
    }
    return await response.json();
}

export async function ajax_delete(endpoint: string): Promise<any> {
    const url = construir_url(endpoint);
    const headers = await obtener_headers(false);
    const response = await fetch(url, { headers });
    if (!response.ok) {
        const errorBody = await response.json().catch(() => ({}));
        throw { status: response.status, ...errorBody };
    }
    return await response.json();
}

export const solicitud_get = ajax_get;
export const solicitud_post = ajax_post;
export const solicitud_put = ajax_put;
export const solicitud_patch = ajax_patch;
export const solicitud_delete = ajax_delete;

export default {
    ajax_get,
    ajax_post,
    ajax_put,
    ajax_patch,
    ajax_delete,
    solicitud_get,
    solicitud_post,
    solicitud_put,
    solicitud_patch,
    solicitud_delete
};