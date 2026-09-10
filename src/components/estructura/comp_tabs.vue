<template>
  <ion-tab-bar slot="bottom">
    <ion-tab-button v-for="item in itemsPrincipales" :key="item.id" :tab="item.id" :href="item.ruta">
      <ion-icon :icon="item.icono" />
      <ion-label>{{ item.titulo }}</ion-label>
    </ion-tab-button>
  </ion-tab-bar>
</template>

<script setup lang="ts">
import { IonTabBar, IonTabButton, IonIcon, IonLabel } from '@ionic/vue';
import { navegacion } from '../../config/navegacion';
import { computed } from 'vue';

const itemsPrincipales = computed(() => navegacion.filter(item => item.grupo === 'principal'));
</script>