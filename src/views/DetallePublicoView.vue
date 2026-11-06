<template>
  <ion-page>
    <ion-header>
      <ion-toolbar color="primary">
        <ion-title>Detalle del Turno</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding" fullscreen>
      <!-- Estado de carga -->
      <div v-if="cargando" class="ion-text-center ion-padding">
        <ion-spinner name="crescent"></ion-spinner>
        <p>Verificando turno...</p>
      </div>

      <!-- Estado de error -->
      <div v-else-if="errorMensaje" class="ion-text-center ion-padding">
        <ion-text color="danger">
          <h2>⚠️ Enlace no válido</h2>
          <p>{{ errorMensaje }}</p>
        </ion-text>
        <ion-button expand="block" router-link="/login" class="ion-margin-top">
          Ir al Inicio
        </ion-button>
      </div>

      <!-- Visualización de los datos del Turno -->
      <div v-else-if="turno" class="turno-card">
        <div class="header-ticket">
          <h3>Barbería Santiago</h3>
          <span class="badge" :class="turno.estado.toLowerCase()">{{ turno.estado }}</span>
        </div>

        <div class="detalle-group">
          <p><strong>Servicio:</strong> {{ turno.servicio }}</p>
          <p><strong>Precio:</strong> ${{ turno.precio }}</p>
          <p><strong>Fecha y Hora:</strong> {{ formatearFecha(turno.fechaHora) }}</p>
          <p><strong>Cliente:</strong> {{ turno.clienteNombre }}</p>
          <p><strong>Barbero:</strong> {{ turno.barberoNombre }}</p>
        </div>

        <ion-button expand="block" router-link="/login" class="ion-margin-top">
          Volver a la App
        </ion-button>
      </div>
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { IonPage, IonHeader, IonToolbar, IonTitle, IonContent, IonSpinner, IonText, IonButton } from '@ionic/vue';
import { obtener_api_url } from '../config/debug';

const route = useRoute();
const apiUrl = obtener_api_url();

const turno = ref<any>(null);
const cargando = ref(true);
const errorMensaje = ref('');

const cargarTurnoPublico = async (token: string) => {
  try {
    cargando.value = true;
    const baseClean = (apiUrl || 'http://192.168.100.137:5211').replace(/\/+$/, '').replace(/\/api$/, '');
    
    // Petición al endpoint público anónimo que creamos en el backend
    const response = await fetch(`${baseClean}/api/publico/turnos/${token}`);
    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.mensaje || 'El turno no existe o el enlace ha expirado.');
    }

    turno.value = data;
  } catch (error: any) {
    errorMensaje.value = error.message || 'No se pudo cargar la información del turno.';
  } finally {
    cargando.value = false;
  }
};

const formatearFecha = (fechaStr: string) => {
  try {
    const fecha = new Date(fechaStr);
    return fecha.toLocaleString();
  } catch {
    return fechaStr;
  }
};

onMounted(() => {
  const tokenPublico = route.params.tokenPublico as string;
  if (tokenPublico) {
    cargarTurnoPublico(tokenPublico);
  } else {
    errorMensaje.value = 'Token no proporcionado.';
    cargando.value = false;
  }
});
</script>

<style scoped>
.turno-card {
  background: var(--ion-card-background, #ffffff);
  padding: 20px;
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
  max-width: 400px;
  margin: 20px auto;
}

.header-ticket {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 2px dashed #ddd;
  padding-bottom: 12px;
  margin-bottom: 16px;
}

.header-ticket h3 {
  margin: 0;
  font-size: 1.2rem;
  font-weight: bold;
}

.badge {
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: bold;
  text-transform: uppercase;
  background: #e0e0e0;
  color: #333;
}

.badge.reservado {
  background: #e3f2fd;
  color: #1976d2;
}

.detalle-group p {
  margin: 8px 0;
  font-size: 0.95rem;
  color: #444;
}
</style>