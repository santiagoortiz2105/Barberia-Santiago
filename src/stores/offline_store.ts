import { defineStore } from 'pinia';
import { ref } from 'vue';
import ajax_service from '../ajax_service';

export const use_offline_store = defineStore('offline', () => {
  const sinConexion = ref(!navigator.onLine);
  
  // Cargar la cola guardada previamente en localStorage
  const colaCambios = ref<any[]>(
    JSON.parse(localStorage.getItem('cola_turnos') || '[]')
  );

  // Caché local de barberos para modo offline
  const barberosCache = ref<any[]>(
    JSON.parse(localStorage.getItem('cached_barberos') || '[]')
  );

  // Guardar la cola en localStorage para que sobreviva si se cierra la app
  const guardarColaEnStorage = () => {
    localStorage.setItem('cola_turnos', JSON.stringify(colaCambios.value));
  };

  // Escuchar los eventos de red y procesar si vuelve la conexión
  const inicializarRed = async () => {
    sinConexion.value = !navigator.onLine;

    window.addEventListener('online', () => {
      sinConexion.value = false;
      procesarCola(); // Al volver la red, se envía la cola
    });

    window.addEventListener('offline', () => {
      sinConexion.value = true;
    });

    // Si arranca con red y hay cosas pendientes, las procesa
    if (!sinConexion.value && colaCambios.value.length > 0) {
      procesarCola();
    }
  };

  // Obtener barberos con soporte de caché offline 
  const obtenerBarberosOffline = async () => {
    try {
      if (navigator.onLine) {
        const response = await ajax_service.solicitud_get('/barberos'); 
        const datosBarberos = response.data || response;
        
        barberosCache.value = Array.isArray(datosBarberos) ? datosBarberos : [];
        localStorage.setItem('cached_barberos', JSON.stringify(barberosCache.value));
        return barberosCache.value;
      } else {
        throw new Error('Sin conexión');
      }
    } catch (error) {
      // Si no hay red o falla, devuelve lo último guardado en el celular
      return barberosCache.value;
    }
  };

  // Encolar una reserva o cambio cuando no hay conexión
  const encolarCambio = (cambio: any) => {
    const cambioConMeta = {
      id_temporal: `temp_${Date.now()}`,
      ...cambio,
      sin_enviar: true,
      fecha_creacion: new Date().toISOString()
    };

    colaCambios.value.push(cambioConMeta);
    guardarColaEnStorage();
    return cambioConMeta;
  };

  // Reenviar peticiones acumuladas a la API usando el endpoint de sincronización en lote
  const procesarCola = async () => {
    if (colaCambios.value.length === 0 || sinConexion.value) return false;

    const turnosParaSincronizar = colaCambios.value.map(item => ({
      barberoId: Number(item.barberoId || item.barbero_id || 0),
      productoId: Number(item.productoId || item.servicio_id || 0),
      usuarioId: Number(item.usuarioId || item.usuario_id || 1),
      fechaHora: item.fechaHora || item.fecha_hora || new Date().toISOString()
    }));

    try {
      const response = await ajax_service.solicitud_post('/api/sincronizacion/turnos-pendientes', turnosParaSincronizar);
      console.log("Respuesta de sincronización:", response);

      const resData = response?.data || response;
      if (resData) {
        colaCambios.value = [];
        guardarColaEnStorage();
        console.log('Sincronización masiva de turnos completada con éxito.');
        return true; // 
      }
      return false;
    } catch (error: any) {
      console.error('Error detallado al sincronizar el lote offline:', error);
      return false; 
    }
  }; 

  return {
    sinConexion,
    colaCambios,
    barberosCache,
    inicializarRed,
    obtenerBarberosOffline,
    encolarCambio,
    procesarCola
  };
});