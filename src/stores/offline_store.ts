import { defineStore } from 'pinia';
import { ref } from 'vue';
import ajax_service from '../ajax_service';

export const use_offline_store = defineStore('offline', () => {
  const sinConexion = ref(!navigator.onLine);
  
  // Cargar la cola guardada previamente en localStorage
  const colaCambios = ref<any[]>(
    JSON.parse(localStorage.getItem('cola_turnos') || '[]')
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
      procesarCola(); // Al volver la red, se envía la cola sola
    });

    window.addEventListener('offline', () => {
      sinConexion.value = true;
    });

    // Si arranca con red y hay cosas pendientes, las procesa
    if (!sinConexion.value && colaCambios.value.length > 0) {
      procesarCola();
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

  // Reenviar peticiones acumuladas a la API
  const procesarCola = async () => {
    if (colaCambios.value.length === 0 || sinConexion.value) return;

    const copiaprocesar = [...colaCambios.value];

    for (const cambio of copiaprocesar) {
      try {
        // Enviar la reserva de turno pendiente al backend C#
        await ajax_service.solicitud_post('/turnos', {
          barbero_id: cambio.barbero_id,
          servicio_id: cambio.servicio_id,
          fecha_hora: cambio.fecha_hora
        });

        // Si la API responde OK, eliminar de la cola
        colaCambios.value = colaCambios.value.filter(
          item => item.id_temporal !== cambio.id_temporal
        );
        guardarColaEnStorage();
      } catch (error: any) {
        // Si el servidor rechaza por error de cliente (400/409 conflicto de horario), sale de la cola
        if (error.status >= 400 && error.status < 500) {
          colaCambios.value = colaCambios.value.filter(
            item => item.id_temporal !== cambio.id_temporal
          );
          guardarColaEnStorage();
        }
        // Si es error de red o servidor 500, se mantiene en la cola para reintentar luego
      }
    }
  };

  return {
    sinConexion,
    colaCambios,
    inicializarRed,
    encolarCambio,
    procesarCola
  };
});