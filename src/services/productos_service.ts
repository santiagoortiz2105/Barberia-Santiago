import { obtener_api_url } from '../config/debug';
import { use_sesion_store } from '@/stores/sesion_store';

function obtener_token_seguro(): string {
    const sesion = use_sesion_store();
    if (sesion?.token) return sesion.token;

    const posiblesClaves = [
        'capacitor-storage_tudominio_token',
        'token',
        'access_token',
        'auth_token'
    ];

    for (const clave of posiblesClaves) {
        const valor = localStorage.getItem(clave);
        if (valor) {
            return valor.replace(/^["'](.*)["']$/, '$1').trim();
        }
    }
    return '';
}

export async function api_obtener_resumen_productos() {
    const baseUrl = obtener_api_url().replace(/\/+$/, "");
    const url = `${baseUrl}/api/productos/resumen`;
    const token = obtener_token_seguro();
    
    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Authorization': token ? `Bearer ${token}` : '',
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) throw new Error('Error al obtener productos');
        return await response.json();
    } catch (error) {
        console.error("Error en fetch de productos:", error);
        return [];
    }
}

export async function api_crear_producto(producto: any) {
    const baseUrl = obtener_api_url().replace(/\/+$/, "");
    const token = obtener_token_seguro();

    const response = await fetch(`${baseUrl}/api/productos`, {
        method: 'POST',
        headers: {
            'Authorization': token ? `Bearer ${token}` : '',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(producto)
    });
    if (!response.ok) throw new Error('Error al crear producto');
    return await response.json();
}

export async function api_modificar_producto(id: number, producto: any) {
    const baseUrl = obtener_api_url().replace(/\/+$/, "");
    const token = obtener_token_seguro();

    const response = await fetch(`${baseUrl}/api/productos/${id}`, {
        method: 'PUT',
        headers: {
            'Authorization': token ? `Bearer ${token}` : '',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(producto)
    });
    if (!response.ok) throw new Error('Error al modificar producto');
    return await response.json();
}

export async function api_dar_de_baja_producto(id: number) {
    const baseUrl = obtener_api_url().replace(/\/+$/, "");
    const token = obtener_token_seguro();

    const response = await fetch(`${baseUrl}/api/productos/${id}`, {
        method: 'DELETE',
        headers: {
            'Authorization': token ? `Bearer ${token}` : ''
        }
    });
    if (!response.ok) throw new Error('Error al dar de baja el producto');
    return await response.json();
}