<template>
  <comp-page titulo="Barberos">
    <ion-list>
      <ion-item>
        <ion-label>
          <h2>Santiago</h2>
          <p>Master Barber - Especialista en cortes clásicos</p>
        </ion-label>
      </ion-item>
    </ion-list>
  </comp-page>
</template>

<script setup lang="ts">
import { IonList, IonItem, IonLabel } from '@ionic/vue';
import compPage from '../components/base/comp_page.vue';
</script>