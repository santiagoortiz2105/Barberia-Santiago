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
      {
        path: 'escanear-qr',
        name: 'EscanearQr',
        component: () => import('@/views/EscanearQrView.vue'),
        meta: { roles: ['ADMIN', 'BARBERO'] }
      }
    ] 
  },
  { path: '/:pathMatch(.*)*', redirect: '/app/inicio' }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

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

  const roles_permitidos = (to.meta.roles as string[]) || ['TODOS'];
  if (roles_permitidos.includes('TODOS')) return true;

  const rolActivo = sesion.obtener_rol_activo();
  const rolActivoUpper = rolActivo ? rolActivo.toUpperCase() : '';
  
  // 🔍 LOG DE DIAGNÓSTICO: Abre la consola para ver qué valores se están evaluando
  console.log(`[Router Guard] Ruta destino: ${to.path} | Rol Activo en Store:`, rolActivo, `(Upper: ${rolActivoUpper}) | Roles permitidos:`, roles_permitidos);

  // Si el usuario es ADMIN, pase libre absoluto
  if (rolActivoUpper === 'ADMIN') return true;

  const rolesPermitidosUpper = roles_permitidos.map(r => r.toUpperCase());
  if (rolActivoUpper && rolesPermitidosUpper.includes(rolActivoUpper)) return true;

  // Si llega aquí, dispara el 403
  const alert = await alertController.create({
    header: 'Acceso Denegado (403)',
    message: `No tienes permisos. Ruta: ${to.path} - Tu rol: ${rolActivo}`,
    buttons: ['Aceptar'],
    cssClass: 'custom-ionic-alert'
  });
  
  await alert.present();
  return false;
});

export default router;