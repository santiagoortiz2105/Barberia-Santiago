<template>
  <comp-page titulo="Gestión de Turnos">
    <!-- Refresco de pantalla -->
    <ion-refresher slot="fixed" @ionRefresh="recargarTurnos($event)">
      <ion-refresher-content
        pulling-text="Desliza hacia abajo para actualizar..."
        refreshing-spinner="circles"
        refreshing-text="Cargando turnos...">
      </ion-refresher-content>
    </ion-refresher>

    <div class="cards-container">
      <h2 class="subtitulo-seccion">
        {{ tituloSeccion }}
      </h2>

      <!-- Estado Cargando -->
      <div v-if="cargando" class="estado-box">
        <p>Cargando turnos...</p>
      </div>

      <!-- Estado Vacío -->
      <div v-else-if="turnos.length === 0" class="estado-box">
        <p>No hay turnos registrados en este momento.</p>
      </div>

      <!-- Lista de Turnos -->
      <template v-else>
        <ion-card v-for="turno in turnos" :key="turno.id" class="turno-card">
          <ion-card-content>
            <div class="card-header-row">
              <span class="fecha-turno">📅 {{ formatearFecha(turno.fecha) }}</span>
              <span class="precio">${{ turno.precioServicio || '0' }}</span>
            </div>

            <h2 class="titulo-servicio">{{ turno.nombreServicio || 'Corte de Cabello' }}</h2>
            
            <div class="detalles-info">
              <p><strong>Cliente:</strong> {{ turno.clienteNombre || 'Usuario' }}</p>
              <p><strong>Barbero:</strong> {{ turno.barberoNombre || 'Por asignar' }}</p>
            </div>

            <!-- SECCIÓN CÓDIGO QR -->
            <div class="qr-container" v-if="turno.id">
              <p class="qr-label">Código QR del Turno:</p>
              <img 
                :src="`${apiUrl}/turnos/${turno.id}/qr`" 
                alt="QR del Turno" 
                class="qr-imagen"
                @error="ocultarQrError"
              />
            </div>

            <!-- Acciones -->
            <div class="card-actions">
              <!-- Botón Descargar/Compartir PDF -->
              <button class="btn-pdf" @click="descargarYCompartirPdf(turno.id)">
                📄 Comprobante PDF
              </button>

              <!-- Botón Dar de Baja (EXCLUSIVO PARA EL ADMINISTRADOR) -->
              <button 
                v-if="esAdmin" 
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
import compPage from '../components/base/comp_page.vue';
import { use_sesion_store } from '@/stores/sesion_store';
import { obtener_api_url } from '@/config/debug';

const sesion = use_sesion_store();
const turnos = ref<any[]>([]);
const cargando = ref(false);
const apiUrl = obtener_api_url();

// Propiedad computada para verificar si es administrador
const esAdmin = computed(() => sesion.rol_activo === 'administrador');

const tituloSeccion = computed(() => {
  if (sesion.rol_activo === 'administrador') return 'Todos los Turnos (Admin)';
  if (sesion.rol_activo === 'barbero') return 'Turnos Asignados (Barbero)';
  return 'Mis Turnos Reservados';
});

onMounted(async () => {
  await cargarTurnos();
});

const cargarTurnos = async () => {
  cargando.value = true;
  try {
    const endpoint = `${apiUrl}/turnos`; 
    
    const response = await fetch(endpoint, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${sesion.token}`,
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      const data = await response.json();
      turnos.value = data.map((t: any) => ({
        id: t.id,
        nombreServicio: t.producto || 'Servicio general',
        precioServicio: t.precio || 0,
        fecha: t.fechaHora,
        estado: t.estado,
        usuarioId: t.usuarioId,
        clienteNombre: `Cliente #${t.usuarioId}`,
        barberoNombre: 'Por asignar'
      }));
    } else {
      console.error('Error al obtener los turnos de la API. Código:', response.status);
      turnos.value = [];
    }
  } catch (error) {
    console.error('Error de red al conectar con la API de turnos:', error);
    turnos.value = [];
  } finally {
    cargando.value = false;
  }
};

const recargarTurnos = async (event: RefresherCustomEvent) => {
  await cargarTurnos();
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
            const res = await fetch(`${apiUrl}/turnos/${id}`, { 
              method: 'DELETE',
              headers: {
                'Authorization': `Bearer ${sesion.token}`
              }
            });
            if (res.ok) {
              turnos.value = turnos.value.filter(t => t.id !== id);
              mostrarAlerta('Éxito', 'Turno dado de baja correctamente.');
            } else {
              mostrarAlerta('Error', 'No se pudo eliminar el turno.');
            }
          } catch (e) {
            mostrarAlerta('Error', 'Ocurrió un error de conexión.');
          }
        }
      }
    ]
  });
  await alert.present();
};

const descargarYCompartirPdf = async (idTurno: number) => {
  try {
    const urlPdf = `${apiUrl}/comprobantes/turno/${idTurno}/pdf`;
    
    const response = await fetch(urlPdf, {
      headers: {
        'Authorization': `Bearer ${sesion.token}`
      }
    });
    
    if (!response.ok) throw new Error('No se pudo obtener el comprobante.');
    
    const blob = await response.blob();
    const fileName = `comprobante_turno_${idTurno}.pdf`;

    // Detectar si estamos en un entorno nativo (Android / iOS) o en Navegador Web
    const esNativo = Capacitor.isNativePlatform();

    if (esNativo) {
      // Flujo nativo con Capacitor (Filesystem + Share)
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
      // Flujo para Navegador Web (Descarga directa limpia)
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

const ocultarQrError = (event: Event) => {
  const target = event.target as HTMLElement;
  if (target) target.style.display = 'none';
};

const formatearFecha = (fechaIso: string) => {
  try {
    return new Date(fechaIso).toLocaleString();
  } catch {
    return fechaIso;
  }
};
</script>

<style scoped>
.cards-container {
  padding: 12px;
}

.subtitulo-seccion {
  font-size: 1.1rem;
  font-weight: 700;
  margin-bottom: 12px;
  color: var(--ion-text-color, #000);
}

.turno-card {
  margin: 0 0 16px 0;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
  background: var(--ion-card-background, #ffffff);
}

.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
}

.fecha-turno {
  font-size: 0.8rem;
  color: var(--ion-color-step-600, #666);
}

.precio {
  font-size: 0.9rem;
  font-weight: 700;
  color: #3880ff;
}

.titulo-servicio {
  font-size: 1.3rem;
  font-weight: 800;
  margin: 0 0 8px 0;
  color: var(--ion-text-color, #000);
}

.detalles-info {
  font-size: 0.9rem;
  color: var(--ion-color-step-600, #666);
  margin-bottom: 12px;
  line-height: 1.4;
}

.qr-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin: 12px 0;
  padding: 8px;
  background: var(--ion-color-step-50, #f9f9f9);
  border-radius: 8px;
}

.qr-label {
  font-size: 0.75rem;
  color: var(--ion-color-step-600, #666);
  margin-bottom: 6px;
}

.qr-imagen {
  width: 120px;
  height: 120px;
  object-fit: contain;
  background: white;
  padding: 4px;
  border-radius: 6px;
  border: 1px solid #ddd;
}

.card-actions {
  display: flex;
  gap: 8px;
  margin-top: 10px;
}

.btn-pdf {
  flex: 1;
  background: #3880ff;
  color: white;
  border: none;
  padding: 10px;
  border-radius: 8px;
  font-weight: bold;
  font-size: 0.85rem;
  cursor: pointer;
}

.btn-eliminar {
  background: #eb444a;
  color: white;
  border: none;
  padding: 10px 14px;
  border-radius: 8px;
  font-weight: bold;
  font-size: 0.85rem;
  cursor: pointer;
}

.estado-box {
  text-align: center;
  padding: 40px 20px;
  border-radius: 12px;
  background: var(--ion-card-background, #ffffff);
  margin-top: 20px;
}
</style>