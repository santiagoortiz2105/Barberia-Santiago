<template>
  <ion-menu menu-id="main-menu" :content-id="contentId">
    <ion-header>
      <ion-toolbar>
        <ion-title>Menú Barbería</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content>
      <ion-list>
        <ion-menu-toggle v-for="item in navegacion" :key="item.id">
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
import { IonMenu, IonHeader, IonToolbar, IonTitle, IonContent, IonList, IonItem, IonIcon, IonLabel, IonMenuToggle } from '@ionic/vue';
import { navegacion } from '../../config/navegacion';

defineProps<{ contentId: string }>();
</script>