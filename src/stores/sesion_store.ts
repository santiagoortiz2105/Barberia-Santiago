import { defineStore } from 'pinia';
import { ref } from 'vue';
import { SecureStorage } from '@aparajita/capacitor-secure-storage';
import axios from 'axios';
import { obtener_api_url } from '../config/debug';

const CLAVE_TOKEN = 'tudominio_token';
const CLAVE_REFRESH = 'tudominio_refresh';
const CLAVE_ROL = 'tudominio_rol';
const CLAVE_USER_ID = 'tudominio_user_id'; 

export const use_sesion_store = defineStore('sesion', () => {
  const token = ref<string | null>(null);
  const refresh_token = ref<string | null>(null);
  const autenticado = ref<boolean>(false);
  const restaurando = ref<boolean>(true);
  const rol_activo = ref<string | null>(null);
  const usuario_id = ref<any | null>(null); 
  const error_login = ref<string | null>(null); 

  const restaurar_sesion = async () => {
    try {
      restaurando.value = true;
      
      let tokenRes: any = null;
      let rolRes: any = null;
      let userIdRes: any = null;

      try {
        tokenRes = await (SecureStorage as any).get(CLAVE_TOKEN);
        rolRes = await (SecureStorage as any).get(CLAVE_ROL);
        userIdRes = await (SecureStorage as any).get(CLAVE_USER_ID);
      } catch (err) {
        console.warn("SecureStorage falló en el dispositivo, usando respaldo local:", err);
      }
    
      const finalToken = (typeof tokenRes === 'string' ? tokenRes : tokenRes?.value) || localStorage.getItem(CLAVE_TOKEN);
      const finalRol = (typeof rolRes === 'string' ? rolRes : rolRes?.value) || localStorage.getItem(CLAVE_ROL);
      const finalUserId = (typeof userIdRes === 'string' ? userIdRes : userIdRes?.value) || localStorage.getItem(CLAVE_USER_ID);

      if (finalToken) {
        token.value = finalToken;
        rol_activo.value = finalRol ? finalRol.toLowerCase() : null; 
        usuario_id.value = finalUserId;
        autenticado.value = true;
        console.log("Sesión restaurada exitosamente. Rol activo:", rol_activo.value);
        
        // Opcional: Re-sincronizar el token push al restaurar sesión activa
        registrar_dispositivo_push(finalToken);
      }
    } catch (e) {
      console.error("Error crítico al restaurar sesión:", e);
      autenticado.value = false;
    } finally {
      restaurando.value = false;
    }
  };

  // NObtiene el token FCM del dispositivo y lo manda a la API
  const registrar_dispositivo_push = async (tokenJwt: string) => {
    try {
      let fcmToken = null;

      try {
        // Importación dinámica del plugin de Capacitor Firebase Messaging
        // @ts-ignore
        const { FirebaseMessaging } = await import('@capacitor-firebase/messaging');
        await FirebaseMessaging.requestPermissions();
        const result = await FirebaseMessaging.getToken();
        fcmToken = result.token;
      } catch (err) {
        console.log("Entorno web o plugin nativo no disponible para Push (normal en navegador):", err);
        console.error("❌ ERROR DETALLADO AL REGISTRAR PUSH:", err);
        return; 
      }

      if (!fcmToken) return;

      const apiUrl = obtener_api_url();
      const baseClean = (apiUrl || 'http://192.168.100.137:5211').replace(/\/+$/, '').replace(/\/api$/, '');

      // Envía el token a tu endpoint PUT /api/dispositivos
      await axios.put(`${baseClean}/api/dispositivos`, {
        token: fcmToken
      }, {
        headers: {
          Authorization: `Bearer ${tokenJwt}`
        }
      });

      console.log("Token push registrado correctamente en la API.");
    } catch (e) {
      console.warn("No se pudo registrar el dispositivo push:", e);
    }
  };

  const guardar_sesion = async (newToken: string, newRefresh: string, newRol: string, newUserId: any) => {
    const rolNormalizado = newRol ? newRol.toLowerCase() : '';

    token.value = newToken;
    refresh_token.value = newRefresh;
    rol_activo.value = rolNormalizado;
    usuario_id.value = newUserId;
    autenticado.value = true;

    localStorage.setItem(CLAVE_TOKEN, newToken);
    localStorage.setItem('token', newToken);
    localStorage.setItem('access_token', newToken);

    localStorage.setItem(CLAVE_REFRESH, newRefresh);
    localStorage.setItem(CLAVE_ROL, rolNormalizado);
    localStorage.setItem(CLAVE_USER_ID, String(newUserId));

    try {
      await (SecureStorage as any).set(CLAVE_TOKEN, newToken);
      await (SecureStorage as any).set(CLAVE_REFRESH, newRefresh);
      await (SecureStorage as any).set(CLAVE_ROL, rolNormalizado);
      await (SecureStorage as any).set(CLAVE_USER_ID, String(newUserId));
    } catch (e) {
      console.warn("No se pudo guardar en SecureStorage nativo, se usó localStorage:", e);
    }

    // DISPARAR EL REGISTRO AUTOMÁTICO DEL DISPOSITIVO PUSH
    await registrar_dispositivo_push(newToken);
  };

  // ACCIÓN PARA INICIAR SESIÓN CON GOOGLE OAUTH
  const login_con_google = async (idTokenGoogle: string) => {
    try {
      error_login.value = null;
      
      const apiUrl = obtener_api_url();
      const baseClean = (apiUrl || 'http://192.168.100.137:5211').replace(/\/+$/, '').replace(/\/api$/, '');

      const response = await axios.post(`${baseClean}/api/auth/google`, {
        idToken: idTokenGoogle
      });

      const data = response.data;
      
      await guardar_sesion(
        data.token, 
        data.refreshToken || 'google-token', 
        data.rol || 'Cliente', 
        data.usuarioId
      );

      return true;
    } catch (error: any) {
      if (error.response && error.response.status === 403) {
        error_login.value = error.response.data.mensaje || 'Tu cuenta no está habilitada por un administrador.';
      } else {
        error_login.value = 'Ocurrió un error al autenticar con Google.';
      }
      return false;
    }
  };

  const cerrar_sesion = async () => {
    token.value = null;
    refresh_token.value = null;
    rol_activo.value = null;
    usuario_id.value = null; 
    autenticado.value = false;
    error_login.value = null;

    localStorage.removeItem(CLAVE_TOKEN);
    localStorage.removeItem(CLAVE_REFRESH);
    localStorage.removeItem(CLAVE_ROL);
    localStorage.removeItem(CLAVE_USER_ID);

    try {
      await (SecureStorage as any).remove(CLAVE_TOKEN);
      await (SecureStorage as any).remove(CLAVE_REFRESH);
      await (SecureStorage as any).remove(CLAVE_ROL);
      await (SecureStorage as any).remove(CLAVE_USER_ID);
    } catch (e) {
      
    }
  };

  const obtener_rol_activo = () => {
    return rol_activo.value;
  };

  return {
    token,
    refresh_token,
    autenticado,
    restaurando,
    rol_activo,
    usuario_id, 
    error_login,
    restaurar_sesion,
    guardar_sesion,
    login_con_google, 
    cerrar_sesion,
    obtener_rol_activo,
  };
});