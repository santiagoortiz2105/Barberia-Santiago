<template>
  <comp-page titulo="Inicio">
    <template #header-actions>
      <div class="theme-toggle-wrapper">
        <ion-icon :icon="moonOutline" class="moon-icon"></ion-icon>
        <ion-toggle :checked="esOscuro" @ionChange="cambiarTema"></ion-toggle>
      </div>
    </template>

    <div class="inicio-container">
      <!-- 1. Banner -->
      <div class="banner-bienvenida">
        <div class="avatar-container">
          <div class="avatar-placeholder"><span>✂️</span></div>
        </div>
        <h2>¡Bienvenido a Santiago Barbería!</h2>
        <p>Tu estilo, en las mejores manos. Gestiona tus turnos y cortes fácilmente.</p>
      </div>

      <!-- 2. Panel Admin (Cámara) -->
      <div v-if="esAdministrador" class="camera-section">
        <h3 class="titulo-seccion">Galería y Fotos (Panel Admin)</h3>
        <div class="button-group">
          <ion-button color="primary" expand="block" shape="round" @click="abrirCamara">📸 Sacar Foto</ion-button>
          <ion-button color="tertiary" expand="block" shape="round" @click="abrirGaleria">🖼️ Elegir de Galería</ion-button>
        </div>
        <div v-if="imagen_seleccionada" class="preview-container">
          <p>Vista previa:</p>
          <img :src="imagen_seleccionada" alt="Foto seleccionada" class="preview-image" />
          <ion-button color="success" expand="block" shape="round" class="mt-2" @click="subirImagenAlServidor">Subir imagen para todos</ion-button>
        </div>
      </div>

      <!-- 3. Galería Pública con opción de borrado para Admin -->
      <div class="galeria-publica-section">
        <h3 class="titulo-seccion">Trabajos Recientes</h3>
        <div class="grid-galeria">
          <p class="text-muted" v-if="galeriaPublica.length === 0">No hay imágenes publicadas todavía.</p>
          
          <div v-for="(img, index) in galeriaPublica" :key="img.id || index" class="imagen-wrapper">
            <img :src="img.url || img" class="imagen-galeria-item" alt="Trabajo" />
            
            <!-- ❌ Botón de eliminar (SOLO VISIBLE PARA ADMINISTRADOR) -->
            <button v-if="esAdministrador" class="btn-eliminar-foto" @click="eliminarImagen(img.id || index)" title="Eliminar foto">
              ❌
            </button>
          </div>
        </div>
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { IonButton, IonToggle, IonIcon } from '@ionic/vue';
import { moonOutline } from 'ionicons/icons';
import { use_sesion_store } from '../stores/sesion_store';
import compPage from '../components/base/comp_page.vue';
import { tomar_foto, seleccionar_de_galeria } from '../services/camara_service';
import { cargarTema, aplicarTema } from '../config/tema';
import ajax_service from '@/ajax_service';

const sesion = use_sesion_store();
const esOscuro = ref(false);
const imagen_seleccionada = ref<string | null>(null);
const galeriaPublica = ref<any[]>([]);

const esAdministrador = computed(() => {
  const rol = (sesion.rol_activo || (sesion as any).rol || (sesion as any).usuario?.rol || '').toLowerCase();
  return rol.includes('admin');
});

onMounted(async () => {
  esOscuro.value = cargarTema();
  try {
    const res = await ajax_service.solicitud_get('/api/galeria');
    galeriaPublica.value = Array.isArray(res) ? res : (res?.data || []);
  } catch (e) { console.error(e); }
});

const cambiarTema = (e: CustomEvent) => { esOscuro.value = e.detail.checked; aplicarTema(esOscuro.value); };
const abrirCamara = async () => { const b64 = await tomar_foto(); if (b64) imagen_seleccionada.value = b64; };
const abrirGaleria = async () => { const b64 = await seleccionar_de_galeria(); if (b64) imagen_seleccionada.value = b64; };

const subirImagenAlServidor = async () => {
  if (!imagen_seleccionada.value) return;
  await ajax_service.ajax_post('/api/galeria', { imagenBase64: imagen_seleccionada.value });
  imagen_seleccionada.value = null;
  const res = await ajax_service.solicitud_get('/api/galeria');
  galeriaPublica.value = Array.isArray(res) ? res : (res?.data || []);
};

// 🗑️ Función para eliminar la foto seleccionada
const eliminarImagen = async (idImagen: any) => {
  if (!confirm('¿Estás seguro de que deseas eliminar esta foto de la galería?')) return;

  try {
    // Petición DELETE al backend 
    await ajax_service.ajax_delete 
      ? await ajax_service.ajax_delete(`/api/galeria/${idImagen}`) 
      : await fetch(`http://localhost:5211/api/galeria/${idImagen}`, { method: 'DELETE' });
    
    // Filtramos la lista localmente para quitarla de la vista de inmediato
    galeriaPublica.value = galeriaPublica.value.filter((img: any) => (img.id || galeriaPublica.value.indexOf(img)) !== idImagen);
  } catch (err) {
    console.error('Error al eliminar la imagen:', err);
    alert('No se pudo eliminar la imagen del servidor.');
  }
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
  padding: 16px 16px 130px 16px; 
  max-width: 500px; 
  margin: 0 auto; 
  display: flex; 
  flex-direction: column; 
  gap: 20px; 
}

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

.titulo-seccion { 
  font-size: 1.05rem; 
  font-weight: 800; 
  margin-bottom: 12px; 
  color: var(--ion-text-color, #000); 
}

.camera-section, .galeria-publica-section { 
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

.mt-2 { 
  margin-top: 10px; 
}

.grid-galeria { 
  display: grid; 
  grid-template-columns: repeat(2, 1fr); 
  gap: 12px; 
}

/* Estilos para el contenedor de la imagen con el botón de borrar flotando */
.imagen-wrapper {
  position: relative;
  width: 100%;
  height: 120px;
}

.imagen-galeria-item { 
  width: 100%; 
  height: 100%; 
  object-fit: cover; 
  border-radius: 10px; 
}

.btn-eliminar-foto {
  position: absolute;
  top: 6px;
  right: 6px;
  background: rgba(239, 68, 68, 0.95);
  color: white;
  border: none;
  border-radius: 50%;
  width: 26px;
  height: 26px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
  transition: transform 0.2s;
}

.btn-eliminar-foto:hover {
  transform: scale(1.1);
  background: rgb(220, 38, 38);
}

.text-muted { 
  font-size: 0.85rem; 
  color: #888; 
  text-align: center; 
  grid-column: span 2; 
}
</style>