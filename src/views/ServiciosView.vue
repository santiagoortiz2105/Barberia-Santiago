<template>
  <comp-page titulo="Santiago - Barbería">
    <template #header-actions>
      <div class="theme-toggle-wrapper">
        <ion-icon :icon="moonOutline" class="moon-icon"></ion-icon>
        <ion-toggle :checked="esOscuro" @ionChange="cambiarTema"></ion-toggle>
      </div>
    </template>

    <!-- Refresco de pantalla -->
    <ion-refresher slot="fixed" @ionRefresh="recargarDatos($event)">
      <ion-refresher-content
        pulling-text="Desliza hacia abajo para actualizar..."
        refreshing-spinner="circles"
        refreshing-text="Actualizando servicios...">
      </ion-refresher-content>
    </ion-refresher>

    <div class="cards-container">
      <!-- Botón Global para desplegar/ocultar el formulario de Alta (SOLO ADMINISTRADOR) -->
      <div v-if="sesion.rol_activo === 'administrador'" class="accion-global">
        <button class="btn-agregar" @click="abrirFormularioNuevo">
          {{ mostrandoFormulario && !editandoId ? '✖ Cancelar' : '+ Nuevo Servicio' }}
        </button>
      </div>

      <!-- Formulario Desplegable (SOLO ADMINISTRADOR) -->
      <div v-if="mostrandoFormulario && sesion.rol_activo === 'administrador'" class="form-nuevo-servicio">
        <h3>{{ editandoId ? 'Editar Servicio' : 'Nuevo Servicio / Corte' }}</h3>

        <div class="input-group">
          <label>Tipo de corte / Nombre</label>
          <input type="text" v-model="formServicio.nombre" placeholder="Ej. Corte Fade" />
        </div>

        <div class="input-group">
          <label>Descripción</label>
          <input type="text" v-model="formServicio.descripcion" placeholder="Ej. Incluye perfilado y navaja" />
        </div>

        <div class="input-group">
          <label>Precio ($)</label>
          <input type="number" v-model.number="formServicio.precio" placeholder="Ej. 9000" />
        </div>

        <div class="input-group">
          <label>Categoría</label>
          <select v-model.number="formServicio.categoriaId">
            <option v-for="cat in productosStore.categorias" :key="cat.id" :value="cat.id">
              {{ cat.nombre }}
            </option>
          </select>
        </div>

        <button class="btn-verde-agregar" @click="guardarServicio">
          {{ editandoId ? 'ACTUALIZAR SERVICIO' : 'AGREGAR SERVICIO' }}
        </button>
      </div>

      <h2 class="subtitulo-seccion">Servicios Disponibles</h2>

      <!-- 1. Estado Cargando -->
      <div v-if="productosStore.cargando" class="estado-box">
        <p>Cargando servicios...</p>
      </div>

      <!-- 2. Estado Error -->
      <div v-else-if="productosStore.error" class="estado-box error-box">
        <ion-icon :icon="cloudOfflineOutline" class="icono-error"></ion-icon>
        <h3>No se pudo cargar</h3>
        <p>{{ productosStore.error }}</p>
        <button class="btn-reintentar" @click="productosStore.cargar_productos()">REINTENTAR</button>
      </div>

      <!-- 3. Estado Vacío -->
      <div v-else-if="!productosStore.hay_productos" class="estado-box">
        <p>No hay productos disponibles por el momento.</p>
      </div>

      <!-- 4. Estado Éxito (Lista de Productos) -->
      <template v-else>
        <ion-card v-for="servicio in productosStore.productos" :key="servicio.id" class="servicio-card">
          <ion-card-content>
            <div class="card-header-row">
              <span class="precio">${{ servicio.precio }}</span>
              
              <!-- ACCIONES DE EDICIÓN Y BAJA: Exclusivas del Administrador -->
              <div v-if="sesion.rol_activo === 'administrador'" class="card-actions-buttons">
                <button class="btn-editar" @click="cargarParaEditar(servicio)">✏️ Editar</button>
                <button class="btn-eliminar" @click="confirmarBaja(servicio.id, servicio.nombre)">🗑️ Dar de baja</button>
              </div>
            </div>
            <h2 class="titulo-servicio">{{ servicio.nombre }}</h2>
            <p class="descripcion">{{ servicio.descripcion || 'Sin descripción' }}</p>
          </ion-card-content>
        </ion-card>
      </template>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { 
  IonCard, 
  IonCardContent, 
  IonToggle, 
  IonIcon, 
  IonRefresher, 
  IonRefresherContent, 
  alertController, 
  type RefresherCustomEvent 
} from '@ionic/vue';
import { moonOutline, cloudOfflineOutline } from 'ionicons/icons';
import compPage from '../components/base/comp_page.vue';
import { cargarTema, aplicarTema } from '../config/tema';
import { useProductosStore } from '@/stores/productos_store';
import { use_sesion_store } from '@/stores/sesion_store';

const productosStore = useProductosStore();
const sesion = use_sesion_store(); // <- Store de sesión para validar los roles
const esOscuro = ref(false);

const mostrandoFormulario = ref(false);
const editandoId = ref<number | null>(null);

const formServicio = reactive({
  nombre: '',
  descripcion: '',
  precio: 0,
  categoriaId: 1,
  disponible: true
});

onMounted(async () => {
  esOscuro.value = cargarTema();
  await productosStore.cargar_productos();
  if (productosStore.categorias.length > 0) {
    formServicio.categoriaId = productosStore.categorias[0].id;
  }
});

const cambiarTema = (e: CustomEvent) => {
  esOscuro.value = e.detail.checked;
  aplicarTema(esOscuro.value);
};

const recargarDatos = async (event: RefresherCustomEvent) => {
  await productosStore.cargar_productos();
  event.target.complete();
};

const mostrarAlerta = async (titulo: string, mensaje: string) => {
  const alert = await alertController.create({
    header: titulo,
    message: mensaje,
    buttons: ['Aceptar'],
    cssClass: 'custom-ionic-alert'
  });
  await alert.present();
};

const abrirFormularioNuevo = () => {
  if (mostrandoFormulario.value && !editandoId.value) {
    mostrandoFormulario.value = false;
  } else {
    mostrandoFormulario.value = true;
    editandoId.value = null;
    formServicio.nombre = '';
    formServicio.descripcion = '';
    formServicio.precio = 0;
    if (productosStore.categorias.length > 0) {
      formServicio.categoriaId = productosStore.categorias[0].id;
    }
  }
};

const cargarParaEditar = (servicio: any) => {
  editandoId.value = servicio.id;
  formServicio.nombre = servicio.nombre;
  formServicio.descripcion = servicio.descripcion || '';
  formServicio.precio = servicio.precio;
  formServicio.categoriaId = servicio.categoriaId || (productosStore.categorias[0]?.id ?? 1);
  mostrandoFormulario.value = true;
  window.scrollTo({ top: 0, behavior: 'smooth' });
};

const guardarServicio = async () => {
  if (!formServicio.nombre.trim()) {
    mostrarAlerta('Campo requerido', 'Por favor, ingresa el tipo de corte o nombre del servicio.');
    return;
  }
  if (formServicio.precio <= 0) {
    mostrarAlerta('Precio inválido', 'El precio debe ser mayor a 0.');
    return;
  }

  let res;
  if (editandoId.value) {
    res = await productosStore.modificar_producto(editandoId.value, { ...formServicio });
  } else {
    res = await productosStore.crear_producto({ ...formServicio });
  }

  if (!res.success) {
    mostrarAlerta('Error en la operación', res.mensaje);
  } else {
    formServicio.nombre = '';
    formServicio.descripcion = '';
    formServicio.precio = 0;
    mostrandoFormulario.value = false;
    editandoId.value = null;
    mostrarAlerta('¡Éxito!', editandoId.value ? 'Servicio actualizado correctamente.' : 'Servicio agregado correctamente.');
  }
};

const confirmarBaja = async (id: number, nombre: string) => {
  const alert = await alertController.create({
    header: 'Confirmar baja',
    message: `¿Estás seguro de dar de baja el servicio "${nombre}"?`,
    buttons: [
      {
        text: 'Cancelar',
        role: 'cancel'
      },
      {
        text: 'Sí, dar de baja',
        handler: async () => {
          const res = await productosStore.dar_de_baja_producto(id);
          if (!res.success) {
            mostrarAlerta('Error', res.mensaje);
          }
        }
      }
    ]
  });
  await alert.present();
};
</script>

<style scoped>
/* Tus estilos se mantienen intactos */
.theme-toggle-wrapper {
  display: flex;
  align-items: center;
  gap: 6px;
  padding-right: 12px;
}

.moon-icon {
  font-size: 1.2rem;
  color: var(--ion-color-step-650, #666);
}

.cards-container {
  padding: 12px;
}

.accion-global {
  margin-bottom: 16px;
  text-align: right;
}

.btn-agregar {
  background: #2dd36f;
  color: white;
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  font-weight: bold;
  cursor: pointer;
}

.form-nuevo-servicio {
  background: var(--ion-card-background, #ffffff);
  padding: 16px;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
  margin-bottom: 20px;
}

.form-nuevo-servicio h3 {
  font-size: 1.1rem;
  font-weight: 700;
  margin-bottom: 12px;
  color: var(--ion-text-color, #000);
}

.input-group {
  margin-bottom: 10px;
}

.input-group label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #666;
  margin-bottom: 4px;
}

.input-group input,
.input-group select {
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 0.95rem;
  background: var(--ion-background-color, #fff);
  color: var(--ion-text-color, #000);
}

.btn-verde-agregar {
  width: 100%;
  background: #2dd36f;
  color: white;
  border: none;
  padding: 12px;
  border-radius: 8px;
  font-weight: bold;
  font-size: 0.95rem;
  cursor: pointer;
  margin-top: 8px;
}

.subtitulo-seccion {
  font-size: 1.1rem;
  font-weight: 700;
  margin-bottom: 12px;
  color: var(--ion-text-color, #000);
}

.servicio-card {
  margin: 0 0 16px 0;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
  background: var(--ion-card-background, #ffffff);
}

.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.precio {
  font-size: 0.85rem;
  font-weight: 700;
  color: #3880ff;
}

.card-actions-buttons {
  display: flex;
  gap: 12px;
}

.btn-editar {
  background: transparent;
  border: none;
  color: #3880ff;
  font-size: 0.8rem;
  cursor: pointer;
  font-weight: 600;
}

.btn-eliminar {
  background: transparent;
  border: none;
  color: #eb444a;
  font-size: 0.8rem;
  cursor: pointer;
  font-weight: 600;
}

.titulo-servicio {
  font-size: 1.4rem;
  font-weight: 800;
  margin: 0 0 8px 0;
  color: var(--ion-text-color, #000);
}

.descripcion {
  font-size: 0.9rem;
  color: var(--ion-color-step-600, #666);
  margin: 0;
  line-height: 1.3;
}

.estado-box {
  text-align: center;
  padding: 40px 20px;
  border-radius: 12px;
  background: var(--ion-card-background, #ffffff);
  margin-top: 20px;
}

.icono-error {
  font-size: 3rem;
  color: #e74c3c;
  margin-bottom: 12px;
}

.btn-reintentar {
  margin-top: 16px;
  padding: 10px 24px;
  border: 1px solid #3880ff;
  background: transparent;
  color: #3880ff;
  border-radius: 6px;
  font-weight: bold;
  cursor: pointer;
}
</style>