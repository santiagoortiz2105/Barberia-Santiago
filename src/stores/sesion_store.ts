import { defineStore } from 'pinia';
import { ref } from 'vue';
import { SecureStorage } from '@aparajita/capacitor-secure-storage';

const CLAVE_TOKEN = 'tudominio_token';
const CLAVE_REFRESH = 'tudominio_refresh';
const CLAVE_ROL = 'tudominio_rol';
const CLAVE_USER_ID = 'tudominio_user_id'; // <-- Nueva clave

export const use_sesion_store = defineStore('sesion', () => {
  const token = ref<string | null>(null);
  const refresh_token = ref<string | null>(null);
  const autenticado = ref<boolean>(false);
  const restaurando = ref<boolean>(true);
  const rol_activo = ref<string | null>(null);
  const usuario_id = ref<any | null>(null); // <-- Nuevo estado para el ID

  const restaurar_sesion = async () => {
    try {
      restaurando.value = true;
      const tokenRes: any = await (SecureStorage as any).get(CLAVE_TOKEN);
      const rolRes: any = await (SecureStorage as any).get(CLAVE_ROL);
      const userIdRes: any = await (SecureStorage as any).get(CLAVE_USER_ID); // <-- Recuperar ID

      if (tokenRes) {
        token.value = typeof tokenRes === 'string' ? tokenRes : tokenRes.value;
        rol_activo.value = rolRes ? (typeof rolRes === 'string' ? rolRes : rolRes.value) : null;
        usuario_id.value = userIdRes ? (typeof userIdRes === 'string' ? userIdRes : userIdRes.value) : null;
        autenticado.value = true;
      }
    } catch (e) {
      autenticado.value = false;
    } finally {
      restaurando.value = false;
    }
  };

  const guardar_sesion = async (newToken: string, newRefresh: string, newRol: string, newUserId: any) => {
    token.value = newToken;
    refresh_token.value = newRefresh;
    rol_activo.value = newRol;
    usuario_id.value = newUserId; // <-- Guardar en memoria
    autenticado.value = true;

    await (SecureStorage as any).set(CLAVE_TOKEN, newToken);
    await (SecureStorage as any).set(CLAVE_REFRESH, newRefresh);
    await (SecureStorage as any).set(CLAVE_ROL, newRol);
    await (SecureStorage as any).set(CLAVE_USER_ID, String(newUserId)); // <-- Guardar en storage seguro
  };

  const cerrar_sesion = async () => {
    token.value = null;
    refresh_token.value = null;
    rol_activo.value = null;
    usuario_id.value = null; // <-- Limpiar ID
    autenticado.value = false;

    await (SecureStorage as any).remove(CLAVE_TOKEN);
    await (SecureStorage as any).remove(CLAVE_REFRESH);
    await (SecureStorage as any).remove(CLAVE_ROL);
    await (SecureStorage as any).remove(CLAVE_USER_ID);
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
    usuario_id, // <-- Exportar para usarlo en las vistas
    restaurar_sesion,
    guardar_sesion,
    cerrar_sesion,
    obtener_rol_activo,
  };
});