<template>
  <ion-page>
    <comp-menu content-id="contenido_principal" />
    <ion-tabs id="contenido_principal">
      <ion-router-outlet />
      <comp-tabs />
    </ion-tabs>
  </ion-page>
</template>

<script setup lang="ts">
import { IonPage, IonTabs, IonRouterOutlet } from '@ionic/vue';
import compMenu from '../components/estructura/comp_menu.vue';
import compTabs from '../components/estructura/comp_tabs.vue';
</script>