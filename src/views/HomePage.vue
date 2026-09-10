<template>
  <ion-page>
    <ion-header :translucent="true">
      <ion-toolbar :class="{ 'dark-toolbar': isDark }">
        <ion-title>Santiago - Barbería</ion-title>
        
      
        <ion-buttons slot="end">
          <ion-item lines="none" class="bg-transparent">
            <ion-icon :icon="moon" slot="start" :class="{ 'icon-dark': isDark }"></ion-icon>
            <ion-toggle :checked="isDark" @ionChange="toggleDarkMode"></ion-toggle>
          </ion-item>
        </ion-buttons>
      </ion-toolbar>
    </ion-header>


    <ion-content :fullscreen="true" :class="{ 'modo-oscuro': isDark }">
      <ion-header collapse="condense">
        <ion-toolbar :class="{ 'dark-toolbar': isDark }">
          <ion-title size="large">Santiago - Barbería</ion-title>
        </ion-toolbar>
      </ion-header>

      <div id="container">
    
        <ion-card 
          v-for="(servicio, index) in servicios" 
          :key="index"
          :class="{ 'dark-card': isDark }"
        >
          <ion-card-header>
            <ion-card-title :class="{ 'text-white': isDark }">{{ servicio.titulo }}</ion-card-title>
            <ion-card-subtitle :class="{ 'text-accent': isDark }">{{ servicio.precio }}</ion-card-subtitle>
          </ion-card-header>

          <ion-card-content :class="{ 'text-light': isDark }">
            {{ servicio.descripcion }}
          </ion-card-content>
        </ion-card>
      </div>
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import { 
  IonContent, 
  IonHeader, 
  IonPage, 
  IonTitle, 
  IonToolbar,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardSubtitle,
  IonCardContent,
  IonButtons,
  IonItem,
  IonToggle,
  IonIcon
} from '@ionic/vue';
import { ref } from 'vue';
import { moon } from 'ionicons/icons';


const isDark = ref(false);

const toggleDarkMode = (event: CustomEvent) => {
  isDark.value = event.detail.checked;
};

const servicios = ref([
  {
    titulo: 'Corte Tradicional',
    precio: '$10.000',
    descripcion: 'Corte de cabello clásico o moderno.'
  },
  {
    titulo: 'Barba Completa',
    precio: '$3.500',
    descripcion: 'Rebaje, emprolijado y delineado de barba.'
  },
  {
    titulo: 'Perfilado y Cejas',
    precio: '$2.000',
    descripcion: 'Perfilado preciso de contornos a navaja y diseño de cejas.'
  }
]);
</script>

<style scoped>
#container {
  padding: 10px;
}

.bg-transparent {
  --background: transparent;
}




ion-content.modo-oscuro {
  --background: #121212 !important;
  color: #ffffff;
}

/* Barras superiores */
.dark-toolbar {
  --background: #1e1e1e !important;
  --color: #ffffff !important;
  color: #ffffff !important;
}

/* Tarjetas */
.dark-card {
  --background: #1e1e1e !important;
  background: #1e1e1e !important;
  border: 1px solid #333333;
}


.text-white {
  color: #ffffff !important;
}

.text-accent {
  color: #4dabf7 !important;
}

.text-light {
  color: #cccccc !important;
}

.icon-dark {
  color: #4dabf7 !important;
}
</style>