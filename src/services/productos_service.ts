import { obtener_api_url } from '../config/debug'; // Asegúrate de que apunte bien a tu archivo debug.ts
import { use_sesion_store } from '@/stores/sesion_store';

export async function api_obtener_resumen_productos() {
    const sesion = use_sesion_store();
    const url = `${obtener_api_url()}/productos/resumen`;
    
    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${sesion.token}`,
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
    const sesion = use_sesion_store();
    const response = await fetch(`${obtener_api_url()}/productos`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${sesion.token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(producto)
    });
    return await response.json();
}

export async function api_modificar_producto(id: number, producto: any) {
    const sesion = use_sesion_store();
    const response = await fetch(`${obtener_api_url()}/productos/${id}`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${sesion.token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(producto)
    });
    return await response.json();
}

export async function api_dar_de_baja_producto(id: number) {
    const sesion = use_sesion_store();
    const response = await fetch(`${obtener_api_url()}/productos/${id}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${sesion.token}`
        }
    });
    return await response.json();
}