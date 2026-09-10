<template>
  <comp-page titulo="Mi Cuenta">
    <ion-list>
      <ion-item>
        <ion-label>Modo Oscuro</ion-label>
        <ion-toggle :checked="esOscuro" @ionChange="cambiarTema"></ion-toggle>
      </ion-item>
    </ion-list>
  </comp-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { IonList, IonItem, IonLabel, IonToggle } from '@ionic/vue';
import compPage from '../components/base/comp_page.vue';
import { cargarTema, aplicarTema } from '../config/tema';

const esOscuro = ref(false);

onMounted(() => {
  esOscuro.value = cargarTema();
});

const cambiarTema = (e: CustomEvent) => {
  esOscuro.value = e.detail.checked;
  aplicarTema(esOscuro.value);
};
</script>