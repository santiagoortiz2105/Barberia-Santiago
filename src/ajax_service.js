const API_BASE_URL = 'http://localhost:5211';

export async function obtenerResumenCatalogo() {
    try {
        const response = await fetch(`${API_BASE_URL}/productos/resumen`);
        
        if (!response.ok) {
            throw new Error(`Error HTTP: ${response.status}`);
        }

        const data = await response.json();
        return data;
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
    const response = await fetch(`${API_BASE_URL}${endpoint}`);
    if (!response.ok) throw new Error(`Error en GET ${endpoint}: ${response.status}`);
    return await response.json();
}

export async function ajax_post(endpoint, data) {
    const isFormData = data instanceof FormData;
    const headers = isFormData ? {} : { 'Content-Type': 'application/json' };

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: headers,
        body: isFormData ? data : JSON.stringify(data)
    });

    if (!response.ok) throw new Error(`Error en POST ${endpoint}: ${response.status}`);
    
    const text = await response.text();
    return text ? JSON.parse(text) : {};
}

export async function ajax_put(endpoint, data) {
    const isFormData = data instanceof FormData;
    const headers = isFormData ? {} : { 'Content-Type': 'application/json' };

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'PUT',
        headers: headers,
        body: isFormData ? data : JSON.stringify(data)
    });

    if (!response.ok) throw new Error(`Error en PUT ${endpoint}: ${response.status}`);
    
    const text = await response.text();
    return text ? JSON.parse(text) : {};
}

export async function ajax_delete(endpoint) {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'DELETE'
    });

    if (!response.ok) throw new Error(`Error en DELETE ${endpoint}: ${response.status}`);
    
    const text = await response.text();
    return text ? JSON.parse(text) : {};
}