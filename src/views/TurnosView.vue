<template>
  <comp-page titulo="Gestión de Turnos">
    <!-- Indicador de Estado de Conexión -->
    <div v-if="offlineStore.sinConexion" class="banner-offline">
      <span>Modo sin conexión activo</span>
    </div>

    <!-- Refresco de pantalla -->
    <ion-refresher slot="fixed" @ionRefresh="recargarTodo($event)">
      <ion-refresher-content
        pulling-text="Desliza hacia abajo para actualizar..."
        refreshing-spinner="circles"
        refreshing-text="Actualizando...">
      </ion-refresher-content>
    </ion-refresher>

    <div class="cards-container">
      
      <!-- ================= SECCIÓN DE NOTIFICACIONES / BANDEJA (ARRIBA) ================= -->
      <div class="notificaciones-section" v-if="esAdmin || rolActivoUpper === 'BARBERO'">
        <div class="header-bandeja">
          <h3 class="subtitulo-seccion">🔔 Bandeja de Turnos Nuevos</h3>
          <span class="badge-contador">
            {{ notificaciones.length }} en bandeja
          </span>
        </div>
        
        <!-- Estado Vacío o Lista de Notificaciones -->
        <div v-if="notificaciones.length === 0" class="estado-box-pequeno">
          <p>No hay notificaciones en la bandeja.</p>
        </div>

        <div 
          v-else
          v-for="notif in notificaciones" 
          :key="notif.id" 
          class="notif-card"
        >
          <div class="notif-content">
            <div class="notif-header-row">
              <span class="notif-titulo">{{ notif.titulo }}</span>
              <span class="notif-fecha">{{ formatearFecha(notif.fechaCreacion) }}</span>
            </div>
            
            <p class="notif-mensaje">{{ notif.mensaje }}</p>

            <!-- Botón de Borrado Manual -->
            <div class="notif-actions">
              <button class="btn-borrar-notif" @click="borrarNotificacion(notif.id)">
                🗑️ Borrar
              </button>
            </div>
          </div>
        </div>
        
        <div class="divisor-seccion"></div>
      </div>
      <!-- =============================================================================== -->

      <h2 class="subtitulo-seccion">
        {{ tituloSeccion }}
      </h2>

      <!-- Estado Cargando -->
      <div v-if="turnosStore.cargando" class="estado-box">
        <p>Cargando turnos...</p>
      </div>

      <!-- Estado Vacío -->
      <div v-else-if="listaTurnosCombinada.length === 0" class="estado-box">
        <p>No hay turnos registrados en este momento.</p>
      </div>

      <!-- Lista de Turnos (Servidor + Cola Local) -->
      <template v-else>
        <ion-card v-for="turno in listaTurnosCombinada" :key="turno.id || turno.id_temporal" class="turno-card">
          <ion-card-content>
            <div class="card-header-row">
              <span class="fecha-turno">📅 {{ formatearFecha(turno.fechaHora || turno.fecha) }}</span>
              <span class="precio">${{ turno.precio || turno.precioServicio || '0' }}</span>
            </div>

            <h2 class="titulo-servicio">
              {{ turno.producto || turno.servicio_nombre || turno.nombreServicio || 'Corte de Cabello' }}
              <span v-if="turno.sin_enviar || turno.sinEnviar" class="badge-sin-enviar">⚠️ Sin enviar</span>
            </h2>
            
            <div class="detalles-info">
             <p><strong>Cliente:</strong> {{ turno.clienteNombre || 'Usuario' }}</p>
              <p><strong>Barbero:</strong> {{ turno.barberoNombre || 'Por asignar' }}</p>
              <p v-if="turno.estado"><strong>Estado:</strong> {{ turno.estado }}</p>
              <p v-else-if="turno.sin_enviar"><strong>Estado:</strong> Pendiente de envío</p>
            </div>

            <!-- SECCIÓN CÓDIGO QR -->
            <div class="qr-container" v-if="turno.id && !offlineStore.sinConexion">
              <p class="qr-label">Código QR del Turno:</p>
              <img 
                v-if="qrUrlsMap[turno.id]"
                :src="qrUrlsMap[turno.id]" 
                alt="QR del Turno" 
                class="qr-imagen"
              />
              <div v-else class="qr-cargando">
                <p style="font-size: 0.75rem; color: #888;">Cargando QR...</p>
              </div>
            </div>

            <!-- Acciones -->
            <div class="card-actions">
              <button v-if="turno.id" class="btn-pdf" @click="descargarYCompartirPdf(turno.id)">
                📄 Comprobante PDF
              </button>

              <button 
                v-if="esAdmin && turno.id" 
                class="btn-eliminar" 
                @click="confirmarBajaTurno(turno.id)">
                🗑️ Dar de baja
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
import { 
  IonCard, 
  IonCardContent, 
  IonRefresher, 
  IonRefresherContent, 
  alertController, 
  type RefresherCustomEvent 
} from '@ionic/vue';
import { Filesystem, Directory } from '@capacitor/filesystem';
import { Share } from '@capacitor/share';
import { Capacitor } from '@capacitor/core';

// Componentes y Stores
import compPage from '../components/base/comp_page.vue';
import { use_sesion_store } from '../stores/sesion_store';
import { useTurnosStore } from '../stores/turnos_stores'; 
import { use_offline_store } from '../stores/offline_store';
import { obtener_api_url } from '../config/debug';
import ajax_service from '../ajax_service';

const sesion = use_sesion_store();
const turnosStore = useTurnosStore();
const offlineStore = use_offline_store();

const notificaciones = ref<any[]>([]);
const qrUrlsMap = ref<{ [key: number]: string }>({});
const apiUrl = obtener_api_url();

// Normalización segura del rol en mayúsculas para evitar fallas de comparación
const rolActivoUpper = computed(() => {
  const r = sesion.obtener_rol_activo() || sesion.rol_activo;
  return r ? r.toUpperCase() : '';
});

const esAdmin = computed(() => rolActivoUpper.value === 'ADMIN' || rolActivoUpper.value === 'ADMINISTRADOR');

const tituloSeccion = computed(() => {
  if (esAdmin.value) return 'Todos los Turnos (Admin)';
  if (rolActivoUpper.value === 'BARBERO') return 'Turnos Asignados (Barbero)';
  return 'Mis Turnos Reservados';
});

// Combinación unificada de turnos encolados pendientes (offline) + turnos del servidor
const listaTurnosCombinada = computed(() => {
  const turnosServidor = turnosStore.turnos || [];
  const colaPendiente = offlineStore.colaCambios || [];
  return [...colaPendiente, ...turnosServidor];
});

onMounted(async () => {
  await offlineStore.inicializarRed();
  await cargarTodo();
});

const cargarTodo = async () => {
  await Promise.all([
    turnosStore.cargarTurnos(), 
    cargarNotificaciones()
  ]);
  await cargarCodigosQr();
};

const cargarCodigosQr = async () => {
  const turnos = turnosStore.turnos || [];
  const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');
  const tokenLimpio = obtenerTokenLimpio();

  for (const turno of turnos) {
    if (turno.id && !qrUrlsMap.value[turno.id]) {
      try {
        const response = await fetch(`${baseClean}/api/turnos/${turno.id}/qr`, {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${tokenLimpio || sesion.token}`
          }
        });

        if (!response.ok) throw new Error('No se pudo obtener el QR');

        const blob = await response.blob();
        qrUrlsMap.value[turno.id] = window.URL.createObjectURL(blob);
      } catch (e) {
        console.error(`No se pudo cargar el QR para el turno ${turno.id}:`, e);
      }
    }
  }
};

// Función auxiliar para obtener el token limpio
const obtenerTokenLimpio = () => {
  const tokenGuardado = localStorage.getItem('tudominio_token') || localStorage.getItem('token') || sesion.token;
  return tokenGuardado ? tokenGuardado.replace(/^["']|["']$/g, '').trim() : '';
};

const cargarNotificaciones = async () => {
  try {
    const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');
    const tokenLimpio = obtenerTokenLimpio();

    const response = await fetch(`${baseClean}/api/turnos/bandeja`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${tokenLimpio || sesion.token}`
      }
    });

    if (!response.ok) throw new Error('Error al cargar la bandeja');
    
    const data = await response.json();
    notificaciones.value = data || [];
    localStorage.setItem('copia_notificaciones', JSON.stringify(notificaciones.value));
  } catch (error) {
    console.error('Error al cargar notificaciones (offline o servidor):', error);
    const copia = localStorage.getItem('copia_notificaciones');
    notificaciones.value = copia ? JSON.parse(copia) : [];
  }
};

const borrarNotificacion = async (id: number) => {
  try {
    notificaciones.value = notificaciones.value.filter(n => n.id !== id);
    localStorage.setItem('copia_notificaciones', JSON.stringify(notificaciones.value));

    if (!offlineStore.sinConexion) {
      const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');
      const tokenLimpio = obtenerTokenLimpio();
      await fetch(`${baseClean}/api/turnos/notificacion/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${tokenLimpio}`
        }
      });
    }
  } catch (error) {
    console.error('Error al borrar la notificación:', error);
  }
};

const recargarTodo = async (event: RefresherCustomEvent) => {
  await cargarTodo();
  event.target.complete();
};

const mostrarAlerta = async (titulo: string, mensaje: string) => {
  const alert = await alertController.create({
    header: titulo,
    message: mensaje,
    buttons: ['Aceptar']
  });
  await alert.present();
};

const confirmarBajaTurno = async (id: number) => {
  const alert = await alertController.create({
    header: 'Confirmar baja',
    message: '¿Estás seguro de dar de baja este turno?',
    buttons: [
      { text: 'Cancelar', role: 'cancel' },
      {
        text: 'Sí, dar de baja',
        handler: async () => {
          try {
            const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');
            const tokenLimpio = obtenerTokenLimpio();
            
            const res = await fetch(`${baseClean}/api/turnos/${id}`, {
              method: 'DELETE',
              headers: {
                'Authorization': `Bearer ${tokenLimpio}`
              }
            });

            if(!res.ok) throw new Error('No se pudo eliminar');

            turnosStore.turnos = turnosStore.turnos.filter((t: any) => t.id !== id);
            mostrarAlerta('Éxito', 'Turno dado de baja correctamente.');
          } catch (e) {
            mostrarAlerta('Error', 'Ocurrió un error de conexión al intentar eliminar.');
          }
        }
      }
    ]
  });
  await alert.present();
};

const descargarYCompartirPdf = async (idTurno: number) => {
  try {
    const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');
    const urlPdf = `${baseClean}/api/comprobantes/turno/${idTurno}/pdf`;
    
    const tokenLimpio = obtenerTokenLimpio();

    const response = await fetch(urlPdf, {
      headers: {
        'Authorization': `Bearer ${tokenLimpio}`
      }
    });
    
    if (!response.ok) throw new Error('No se pudo obtener el comprobante.');
    
    const blob = await response.blob();
    const fileName = `comprobante_turno_${idTurno}.pdf`;

    const esNativo = Capacitor.isNativePlatform();

    if (esNativo) {
      const base64Data = await convertBlobToBase64(blob);

      const savedFile = await Filesystem.writeFile({
        path: fileName,
        data: base64Data,
        directory: Directory.Cache,
      });

      await Share.share({
        title: 'Comprobante de Turno',
        text: `Aquí tienes el comprobante del turno #${idTurno} en Barbería Santiago.`,
        url: savedFile.uri,
        dialogTitle: 'Compartir Comprobante',
      });
    } else {
      const blobUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = blobUrl;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      
      document.body.removeChild(link);
      window.URL.revokeObjectURL(blobUrl);
    }

  } catch (error) {
    console.error('Error al generar o compartir PDF:', error);
    mostrarAlerta('Error', 'No se pudo generar o descargar el comprobante PDF.');
  }
};

const convertBlobToBase64 = (blob: Blob) => new Promise<string>((resolve, reject) => {
  const reader = new FileReader();
  reader.onerror = reject;
  reader.onload = () => resolve(reader.result as string);
  reader.readAsDataURL(blob);
});

const formatearFecha = (fechaIso: string) => {
  if (!fechaIso) return 'Fecha no especificada';
  try {
    return new Date(fechaIso).toLocaleString();
  } catch {
    return fechaIso;
  }
};
</script>

<style scoped>
.cards-container {
  padding: 16px;
  background-color: var(--ion-background-color, #f4f5f8);
  max-width: 600px;
  margin: 0 auto;
}

.banner-offline {
  background: #ffc409;
  color: #000;
  text-align: center;
  padding: 8px 12px;
  font-weight: bold;
  font-size: 0.85rem;
}

.badge-sin-enviar {
  background: #ff4961;
  color: white;
  font-size: 0.65rem;
  padding: 3px 8px;
  border-radius: 6px;
  margin-left: 6px;
  vertical-align: middle;
  font-weight: 600;
}

.subtitulo-seccion {
  font-size: 1.15rem;
  font-weight: 800;
  margin-bottom: 14px;
  color: var(--ion-text-color, #000);
  letter-spacing: -0.3px;
}

.notificaciones-section {
  margin-bottom: 20px;
}

.header-bandeja {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.badge-contador {
  background: #3880ff;
  color: white;
  font-size: 0.7rem;
  padding: 3px 10px;
  border-radius: 12px;
  font-weight: 700;
}

.notif-card {
  margin: 0 0 10px 0;
  border-radius: 12px;
  padding: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  background: var(--ion-card-background, #ffffff);
  border-left: 4px solid #eb444a;
}

.notif-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.notif-titulo {
  font-size: 0.9rem;
  font-weight: 700;
  color: var(--ion-text-color, #000);
}

.notif-fecha {
  font-size: 0.7rem;
  color: var(--ion-color-step-600, #666);
}

.notif-mensaje {
  font-size: 0.85rem;
  color: var(--ion-color-step-700, #444);
  margin-bottom: 8px;
}

.notif-actions {
  display: flex;
  justify-content: flex-end;
}

.btn-borrar-notif {
  background: #eb444a;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  font-weight: bold;
  font-size: 0.75rem;
  cursor: pointer;
}

.estado-box-pequeno {
  text-align: center;
  padding: 16px;
  background: var(--ion-card-background, #ffffff);
  border-radius: 8px;
  font-size: 0.85rem;
  color: var(--ion-color-step-600, #666);
  margin-bottom: 12px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.03);
}

.divisor-seccion {
  height: 1px;
  background: var(--ion-color-step-200, #ddd);
  margin: 15px 0 20px;
}

/* Tarjeta de Turno Moderna */
.turno-card {
  margin: 0 0 16px 0;
  border-radius: 16px;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.06);
  background: var(--ion-card-background, #ffffff);
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.03);
}

.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.fecha-turno {
  font-size: 0.82rem;
  color: var(--ion-color-step-600, #666);
  font-weight: 600;
}

.precio {
  font-size: 1rem;
  font-weight: 800;
  color: #3880ff;
  background: rgba(56, 128, 255, 0.1);
  padding: 4px 10px;
  border-radius: 8px;
}

.titulo-servicio {
  font-size: 1.35rem;
  font-weight: 800;
  margin: 0 0 10px 0;
  color: var(--ion-text-color, #000);
}

.detalles-info {
  font-size: 0.95rem;
  color: var(--ion-color-step-600, #555);
  margin-bottom: 14px;
  line-height: 1.6;
}

.detalles-info p {
  margin: 3px 0;
}

/* Estética Mejorada para el QR */
.qr-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin: 14px 0;
  padding: 12px;
  background: var(--ion-color-step-50, #fafafa);
  border-radius: 12px;
  border: 1px dashed var(--ion-color-step-200, #ddd);
}

.qr-label {
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--ion-color-step-600, #666);
  margin-bottom: 8px;
}

.qr-imagen {
  width: 130px;
  height: 130px;
  object-fit: contain;
  background: white;
  padding: 6px;
  border-radius: 8px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
  border: 1px solid #eaeaea;
}

/* Botones de Acción Estilizados */
.card-actions {
  display: flex;
  gap: 10px;
  margin-top: 14px;
}

.btn-pdf {
  flex: 1;
  background: linear-gradient(135deg, #3880ff, #3171e0);
  color: white;
  border: none;
  padding: 12px 14px;
  border-radius: 10px;
  font-weight: 800;
  font-size: 0.9rem;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(56, 128, 255, 0.3);
  transition: transform 0.2s ease;
}

.btn-pdf:active {
  transform: scale(0.98);
}

.btn-eliminar {
  background: rgba(235, 68, 74, 0.1);
  color: #eb444a;
  border: none;
  padding: 12px 16px;
  border-radius: 10px;
  font-weight: 800;
  font-size: 0.9rem;
  cursor: pointer;
  transition: background 0.2s ease;
}

.btn-eliminar:active {
  background: rgba(235, 68, 74, 0.2);
}

.estado-box {
  text-align: center;
  padding: 40px 20px;
  border-radius: 16px;
  background: var(--ion-card-background, #ffffff);
  margin-top: 20px;
  color: var(--ion-color-step-600, #666);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
}
</style>