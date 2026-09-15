<template>
  <comp-page titulo="Mi Cuenta">
    <div class="ion-padding">
      
      <!-- Aviso exclusivo para el usuario sin rol -->
      <div v-if="es_sin_rol" class="aviso-sin-rol">
        <h3>Cuenta pendiente de habilitación</h3>
        <p>Tu cuenta se encuentra registrada exitosamente, pero aún no tienes un rol asignado en el sistema.</p>
        <p class="sub-aviso">Un administrador debe habilitar tu usuario para que puedas acceder a las funciones operativas de la barbería.</p>
      </div>

      <ion-list>
        <ion-item>
          <ion-label>Modo Oscuro</ion-label>
          <ion-toggle :checked="esOscuro" @ionChange="cambiarTema"></ion-toggle>
        </ion-item>
      </ion-list>

    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { IonList, IonItem, IonLabel, IonToggle } from '@ionic/vue';
import compPage from '../components/base/comp_page.vue';
import { cargarTema, aplicarTema } from '../config/tema';
import { use_sesion_store } from '@/stores/sesion_store';

const sesion = use_sesion_store();
const esOscuro = ref(false);

// Obtenemos el rol actual de forma segura
const rol_actual = computed(() => {
  return sesion.obtener_rol_activo();
});

// Verificamos si no tiene rol (si es nulo, vacío o 'sinrol')
const es_sin_rol = computed(() => {
  const rol = rol_actual.value;
  if (!rol) return true;
  if (Array.isArray(rol) && rol.length === 0) return true;
  if (rol === 'sinrol') return true;
  return false;
});

onMounted(() => {
  esOscuro.value = cargarTema();
});

const cambiarTema = (e: CustomEvent) => {
  esOscuro.value = e.detail.checked;
  aplicarTema(esOscuro.value);
};
</script>

<style scoped>
.aviso-sin-rol {
  background: var(--ion-color-warning-tint, #fff3cd);
  color: var(--ion-color-warning-shade, #856404);
  padding: 16px;
  border-radius: 12px;
  margin-bottom: 20px;
  border: 1px solid var(--ion-color-warning, #ffeeba);
}

.aviso-sin-rol h3 {
  font-size: 1.1rem;
  font-weight: 700;
  margin-top: 0;
  margin-bottom: 8px;
}

.aviso-sin-rol p {
  font-size: 0.95rem;
  margin: 0 0 8px 0;
  line-height: 1.4;
}

.sub-aviso {
  font-size: 0.85rem !important;
  opacity: 0.85;
}
</style>