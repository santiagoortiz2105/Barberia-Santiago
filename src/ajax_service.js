const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5211';

function obtenerHeaders(isFormData = false) {
    const headers = isFormData ? {} : { 'Content-Type': 'application/json' };
    const token = localStorage.getItem('token') || localStorage.getItem('access_token') || localStorage.getItem('capacitor-storage_tudominio_token');
    
    if (token) {
        const tokenLimpio = token.replace(/^["']|["']$/g, '').trim();
        headers['Authorization'] = `Bearer ${tokenLimpio}`;
    }
    
    return headers;
}

// Función blindada contra duplicaciones de /api/
function construirUrl(endpoint) {
    let limpio = String(endpoint || '').trim();
    
    // Remueve cualquier combinación inicial de '/api', 'api' o barras para evitar duplicados
    while (limpio.startsWith('/api') || limpio.startsWith('api') || limpio.startsWith('/')) {
        if (limpio.startsWith('/api/')) limpio = limpio.substring(5);
        else if (limpio.startsWith('api/')) limpio = limpio.substring(4);
        else if (limpio.startsWith('/api')) limpio = limpio.substring(4);
        else if (limpio.startsWith('api')) limpio = limpio.substring(3);
        else if (limpio.startsWith('/')) limpio = limpio.substring(1);
        else break;
        limpio = limpio.trim();
    }

    const baseClean = API_BASE_URL.replace(/\/+$/, '').replace(/\/api$/, '');
    return `${baseClean}/api/${limpio}`;
}

export async function obtenerResumenCatalogo() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/productos/resumen`);
        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);
        return await response.json();
    } catch (error) {
        console.error('Error al conectar con la API de Barbería:', error);
        return null;
    }
}

export async function cargarServiciosEnPantalla(containerId = 'servicios-container') {
    const contenedor = document.getElementById(containerId);
    if (!contenedor) return;

    contenedor.innerHTML = '<p>Cargando servicios...</p>';
    const datos = await obtenerResumenCatalogo();

    if (!datos || !datos.productos || datos.productos.length === 0) {
        contenedor.innerHTML = '<p>No hay servicios disponibles por el momento.</p>';
        return;
    }

    contenedor.innerHTML = datos.productos.map(prod => `
        <div class="card-servicio" style="border: 1px solid #ccc; padding: 15px; margin: 10px 0; border-radius: 8px;">
            <h3>${prod.nombre}</h3>
            <p>${prod.descripcion || 'Sin descripción'}</p>
            <p><strong>Precio:</strong> $${prod.precio}</p>
            <span style="color: ${prod.disponible ? 'green' : 'red'}; font-weight: bold;">
                ${prod.disponible ? 'Disponible' : 'No disponible'}
            </span>
        </div>
    `).join('');
}

export async function ajax_get(endpoint) {
    const url = construirUrl(endpoint);
    const response = await fetch(url, {
        method: 'GET',
        headers: obtenerHeaders(false)
    });
    if (!response.ok) throw new Error(`Error en GET ${endpoint}: ${response.status}`);
    const text = await response.text();
    return text ? JSON.parse(text) : {};
}

export async function ajax_post(endpoint, data) {
    const url = construirUrl(endpoint);
    const isFormData = data instanceof FormData;
    const headers = obtenerHeaders(isFormData);

    const response = await fetch(url, {
        method: 'POST',
        headers: headers,
        body: isFormData ? data : JSON.stringify(data)
    });

    if (!response.ok) throw new Error(`Error en POST ${endpoint}: ${response.status}`);
    const text = await response.text();
    return text ? JSON.parse(text) : {};
}

export async function ajax_put(endpoint, data) {
    const url = construirUrl(endpoint);
    const isFormData = data instanceof FormData;
    const headers = obtenerHeaders(isFormData);

    const response = await fetch(url, {
        method: 'PUT',
        headers: headers,
        body: isFormData ? data : JSON.stringify(data)
    });

    if (!response.ok) throw new Error(`Error en PUT ${endpoint}: ${response.status}`);
    const text = await response.text();
    return text ? JSON.parse(text) : {};
}

export async function ajax_patch(endpoint, data) {
    const url = construirUrl(endpoint);
    const isFormData = data instanceof FormData;
    const headers = obtenerHeaders(isFormData);

    const response = await fetch(url, {
        method: 'PATCH',
        headers: headers,
        body: isFormData ? data : JSON.stringify(data)
    });

    if (!response.ok) throw new Error(`Error en PATCH ${endpoint}: ${response.status}`);
    const text = await response.text();
    return text ? JSON.parse(text) : {};
}

export async function ajax_delete(endpoint) {
    const url = construirUrl(endpoint);
    const response = await fetch(url, {
        method: 'DELETE',
        headers: obtenerHeaders(false)
    });

    if (!response.ok) throw new Error(`Error en DELETE ${endpoint}: ${response.status}`);
    const text = await response.text();
    return text ? JSON.parse(text) : {};
}

// Alias de compatibilidad
export const solicitud_get = ajax_get;
export const solicitud_post = ajax_post;
export const solicitud_put = ajax_put;
export const solicitud_patch = ajax_patch;
export const solicitud_delete = ajax_delete;

export default {
    obtenerResumenCatalogo,
    cargarServiciosEnPantalla,
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