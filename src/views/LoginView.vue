<template>
  <ion-page>
    <ion-content class="login-background" :fullscreen="true">
      <div class="login-wrapper">
        <div class="login-card">
          
          <!-- Encabezado con Avatar / Logo -->
          <div class="login-header">
            <div class="logo-circle">
              <span>✂️</span>
            </div>
            <h2>Barbería Santiago</h2>
            <p class="subtitulo">
              {{ esRegistro ? 'Crea una cuenta nueva' : 'Inicia sesión para continuar' }}
            </p>
          </div>

          <!-- Formulario de Acceso / Registro -->
          <div class="form-container">
            
            <!-- Campo Nombre (Solo visible en modo Registro) -->
            <div class="input-group" v-if="esRegistro">
              <label>Nombre y Apellido</label>
              <input type="text" v-model="nombre" placeholder="Ej. Juan Pérez" class="custom-input" />
            </div>

            <div class="input-group">
              <label>Correo Electrónico</label>
              <input type="email" v-model="email" placeholder="correo@gmail.com" class="custom-input" />
            </div>

            <div class="input-group">
              <label>Contraseña</label>
              <input type="password" v-model="password" placeholder="••••••••" class="custom-input" />
            </div>

            <button class="btn-login" @click="handleAccion">
              {{ esRegistro ? 'REGISTRARSE' : 'INGRESAR' }}
            </button>

            <!-- Separador visual -->
            <div class="divider">
              <span>o</span>
            </div>

            <!-- 🌐 BOTÓN DE GOOGLE OAUTH 2.0 -->
            <button class="btn-google" @click="handleGoogleLogin">
              <svg class="google-icon" viewBox="0 0 24 24">
                <path fill="#4285F4" d="M23.745 12.27c0-.7-.06-1.4-.19-2.07H12v4.51h6.6c-.29 1.52-1.14 2.82-2.4 3.68v3.05h3.88c2.27-2.09 3.66-5.17 3.66-9.17z"/>
                <path fill="#34A853" d="M12 24c3.24 0 5.95-1.08 7.93-2.91l-3.88-3.05c-1.08.72-2.45 1.16-4.05 1.16-3.13 0-5.78-2.11-6.73-4.96H1.19v3.15C3.18 21.38 7.22 24 12 24z"/>
                <path fill="#FBBC05" d="M5.27 14.24c-.25-.72-.38-1.49-.38-2.24s.13-1.52.38-2.24V6.61H1.19C.43 8.15 0 9.89 0 11.75s.43 3.6 1.19 5.14l4.08-2.65z"/>
                <path fill="#EA4335" d="M12 4.75c1.77 0 3.35.61 4.6 1.8l3.42-3.42C17.95 1.19 15.24 0 12 0 7.22 0 3.18 2.62 1.19 6.61l4.08 3.15c.95-2.85 3.6-4.96 6.73-4.96z"/>
              </svg>
              Continuar con Google
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
import { GoogleAuth } from '@codetrix-studio/capacitor-google-auth';

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

      const tokenRecibido = data.token;
      if (!tokenRecibido) {
        throw new Error('El servidor no devolvió un token de acceso válido.');
      }

      localStorage.setItem('tudominio_token', tokenRecibido);
      localStorage.setItem('token', tokenRecibido);
      localStorage.setItem('access_token', tokenRecibido);

      let rolAsignado = data.rol || 'Cliente';

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

const handleGoogleLogin = async () => {
  try {
    const googleUser = await GoogleAuth.signIn();
    const idTokenReal = googleUser.authentication.idToken;

    if (!idTokenReal) {
      throw new Error("No se pudo obtener el token de autenticación de Google.");
    }

    const exito = await sesion.login_con_google(idTokenReal);

    if (exito) {
      router.replace('/app/inicio');
    } else {
      await mostrarAlerta('Acceso Denegado', sesion.error_login || 'No se pudo iniciar sesión con Google.');
    }
  } catch (error: any) {
    console.error('Error en Google Login:', error);
    await mostrarAlerta('Error', error.message || 'Ocurrió un error al procesar el inicio de sesión con Google.');
  }
};
</script>

<style scoped>
/* Fondo general con degradado moderno y elegante */
.login-background {
  --background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
}

/* Contenedor flexible para centrar perfectamente al medio en cualquier pantalla */
.login-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 20px;
}

/* Tarjeta flotante moderna */
.login-card {
  background: var(--ion-card-background, #ffffff);
  padding: 32px 24px;
  border-radius: 24px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.25);
  width: 100%;
  max-width: 400px;
  box-sizing: border-box;
}

/* Encabezado y Logo */
.login-header {
  text-align: center;
  margin-bottom: 24px;
}

.logo-circle {
  width: 60px;
  height: 60px;
  background: linear-gradient(135deg, #3880ff, #1d4ed8);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.6rem;
  margin: 0 auto 12px auto;
  box-shadow: 0 8px 16px rgba(56, 128, 255, 0.3);
}

.login-card h2 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0 0 4px 0;
  color: var(--ion-text-color, #111);
}

.subtitulo {
  text-align: center;
  font-size: 0.88rem;
  color: #64748b;
  margin: 0;
}

/* Contenedor de formulario */
.form-container {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.input-group label {
  font-size: 0.82rem;
  font-weight: 700;
  color: #475569;
  margin-left: 2px;
}

/* Inputs personalizados y limpios */
.custom-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #cbd5e1;
  border-radius: 12px;
  font-size: 0.95rem;
  background: #f8fafc;
  color: #1e293b;
  outline: none;
  transition: all 0.2s;
  box-sizing: border-box;
}

.custom-input:focus {
  border-color: #3880ff;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(56, 128, 255, 0.15);
}


.btn-login {
  width: 100%;
  background: #3880ff;
  color: white;
  border: none;
  padding: 14px;
  border-radius: 12px;
  font-weight: 700;
  font-size: 0.95rem;
  letter-spacing: 0.5px;
  cursor: pointer;
  margin-top: 6px;
  box-shadow: 0 4px 12px rgba(56, 128, 255, 0.3);
  transition: background 0.2s, transform 0.1s;
}

.btn-login:hover {
  background: #3171e0;
}

.btn-login:active {
  transform: scale(0.98);
}


.divider {
  display: flex;
  align-items: center;
  text-align: center;
  margin: 14px 0;
}

.divider::before, 
.divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid #e2e8f0;
}

.divider span {
  padding: 0 12px;
  font-size: 0.8rem;
  color: #94a3b8;
  text-transform: uppercase;
  font-weight: 600;
}

.btn-google {
  width: 100%;
  background: white;
  color: #334155;
  border: 1px solid #cbd5e1;
  padding: 12px;
  border-radius: 12px;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.04);
  transition: background 0.2s, border-color 0.2s;
}

.btn-google:hover {
  background: #f8fafc;
  border-color: #94a3b8;
}

.google-icon {
  width: 18px;
  height: 18px;
}

.toggle-modo {
  text-align: center;
  margin-top: 14px;
  font-size: 0.85rem;
  color: #64748b;
}

.toggle-modo span {
  color: #3880ff;
  font-weight: 700;
  cursor: pointer;
}

.toggle-modo span:hover {
  text-decoration: underline;
}
</style>