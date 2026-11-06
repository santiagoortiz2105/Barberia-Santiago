<template>
  <ion-app>
    <ion-router-outlet />
  </ion-app>
</template>

<script setup lang="ts">
import { IonApp, IonRouterOutlet } from '@ionic/vue';
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { App, URLOpenListenerEvent } from '@capacitor/app';
import { cargarTema } from './config/tema';
import { use_sesion_store } from './stores/sesion_store';

const router = useRouter();
const sesion = use_sesion_store();

onMounted(async () => {
  cargarTema();
  await sesion.restaurar_sesion(); 

  // 🔗 LISTENER DE CAPACITOR PARA DEEP LINKS / QR 
  App.addListener('appUrlOpen', (event: URLOpenListenerEvent) => {
    const url = event.url;
    
    if (url && url.includes('/p/')) {
      const indice = url.indexOf('/p/');
      if (indice !== -1) {
        const rutaConToken = url.substring(indice);
        // Redirige automáticamente a la vista pública del turno
        router.push(rutaConToken);
      }
    }
  });
});
</script>