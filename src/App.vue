<template>
  <ion-app>
    <ion-router-outlet />
  </ion-app>
</template>

<script setup lang="ts">
import { IonApp, IonRouterOutlet } from '@ionic/vue';
import { onMounted } from 'vue';
import { cargarTema } from './config/tema';
import { use_sesion_store } from './stores/sesion_store';

const sesion = use_sesion_store();

onMounted(async () => {
  cargarTema();
  await sesion.restaurar_sesion(); // Restaura la sesión desde el almacenamiento seguro al iniciar
});
</script>