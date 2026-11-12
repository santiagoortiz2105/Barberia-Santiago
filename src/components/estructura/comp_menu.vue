<template>
  <ion-menu menu-id="main-menu" :content-id="contentId">
    <ion-header>
      <ion-toolbar>
        <ion-title>Menú Barbería</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content>
      <ion-list>
        <!-- Opciones dinámicas de navegación -->
        <ion-menu-toggle v-for="item in itemsMenuVisibles" :key="item.id">
          <ion-item :router-link="item.ruta" router-direction="root" button>
            <ion-icon slot="start" :icon="item.icono"></ion-icon>
            <ion-label>{{ item.titulo }}</ion-label>
          </ion-item>
        </ion-menu-toggle>

        <!-- Botón Fijo de Cerrar Sesión al final de la lista -->
        <ion-menu-toggle>
          <ion-item button @click="handleCerrarSesion" lines="none" class="menu-item-logout">
            <ion-icon slot="start" :icon="logOutOutline" color="danger"></ion-icon>
            <ion-label color="danger">Cerrar Sesión</ion-label>
          </ion-item>
        </ion-menu-toggle>
      </ion-list>
    </ion-content>
  </ion-menu>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { IonMenu, IonHeader, IonToolbar, IonTitle, IonContent, IonList, IonItem, IonIcon, IonLabel, IonMenuToggle } from '@ionic/vue';
import { logOutOutline } from 'ionicons/icons';
import { navegacion } from '../../config/navegacion';
import { use_sesion_store } from '../../stores/sesion_store';

defineProps<{ contentId: string }>();

const router = useRouter();
const sesion = use_sesion_store();

// Filtra dinámicamente los elementos del menú basándose en el rol activo del usuario
const itemsMenuVisibles = computed(() => {
  const rolActual = sesion.obtener_rol_activo();

  return navegacion.filter((item: any) => {
    const rolesPermitidos = item.menu_roles || ['TODOS'];

    if (rolesPermitidos.includes('TODOS')) return true;
    if (!rolActual) return false;

    return rolesPermitidos.includes(rolActual);
  });
});

const handleCerrarSesion = async () => {
  await sesion.cerrar_sesion();
  router.replace('/login');
};
</script>

<style scoped>
.menu-item-logout {
  margin-top: 20px;
  border-top: 1px solid rgba(0, 0, 0, 0.08);
}
</style>