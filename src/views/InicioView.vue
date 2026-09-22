<template>
  <comp-page titulo="Inicio">
    <div class="ion-padding ion-text-center">
      <h2>Bienvenido a Santiago Barbería</h2>
      <p>Seleccioná una opción del menú o de la barra inferior para comenzar.</p>

      <!-- Botones de Cámara y Galería -->
      <div class="camera-section">
        <div class="button-group">
          <ion-button color="primary" expand="block" shape="round" @click="abrirCamara">
            Sacar Foto
          </ion-button>
          <ion-button color="tertiary" expand="block" shape="round" @click="abrirGaleria">
            Elegir de Galería
          </ion-button>
        </div>

        <!-- Vista previa de la imagen seleccionada -->
        <div v-if="imagen_seleccionada" class="preview-container">
          <p>Vista previa:</p>
          <img :src="imagen_seleccionada" alt="Foto seleccionada" class="preview-image" />
        </div>
      </div>

      <div style="margin-top: 40px; max-width: 300px; margin-left: auto; margin-right: auto;">
        <ion-button color="danger" expand="block" shape="round" @click="handleCerrarSesion">
          Cerrar Sesión
        </ion-button>
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { IonButton } from '@ionic/vue';
import { useRouter } from 'vue-router';
import { use_sesion_store } from '../stores/sesion_store';
import compPage from '../components/base/comp_page.vue';
import { tomar_foto, seleccionar_de_galeria } from '../services/camara_service';

const router = useRouter();
const sesion = use_sesion_store();

const imagen_seleccionada = ref<string | null>(null);

const abrirCamara = async () => {
  const base64 = await tomar_foto();
  if (base64) {
    imagen_seleccionada.value = base64;
  }
};

const abrirGaleria = async () => {
  const base64 = await seleccionar_de_galeria();
  if (base64) {
    imagen_seleccionada.value = base64;
  }
};

const handleCerrarSesion = async () => {
  await sesion.cerrar_sesion();
  router.replace('/login');
};
</script>

<style scoped>
.camera-section {
  margin-top: 30px;
  max-width: 300px;
  margin-left: auto;
  margin-right: auto;
}

.button-group {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.preview-container {
  margin-top: 20px;
}

.preview-container p {
  font-size: 0.9rem;
  font-weight: 600;
  margin-bottom: 8px;
}

.preview-image {
  width: 100%;
  max-height: 200px;
  object-fit: cover;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}
</style>