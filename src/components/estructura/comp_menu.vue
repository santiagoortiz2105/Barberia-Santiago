<template>
  <ion-menu menu-id="main-menu" :content-id="contentId">
    <ion-header>
      <ion-toolbar>
        <ion-title>Menú Barbería</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content>
      <ion-list>
        <ion-menu-toggle v-for="item in itemsMenuVisibles" :key="item.id">
          <ion-item :router-link="item.ruta" router-direction="root" button>
            <ion-icon slot="start" :icon="item.icono"></ion-icon>
            <ion-label>{{ item.titulo }}</ion-label>
          </ion-item>
        </ion-menu-toggle>
      </ion-list>
    </ion-content>
  </ion-menu>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { IonMenu, IonHeader, IonToolbar, IonTitle, IonContent, IonList, IonItem, IonIcon, IonLabel, IonMenuToggle } from '@ionic/vue';
import { navegacion } from '../../config/navegacion';
import { use_sesion_store } from '../../stores/sesion_store';

defineProps<{ contentId: string }>();

const sesion = use_sesion_store();

// Filtra dinámicamente los elementos del menú basándose en el rol activo del usuario
const itemsMenuVisibles = computed(() => {
  const rolActual = sesion.obtener_rol_activo();

  return navegacion.filter((item: any) => {
    const rolesPermitidos = item.menu_roles || ['TODOS'];

    // Si la ruta permite 'TODOS', se muestra siempre
    if (rolesPermitidos.includes('TODOS')) return true;

    // Si el usuario no tiene rol activo, no ve las pantallas operativas/restringidas
    if (!rolActual) return false;

    // Si el rol activo está incluido en los roles permitidos del item
    return rolesPermitidos.includes(rolActual);
  });
});
</script>