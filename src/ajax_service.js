// URL base de tu API corriendo en .NET
const API_BASE_URL = 'http://localhost:5211/api';

/**
 * Obtiene el resumen del catálogo (categorías y productos/servicios)
 */
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

/**
 * Renderiza los servicios en el contenedor HTML asignado
 * @param {string} containerId - ID del elemento HTML donde se mostrarán las tarjetas
 */
export async function cargarServiciosEnPantalla(containerId = 'servicios-container') {
    const contenedor = document.getElementById(containerId);
    if (!contenedor) return;

    contenedor.innerHTML = '<p>Cargando servicios...</p>';

    const datos = await obtenerResumenCatalogo();

    if (!datos || !datos.productos || datos.productos.length === 0) {
        contenedor.innerHTML = '<p>No hay servicios disponibles por el momento.</p>';
        return;
    }

    // Dibujar las tarjetas de los productos/servicios
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