<template>
  <comp-page titulo="Barberos">
    <div class="cards-container">
      
      <!-- Botón para Administrador: Agregar Barbero por Gmail -->
      <div class="admin-actions" v-if="esAdmin">
        <button class="btn-agregar" @click="pedirEmailNuevoBarbero">
          ➕ Asignar nuevo barbero por Gmail
        </button>
      </div>

      <!-- Estado Cargando -->
      <div v-if="cargando" class="estado-box">
        <p>Cargando barberos...</p>
      </div>

      <!-- Estado Vacío -->
      <div v-else-if="barberos.length === 0" class="estado-box">
        <p>No hay barberos registrados.</p>
      </div>

      <!-- Lista de Barberos -->
      <template v-else>
        <ion-card v-for="barbero in barberos" :key="barbero.id" class="barbero-card">
          <ion-card-content>
            <div class="barbero-info">
              <h2 class="titulo-barbero">{{ barbero.nombre || barbero.email }}</h2>
              <p class="sub-barbero">📧 {{ barbero.email }}</p>
              <p class="desc-barbero">{{ barbero.especialidad || 'Barbero profesional' }}</p>
            </div>

            <!-- Botón de Quitar Rol (Solo Administrador) -->
            <div class="card-actions" v-if="esAdmin">
              <button class="btn-quitar" @click="confirmarQuitarBarbero(barbero.id)">
                🗑️ Quitar rol de barbero
              </button>
            </div>
          </ion-card-content>
        </ion-card>
      </template>

    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { IonCard, IonCardContent, alertController } from '@ionic/vue';
import compPage from '../components/base/comp_page.vue';
import { use_sesion_store } from '../stores/sesion_store';
import { obtener_api_url } from '../config/debug';

const sesion = use_sesion_store();
const apiUrl = obtener_api_url();

const barberos = ref<any[]>([]);
const cargando = ref(false);

const rolActivoUpper = computed(() => {
  const r = sesion.obtener_rol_activo() || sesion.rol_activo;
  return r ? r.toUpperCase() : '';
});
const esAdmin = computed(() => rolActivoUpper.value === 'ADMIN');

const obtenerTokenLimpio = () => {
  const token = localStorage.getItem('tudominio_token') || localStorage.getItem('token') || localStorage.getItem('access_token') || sesion.token;
  return token ? token.replace(/^["']|["']$/g, '').trim() : '';
};

const cargarBarberos = async () => {
  cargando.value = true;
  try {
    const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');
    const token = obtenerTokenLimpio();

    const response = await fetch(`${baseClean}/api/barberos`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) throw new Error('No se pudo cargar la lista de barberos');
    const data = await response.json();
    barberos.value = data || [];
  } catch (error) {
    console.error('Error cargando barberos:', error);
  } finally {
    cargando.value = false;
  }
};

const pedirEmailNuevoBarbero = async () => {
  const alert = await alertController.create({
    header: 'Nuevo Barbero',
    message: 'Ingresa el correo electrónico (Gmail) del cliente registrado:',
    inputs: [
      {
        name: 'email',
        type: 'email',
        placeholder: 'cliente@gmail.com'
      }
    ],
    buttons: [
      { text: 'Cancelar', role: 'cancel' },
      {
        text: 'Asignar',
        handler: async (data) => {
          if (!data.email) return false;
          await ascenderAbarbero(data.email.trim());
        }
      }
    ]
  });
  await alert.present();
};

const ascenderAbarbero = async (email: string) => {
  try {
    const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');
    const token = obtenerTokenLimpio();

    const response = await fetch(`${baseClean}/api/barberos/promover-barbero`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ email })
    });

    const result = await response.json();
    if (!response.ok) throw new Error(result.mensaje || 'No se pudo promover al usuario');

    await cargarBarberos();
    
    const alertaExito = await alertController.create({
      header: 'Éxito',
      message: 'El usuario ahora es barbero correctamente.',
      buttons: ['Aceptar']
    });
    await alertaExito.present();
  } catch (error: any) {
    const alertaError = await alertController.create({
      header: 'Error',
      message: error.message || 'Ocurrió un error al procesar la solicitud.',
      buttons: ['Aceptar']
    });
    await alertaError.present();
  }
};

const confirmarQuitarBarbero = async (id: number) => {
  const alert = await alertController.create({
    header: 'Confirmar acción',
    message: '¿Estás seguro de quitarle el rol de barbero a este usuario?',
    buttons: [
      { text: 'Cancelar', role: 'cancel' },
      {
        text: 'Sí, quitar',
        handler: async () => {
          try {
            const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');
            const token = obtenerTokenLimpio();

            const response = await fetch(`${baseClean}/api/barberos/${id}`, {
              method: 'DELETE',
              headers: { 'Authorization': `Bearer ${token}` }
            });

            if (!response.ok) throw new Error('No se pudo quitar el rol');

            barberos.value = barberos.value.filter(b => b.id !== id);
          } catch (error) {
            console.error('Error al quitar barbero:', error);
          }
        }
      }
    ]
  });
  await alert.present();
};

onMounted(() => {
  cargarBarberos();
});
</script>

<style scoped>
.cards-container {
  padding: 16px;
  background-color: var(--ion-background-color, #f4f5f8);
}

.admin-actions {
  margin-bottom: 16px;
}

.btn-agregar {
  width: 100%;
  background: #3880ff;
  color: white;
  border: none;
  padding: 13px;
  border-radius: 12px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  box-shadow: 0 3px 10px rgba(56, 128, 255, 0.3);
}

.barbero-card {
  margin: 0 0 14px 0;
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
  background: var(--ion-card-background, #ffffff);
}

.titulo-barbero {
  font-size: 1.15rem;
  font-weight: 800;
  margin: 0 0 6px 0;
  color: var(--ion-text-color, #000);
}

.sub-barbero {
  font-size: 0.85rem;
  color: var(--ion-color-step-600, #666);
  margin: 0 0 4px 0;
  font-weight: 500;
}

.desc-barbero {
  font-size: 0.85rem;
  color: var(--ion-color-step-700, #444);
  margin: 0;
}

.card-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 14px;
  border-top: 1px solid var(--ion-color-step-200, #eee);
  padding-top: 12px;
}

.btn-quitar {
  background: #fff5f5;
  color: #eb444a;
  border: 1px solid #fed7d7;
  padding: 9px 14px;
  border-radius: 9px;
  font-weight: 700;
  font-size: 0.8rem;
  cursor: pointer;
}

.estado-box {
  text-align: center;
  padding: 40px 20px;
  color: var(--ion-color-step-600, #666);
  background: var(--ion-card-background, #ffffff);
  border-radius: 14px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.03);
}
</style>