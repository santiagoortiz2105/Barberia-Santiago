import { createRouter, createWebHistory } from '@ionic/vue-router';
import { RouteRecordRaw } from 'vue-router';
import { alertController } from '@ionic/vue';
import { navegacion } from '../config/navegacion';
import main_layout from '../layouts/main_layout.vue';
import { use_sesion_store } from '../stores/sesion_store';

const rutas_app: RouteRecordRaw[] = navegacion.map((item: any) => ({
  path: item.ruta.replace('/app/', ''),
  name: item.id,
  component: item.componente,
  meta: {
    roles: item.menu_roles || ['TODOS']
  }
}));

const routes: RouteRecordRaw[] = [
  { path: '/', redirect: '/login' },
  { 
    path: '/login', 
    component: () => import('../views/LoginView.vue'), 
    meta: { publica: true } 
  },
  { 
    path: '/app', 
    component: main_layout, 
    children: [
      ...rutas_app,
      // Ruta de escaneo de QR integrada dentro del layout principal /app
      {
        path: 'escanear-qr',
        name: 'EscanearQr',
        component: () => import('@/views/EscanearQrView.vue'),
        meta: { roles: ['ADMIN', 'BARBERO'] } // Protegida solo para administradores y barberos
      }
    ] 
  },
  { path: '/:pathMatch(.*)*', redirect: '/app/inicio' }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

// Bloquea de verdad según autenticación, roles y muestra el 403 sin congelar
router.beforeEach(async (to) => {
  const sesion = use_sesion_store();

  if (sesion.restaurando) {
    await sesion.restaurar_sesion();
  }

  if (to.meta.publica) {
    return sesion.autenticado ? { path: "/app/inicio", replace: true } : true;
  }

  if (!sesion.autenticado) {
    return { path: "/login", replace: true };
  }

  const roles_permitidos = to.meta.roles as string[];
  if (!roles_permitidos || roles_permitidos.includes('TODOS')) return true;

  const rolActivo = sesion.obtener_rol_activo();
  if (rolActivo && roles_permitidos.includes(rolActivo)) return true;

  // Autenticado pero sin permiso (Error 403 lógico )
  const alert = await alertController.create({
    header: 'Acceso Denegado (403)',
    message: 'No tienes los permisos necesarios para acceder a esta sección de la barbería.',
    buttons: ['Aceptar'],
    cssClass: 'custom-ionic-alert'
  });
  
  await alert.present();

  // Cancelamos la navegación en curso para evitar bucles o congelamientos
  return false;
});

export default router;