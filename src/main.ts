import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router';

import { IonicVue } from '@ionic/vue';
import { GoogleAuth } from '@codetrix-studio/capacitor-google-auth';

/* Core CSS required for Ionic components to work properly */
import '@ionic/vue/css/core.css';

/* Basic CSS for apps built with Ionic */
import '@ionic/vue/css/normalize.css';
import '@ionic/vue/css/structure.css';
import '@ionic/vue/css/typography.css';

/* Optional CSS utils that can be commented out */
import '@ionic/vue/css/padding.css';
import '@ionic/vue/css/float-elements.css';
import '@ionic/vue/css/text-alignment.css';
import '@ionic/vue/css/text-transformation.css';
import '@ionic/vue/css/flex-utils.css';
import '@ionic/vue/css/display.css';

/**
 * Ionic Dark Mode
 */
import '@ionic/vue/css/palettes/dark.system.css';

/* Theme variables */
import './theme/variables.css';

// Inicialización de Google Auth para Capacitor
GoogleAuth.initialize({
  clientId: '203460388544-k5edkijusrvvu2gjqvr78igf959ogkli.apps.googleusercontent.com', 
  scopes: ['profile', 'email'],
  grantOfflineAccess: true,
});

const app = createApp(App);
const pinia = createPinia();

app.use(pinia);
app.use(IonicVue);
app.use(router);

router.isReady().then(() => {
  app.mount('#app');
});