<template>
  <ion-page>
    <ion-content class="ion-padding login-container" fullscreen>
      <div class="login-card">
        <h2>Barbería Santiago</h2>
        <p class="subtitulo">
          {{ esRegistro ? 'Crea una cuenta nueva' : 'Inicia sesión para continuar' }}
        </p>

        <!-- Campo Nombre (Solo visible en modo Registro) -->
        <div class="input-group" v-if="esRegistro">
          <label>Nombre y Apellido</label>
          <input type="text" v-model="nombre" placeholder="Ej. Juan Pérez" />
        </div>

        <div class="input-group">
          <label>Correo Electrónico</label>
          <input type="email" v-model="email" placeholder="correo@gmail.com" />
        </div>

        <div class="input-group">
          <label>Contraseña</label>
          <input type="password" v-model="password" placeholder="••••••••" />
        </div>

        <button class="btn-login" @click="handleAccion">
          {{ esRegistro ? 'REGISTRARSE' : 'INGRESAR' }}
        </button>

        <!-- Enlace para alternar entre Login y Registro -->
        <div class="toggle-modo">
          <p v-if="!esRegistro">
            ¿No tienes cuenta? <span @click="esRegistro = true">Regístrate aquí</span>
          </p>
          <p v-else>
            ¿Ya tienes cuenta? <span @click="esRegistro = false">Inicia sesión</span>
          </p>
        </div>
      </div>
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { IonPage, IonContent, alertController } from '@ionic/vue';
import { useRouter } from 'vue-router';
import { use_sesion_store } from '../stores/sesion_store';
import { obtener_api_url } from '../config/debug';

const router = useRouter();
const sesion = use_sesion_store();
const apiUrl = obtener_api_url();

const esRegistro = ref(false);
const nombre = ref('');
const email = ref('');
const password = ref('');

const mostrarAlerta = async (titulo: string, mensaje: string) => {
  const alert = await alertController.create({
    header: titulo,
    message: mensaje,
    buttons: ['Aceptar']
  });
  await alert.present();
};

const handleAccion = async () => {
  if (!email.value || !password.value || (esRegistro.value && !nombre.value)) {
    await mostrarAlerta('Campos incompletos', 'Por favor, llena todos los campos necesarios.');
    return;
  }

  try {
    const baseClean = (apiUrl || 'http://192.168.100.137:5211').replace(/\/+$/, '').replace(/\/api$/, '');

    if (esRegistro.value) {
      // Petición de Registro hacia el backend en C#
      const response = await fetch(`${baseClean}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          nombre: nombre.value,
          email: email.value,
          password: password.value
        })
      });

      const data = await response.json();
      if (!response.ok) throw new Error(data.mensaje || 'Error al registrar la cuenta');

      await mostrarAlerta('¡Éxito!', 'Cuenta creada correctamente. Ya puedes iniciar sesión.');
      esRegistro.value = false;
      password.value = '';
    } else {
      // Petición de Login tradicional enviando también la contraseña
      const response = await fetch(`${baseClean}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          email: email.value.trim(),
          password: password.value 
        })
      });

      const data = await response.json();
      if (!response.ok) throw new Error(data.mensaje || 'Correo o contraseña incorrectos');

      console.log("Datos de respuesta del Login:", data);

      const tokenRecibido = data.token;
      if (!tokenRecibido) {
        throw new Error('El servidor no devolvió un token de acceso válido.');
      }

      // 🔑 GUARDADO DIRECTO E INFALIBLE EN EL DISPOSITIVO MÓVIL
      localStorage.setItem('tudominio_token', tokenRecibido);
      localStorage.setItem('token', tokenRecibido);
      localStorage.setItem('access_token', tokenRecibido);

      let rolAsignado = data.rol || 'Cliente';
      console.log("Rol asignado a la sesión:", rolAsignado);

      // Guardar sesión en el Store de Pinia
      await sesion.guardar_sesion(
        tokenRecibido,
        data.refreshToken || 'refresh-token-de-prueba',
        rolAsignado,
        data.usuarioId
      );
      
      router.replace('/app/inicio');
    }

  } catch (error: any) {
    console.error('Error en la autenticación:', error);
    await mostrarAlerta('Atención', error.message || 'No se pudo conectar con el backend.');
  }
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

.toggle-modo {
  text-align: center;
  margin-top: 16px;
  font-size: 0.85rem;
  color: #666;
}

.toggle-modo span {
  color: #3880ff;
  font-weight: bold;
  cursor: pointer;
}
</style>