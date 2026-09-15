<template>
  <comp-page titulo="Inicio">
    <div class="ion-padding ion-text-center">
      <h2>Bienvenido a Santiago Barbería</h2>
      <p>Seleccioná una opción del menú o de la barra inferior para comenzar.</p>

      <div style="margin-top: 40px; max-width: 300px; margin-left: auto; margin-right: auto;">
        <ion-button color="danger" expand="block" shape="round" @click="handleCerrarSesion">
          Cerrar Sesión
        </ion-button>
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { IonButton } from '@ionic/vue';
import { useRouter } from 'vue-router';
import { use_sesion_store } from '../stores/sesion_store';
import compPage from '../components/base/comp_page.vue';

const router = useRouter();
const sesion = use_sesion_store();

const handleCerrarSesion = async () => {
  await sesion.cerrar_sesion();
  router.replace('/login');
};
</script>