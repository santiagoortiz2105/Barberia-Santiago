<template>
  <ion-page>
    <ion-content class="ion-padding login-container" fullscreen>
      <div class="login-card">
        <h2>Barbería Santiago</h2>
        <p class="subtitulo">Inicia sesión para continuar</p>

        <div class="input-group">
          <label>Correo Electrónico</label>
          <input type="email" v-model="email" placeholder="admin@barberia.com" />
        </div>

        <div class="input-group">
          <label>Contraseña</label>
          <input type="password" v-model="password" placeholder="••••••••" />
        </div>

        <button class="btn-login" @click="handleLogin">
          INGRESAR
        </button>
      </div>
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { IonPage, IonContent, alertController } from '@ionic/vue';
import { useRouter } from 'vue-router';
import { use_sesion_store } from '../stores/sesion_store';

const router = useRouter();
const sesion = use_sesion_store();

const email = ref('');
const password = ref('');

const handleLogin = async () => {
  if (!email.value || !password.value) {
    const alert = await alertController.create({
      header: 'Campos vacíos',
      message: 'Por favor, ingresa tu email y contraseña.',
      buttons: ['Aceptar']
    });
    await alert.present();
    return;
  }

  let rolAsignado = 'administrador';
  if (email.value.includes('barbero')) rolAsignado = 'barbero';
  if (email.value.includes('sinrol')) rolAsignado = '';

  await sesion.guardar_sesion('token_jwt_falso_123456', 'refresh_token_789', rolAsignado);
  
  router.replace('/app/inicio');
};
</script>

<style scoped>
.login-container {
  display: flex;
  align-items: center;
  justify-content: center;
  --background: var(--ion-background-color, #f4f5f8);
}

.login-card {
  background: var(--ion-card-background, #ffffff);
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  width: 100%;
  max-width: 380px;
  margin: auto;
}

.login-card h2 {
  font-size: 1.6rem;
  font-weight: 800;
  text-align: center;
  margin-bottom: 4px;
  color: var(--ion-text-color, #000);
}

.subtitulo {
  text-align: center;
  font-size: 0.9rem;
  color: #666;
  margin-bottom: 24px;
}

.input-group {
  margin-bottom: 16px;
}

.input-group label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #666;
  margin-bottom: 6px;
}

.input-group input {
  width: 100%;
  padding: 12px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 1rem;
  background: var(--ion-background-color, #fff);
  color: var(--ion-text-color, #000);
}

.btn-login {
  width: 100%;
  background: #3880ff;
  color: white;
  border: none;
  padding: 14px;
  border-radius: 8px;
  font-weight: bold;
  font-size: 1rem;
  cursor: pointer;
  margin-top: 10px;
}
</style>