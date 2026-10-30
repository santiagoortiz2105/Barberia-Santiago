<template>
  <comp-page titulo="Inicio">
    <!-- Interruptor de Tema Oscuro en la esquina superior derecha -->
    <template #header-actions>
      <div class="theme-toggle-wrapper">
        <ion-icon :icon="moonOutline" class="moon-icon"></ion-icon>
        <ion-toggle :checked="esOscuro" @ionChange="cambiarTema"></ion-toggle>
      </div>
    </template>

    <div class="inicio-container">
      
      <!-- 1. Banner de Bienvenida Estilizado -->
      <div class="banner-bienvenida">
        <div class="avatar-container">
          <div class="avatar-placeholder">
            <span>✂️</span>
          </div>
        </div>
        <h2>¡Bienvenido a Santiago Barbería!</h2>
        <p>Tu estilo, en las mejores manos. Gestiona tus turnos y cortes fácilmente.</p>
      </div>

      <!-- 2. Sección de Cámara y Galería -->
      <div class="camera-section">
        <h3 class="titulo-seccion">Galería y Fotos</h3>
        <div class="button-group">
          <ion-button color="primary" expand="block" shape="round" @click="abrirCamara">
            📸 Sacar Foto
          </ion-button>
          <ion-button color="tertiary" expand="block" shape="round" @click="abrirGaleria">
            🖼️ Elegir de Galería
          </ion-button>
        </div>

        <!-- Vista previa de la imagen seleccionada -->
        <div v-if="imagen_seleccionada" class="preview-container">
          <p>Vista previa:</p>
          <img :src="imagen_seleccionada" alt="Foto seleccionada" class="preview-image" />
        </div>
      </div>

      <!-- 3. Botón de Cerrar Sesión Distintivo -->
      <div class="seccion-logout">
        <ion-button color="danger" expand="block" shape="round" @click="handleCerrarSesion">
          Cerrar Sesión
        </ion-button>
      </div>

    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { IonButton, IonToggle, IonIcon } from '@ionic/vue';
import { moonOutline } from 'ionicons/icons';
import { useRouter } from 'vue-router';
import { use_sesion_store } from '../stores/sesion_store';
import compPage from '../components/base/comp_page.vue';
import { tomar_foto, seleccionar_de_galeria } from '../services/camara_service';
import { cargarTema, aplicarTema } from '../config/tema';

const router = useRouter();
const sesion = use_sesion_store();

const esOscuro = ref(false);
const imagen_seleccionada = ref<string | null>(null);

onMounted(() => {
  esOscuro.value = cargarTema();
});

const cambiarTema = (e: CustomEvent) => {
  esOscuro.value = e.detail.checked;
  aplicarTema(esOscuro.value);
};

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
.theme-toggle-wrapper {
  display: flex;
  align-items: center;
  gap: 6px;
  padding-right: 12px;
}

.moon-icon {
  font-size: 1.2rem;
  color: var(--ion-color-step-650, #666);
}

.inicio-container {
  padding: 16px;
  max-width: 500px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* Banner Superior */
.banner-bienvenida {
  background: linear-gradient(135deg, #1e1e2f, #2d2d44);
  color: white;
  padding: 28px 20px;
  border-radius: 20px;
  text-align: center;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.avatar-container {
  margin-bottom: 12px;
}

.avatar-placeholder {
  width: 65px;
  height: 65px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.8rem;
  margin: 0 auto;
  border: 2px solid rgba(255, 255, 255, 0.2);
}

.banner-bienvenida h2 {
  font-size: 1.35rem;
  font-weight: 800;
  margin: 0 0 6px 0;
}

.banner-bienvenida p {
  font-size: 0.88rem;
  color: #a0aec0;
  margin: 0;
  line-height: 1.4;
}

/* Títulos de sección */
.titulo-seccion {
  font-size: 1.05rem;
  font-weight: 800;
  margin-bottom: 12px;
  color: var(--ion-text-color, #000);
}

/* Sección de Cámara */
.camera-section {
  background: var(--ion-card-background, #ffffff);
  padding: 20px;
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
  border: 1px solid rgba(0, 0, 0, 0.03);
}

.button-group {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.preview-container {
  margin-top: 16px;
}

.preview-container p {
  font-size: 0.85rem;
  font-weight: 700;
  margin-bottom: 6px;
  color: var(--ion-color-step-600, #666);
}

.preview-image {
  width: 100%;
  max-height: 180px;
  object-fit: cover;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  border: 1px solid #eaeaea;
}

/* Sección Logout */
.seccion-logout {
  margin-top: 10px;
}
</style>