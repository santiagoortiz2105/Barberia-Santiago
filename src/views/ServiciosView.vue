<template>
  <comp-page titulo="Santiago - Barbería">
    <!-- Banner informativo de estado sin conexión -->
    <div v-if="offlineStore.sinConexion" class="banner-offline">
      <ion-icon :icon="cloudOfflineOutline"></ion-icon>
      <span>Sin conexión: la reserva se guardará y se enviará al volver la red.</span>
    </div>

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
      <div v-if="esAdministrador" class="accion-global">
        <button class="btn-agregar" @click="abrirFormularioNuevo">
          {{ mostrandoFormulario && !editandoId ? '✖ Cancelar' : '+ Nuevo Servicio' }}
        </button>
      </div>

      <!-- Formulario Desplegable (SOLO ADMINISTRADOR) -->
      <div v-if="mostrandoFormulario && esAdministrador" class="form-nuevo-servicio">
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
              <div v-if="esAdministrador" class="card-actions-buttons">
                <button class="btn-editar" @click="cargarParaEditar(servicio)">Editar</button>
                <button class="btn-eliminar" @click="confirmarBaja(servicio.id, servicio.nombre)">Dar de baja</button>
              </div>
            </div>
            <h2 class="titulo-servicio">{{ servicio.nombre }}</h2>
            <p class="descripcion">{{ servicio.descripcion || 'Sin descripción' }}</p>

            <!-- ACCIÓN DE RESERVA: Exclusiva para Clientes / Usuarios No Administradores -->
            <div v-if="!esAdministrador" class="mt-3">
              <button class="btn-reservar" @click="iniciarProcesoReserva(servicio)">
                Reservar este servicio
              </button>
            </div>
          </ion-card-content>
        </ion-card>
      </template>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue';
import { 
  IonCard, 
  IonCardContent, 
  IonIcon, 
  IonRefresher, 
  IonRefresherContent, 
  alertController, 
  type RefresherCustomEvent 
} from '@ionic/vue';
import { cloudOfflineOutline } from 'ionicons/icons';
import compPage from '../components/base/comp_page.vue';
import { useProductosStore } from '@/stores/productos_store';
import { use_sesion_store } from '@/stores/sesion_store';
import { useTurnosStore } from '@/stores/turnos_stores';
import { use_offline_store } from '@/stores/offline_store';
import ajax_service from '@/ajax_service';

const productosStore = useProductosStore();
const sesion = use_sesion_store();
const turnosStore = useTurnosStore();
const offlineStore = use_offline_store();

const mostrandoFormulario = ref(false);
const editandoId = ref<number | null>(null);

const esAdministrador = computed(() => {
  const rol = sesion.rol_activo || '';
  const rolLower = rol.toLowerCase();
  return rolLower === 'admin' || rolLower === 'administrador';
});

const formServicio = reactive({
  nombre: '',
  descripcion: '',
  precio: 0,
  categoriaId: 1,
  disponible: true
});

onMounted(async () => {
  await offlineStore.inicializarRed();
  await productosStore.cargar_productos();
  if (productosStore.categorias.length > 0) {
    formServicio.categoriaId = productosStore.categorias[0].id;
  }
});

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
      { text: 'Cancelar', role: 'cancel' },
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

const generarHorariosDisponibles = () => {
  const horarios = [];
  let horaActualMinutos = 15 * 60; // 15:00
  const horaFinMinutos = 21 * 60;   // 21:00
  const duracionTurno = 40;         // 40 minutos

  while (horaActualMinutos + duracionTurno <= horaFinMinutos) {
    const horaInicio = Math.floor(horaActualMinutos / 60);
    const minInicio = horaActualMinutos % 60;
    
    const horaFinCalculada = horaActualMinutos + duracionTurno;
    const horaFin = Math.floor(horaFinCalculada / 60);
    const minFin = horaFinCalculada % 60;

    const formatoHora = `${String(horaInicio).padStart(2, '0')}:${String(minInicio).padStart(2, '0')} - ${String(horaFin).padStart(2, '0')}:${String(minFin).padStart(2, '0')}`;
    
    horarios.push(formatoHora);
    horaActualMinutos += duracionTurno;
  }
  return horarios;
};

const iniciarProcesoReserva = async (servicio: any) => {
  let listaBarberos = [];
  try {
    const resBarberos = await ajax_service.ajax_get('/barberos');
    listaBarberos = Array.isArray(resBarberos) ? resBarberos : [];
  } catch (e) {
    console.error("No se pudieron cargar los barberos", e);
  }

  if (listaBarberos.length === 0) {
    mostrarAlerta('Aviso', 'No hay barberos disponibles registrados en este momento. Asigna al menos uno desde la sección de Barberos.');
    return;
  }

  const inputsBarberos: any[] = listaBarberos.map((b: any, index: number) => ({
    type: 'radio' as const,
    label: b.nombre || b.email || `Barbero ID: ${b.id}`,
    value: b.id,
    checked: index === 0
  }));

  const alertBarbero = await alertController.create({
    header: '1. Selecciona un Barbero',
    inputs: inputsBarberos,
    buttons: [
      { text: 'Cancelar', role: 'cancel' },
      {
        text: 'Siguiente',
        handler: async (barberoIdSeleccionado) => {
          if (!barberoIdSeleccionado) {
            mostrarAlerta('Error', 'Debes seleccionar un barbero.');
            return;
          }
          await abrirSelectorFecha(servicio, barberoIdSeleccionado);
        }
      }
    ]
  });

  await alertBarbero.present();
};

const abrirSelectorFecha = async (servicio: any, barberoId: number) => {
  const hoyStr = new Date().toISOString().split('T')[0];

  const alertFecha = await alertController.create({
    header: '2. Selecciona la Fecha',
    inputs: [
      {
        name: 'fechaSeleccionada',
        type: 'date',
        min: hoyStr,
        value: hoyStr
      }
    ],
    buttons: [
      { text: 'Atrás', role: 'cancel' },
      {
        text: 'Siguiente',
        handler: async (data) => {
          const fechaElegida = data.fechaSeleccionada;
          if (!fechaElegida) {
            mostrarAlerta('Error', 'Debes seleccionar una fecha válida.');
            return;
          }
          await abrirSelectorHorario(servicio, barberoId, fechaElegida);
        }
      }
    ]
  });

  await alertFecha.present();
};

const abrirSelectorHorario = async (servicio: any, barberoId: number, fechaSeleccionada: string) => {
  const horarios = generarHorariosDisponibles();
  
  const inputsHorarios: any[] = horarios.map((h, index) => ({
    type: 'radio' as const,
    label: h,
    value: h,
    checked: index === 0
  }));

  const alertHorario = await alertController.create({
    header: '3. Elige el Horario',
    message: `Servicio: ${servicio.nombre}\nFecha: ${fechaSeleccionada}\nFranja: 15:00 - 21:00 hs (40 min c/u)`,
    inputs: inputsHorarios,
    buttons: [
      { text: 'Cancelar', role: 'cancel' },
      {
        text: 'Confirmar Reserva',
        handler: async (horarioElegido) => {
          if (!horarioElegido) {
            mostrarAlerta('Error', 'Debes seleccionar un horario.');
            return;
          }

          const payload = {
            productoId: servicio.id,
            barberoId: Number(barberoId),
            horario: horarioElegido,
            fechaHora: `${fechaSeleccionada}T00:00:00`
          };

          if (offlineStore.sinConexion) {
            const turnoPendiente = offlineStore.encolarCambio(payload);
            if (turnosStore.turnos) {
              turnosStore.turnos.unshift(turnoPendiente);
            }
            mostrarAlerta('Guardado (Sin Conexión)', 'Tu reserva quedó en cola local y se sincronizará al volver la red.');
            return;
          }

          try {
            const res = await ajax_service.ajax_post('/turnos', payload);
            if (turnosStore.turnos) {
              turnosStore.turnos.unshift(res);
            }
            mostrarAlerta('¡Turno Reservado!', `Tu reserva para "${servicio.nombre}" el ${fechaSeleccionada} a las ${horarioElegido} fue registrada con éxito.`);
          } catch (err: any) {
            console.error('Error al reservar turno:', err);
            mostrarAlerta('Error', 'No se pudo procesar la reserva en el servidor.');
          }
        }
      }
    ]
  });

  await alertHorario.present();
};
</script>

<style scoped>
.banner-offline {
  background: #ffc409;
  color: #000;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 10px 14px;
  font-size: 0.85rem;
  font-weight: 600;
  text-align: center;
  border-bottom: 1px solid rgba(0,0,0,0.1);
}

.cards-container {
  padding: 16px;
  max-width: 600px;
  margin: 0 auto;
}

.accion-global {
  margin-bottom: 20px;
  display: flex;
  justify-content: flex-end;
}

.btn-agregar {
  background: linear-gradient(135deg, #2dd36f, #28ba62);
  color: white;
  border: none;
  padding: 12px 20px;
  border-radius: 12px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(45, 211, 111, 0.3);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.btn-agregar:active {
  transform: scale(0.97);
}

.form-nuevo-servicio {
  background: var(--ion-card-background, #ffffff);
  padding: 20px;
  border-radius: 16px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
  margin-bottom: 24px;
  border: 1px solid rgba(0, 0, 0, 0.04);
}

.form-nuevo-servicio h3 {
  font-size: 1.2rem;
  font-weight: 800;
  margin-bottom: 16px;
  color: var(--ion-text-color, #000);
}

.input-group {
  margin-bottom: 14px;
}

.input-group label {
  display: block;
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--ion-color-step-600, #666);
  margin-bottom: 6px;
}

.input-group input,
.input-group select {
  width: 100%;
  padding: 12px;
  border: 1px solid var(--ion-color-step-300, #ccc);
  border-radius: 10px;
  font-size: 1rem;
  background: var(--ion-background-color, #fff);
  color: var(--ion-text-color, #000);
  outline: none;
  transition: border-color 0.2s;
}

.input-group input:focus,
.input-group select:focus {
  border-color: #3880ff;
}

.btn-verde-agregar {
  width: 100%;
  background: linear-gradient(135deg, #2dd36f, #28ba62);
  color: white;
  border: none;
  padding: 14px;
  border-radius: 10px;
  font-weight: 800;
  font-size: 1rem;
  cursor: pointer;
  margin-top: 10px;
  box-shadow: 0 4px 12px rgba(45, 211, 111, 0.3);
}

.subtitulo-seccion {
  font-size: 1.25rem;
  font-weight: 800;
  margin-bottom: 16px;
  color: var(--ion-text-color, #000);
  letter-spacing: -0.5px;
}

.servicio-card {
  margin: 0 0 16px 0;
  border-radius: 16px;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.06);
  background: var(--ion-card-background, #ffffff);
  border: 1px solid rgba(0, 0, 0, 0.03);
  transition: transform 0.2s ease;
}

.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
}

.precio {
  font-size: 1rem;
  font-weight: 800;
  color: #3880ff;
  background: rgba(56, 128, 255, 0.1);
  padding: 4px 10px;
  border-radius: 8px;
}

.card-actions-buttons {
  display: flex;
  gap: 10px;
}

.btn-editar {
  background: rgba(56, 128, 255, 0.1);
  border: none;
  color: #3880ff;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 0.8rem;
  cursor: pointer;
  font-weight: 700;
}

.btn-eliminar {
  background: rgba(235, 68, 74, 0.1);
  border: none;
  color: #eb444a;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 0.8rem;
  cursor: pointer;
  font-weight: 700;
}

.titulo-servicio {
  font-size: 1.35rem;
  font-weight: 800;
  margin: 4px 0 6px 0;
  color: var(--ion-text-color, #000);
}

.descripcion {
  font-size: 0.9rem;
  color: var(--ion-color-step-600, #666);
  margin: 0;
  line-height: 1.4;
}

.mt-3 {
  margin-top: 14px;
}

.btn-reservar {
  width: 100%;
  background: linear-gradient(135deg, #3880ff, #3171e0);
  color: white;
  border: none;
  padding: 12px;
  border-radius: 10px;
  font-weight: 800;
  font-size: 0.95rem;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(56, 128, 255, 0.3);
  transition: transform 0.2s ease;
}

.btn-reservar:active {
  transform: scale(0.98);
}

.estado-box {
  text-align: center;
  padding: 40px 20px;
  border-radius: 16px;
  background: var(--ion-card-background, #ffffff);
  margin-top: 20px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
}

.icono-error {
  font-size: 3rem;
  color: #eb444a;
  margin-bottom: 12px;
}

.btn-reintentar {
  margin-top: 16px;
  padding: 10px 24px;
  border: 2px solid #3880ff;
  background: transparent;
  color: #3880ff;
  border-radius: 8px;
  font-weight: 700;
  cursor: pointer;
}
</style>