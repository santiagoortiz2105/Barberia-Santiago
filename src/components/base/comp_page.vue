<template>
  <ion-page>
    <ion-header class="ion-no-border">
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-menu-button menu="main-menu"></ion-menu-button>
        </ion-buttons>
        <ion-title>{{ titulo }}</ion-title>
        <ion-buttons slot="end">
          <slot name="header-actions" />
        </ion-buttons>
      </ion-toolbar>
    </ion-header>

    <ion-content>
      <slot />
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import { 
  IonPage, 
  IonHeader, 
  IonToolbar, 
  IonTitle, 
  IonContent, 
  IonButtons, 
  IonMenuButton 
} from '@ionic/vue';

defineProps<{ titulo?: string }>();
</script>

<style scoped>
ion-title {
  font-weight: 800;
  font-size: 1.4rem;
}
</style>