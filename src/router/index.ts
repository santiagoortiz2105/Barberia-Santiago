import { createRouter, createWebHistory } from '@ionic/vue-router';
import { RouteRecordRaw } from 'vue-router';
import { navegacion } from '../config/navegacion';
import main_layout from '../layouts/main_layout.vue';

const rutas_app: RouteRecordRaw[] = navegacion.map(item => ({
  path: item.ruta.replace('/app/', ''),
  name: item.id,
  component: item.componente
}));

const routes: RouteRecordRaw[] = [
  { path: '/', redirect: '/app/inicio' },
  { path: '/app', component: main_layout, children: [...rutas_app] },
  { path: '/:pathMatch(.*)*', redirect: '/app/inicio' }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

export default router;