<template>
  <comp-page titulo="Escanear Turno (Admin/Barbero)">
    <div class="scanner-container">
      <div class="info-box">
        <p>Utiliza la cámara para escanear el código QR del turno y validar o marcar la asistencia del cliente.</p>
      </div>

      <!-- Botón principal para iniciar el escáner -->
      <button class="btn-escanear" @click="iniciarEscaneo">
        📷 Abrir Escáner de QR
      </button>

      <!-- Resultado del escaneo previo si lo hubiera -->
      <div v-if="resultadoEscaneo" class="resultado-box">
        <h3>Resultado de la lectura:</h3>
        <p>{{ resultadoEscaneo }}</p>
        <button class="btn-validar" @click="procesarTurnoEscaneado">
          ✅ Validar / Atender Turno
        </button>
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { alertController } from '@ionic/vue';
import { BarcodeScanner, BarcodeFormat } from '@capacitor-mlkit/barcode-scanning';
import compPage from '../components/base/comp_page.vue';
import { obtener_api_url } from '@/config/debug';

const apiUrl = obtener_api_url() || 'http://localhost:5211';
const resultadoEscaneo = ref<string | null>(null);
const datosTurnoId = ref<any | null>(null);

const mostrarAlerta = async (titulo: string, mensaje: string) => {
  const alert = await alertController.create({
    header: titulo,
    message: mensaje,
    buttons: ['Aceptar']
  });
  await alert.present();
};

const iniciarEscaneo = async () => {
  try {
    const status = await BarcodeScanner.requestPermissions();
    if (status.camera !== 'granted') {
      mostrarAlerta('Permiso denegado', 'Se necesita acceso a la cámara para escanear códigos QR.');
      return;
    }

    document.querySelector('body')?.classList.add('barcode-scanner-active');

    const result = await BarcodeScanner.scan({
      formats: [BarcodeFormat.QrCode],
    });

    document.querySelector('body')?.classList.remove('barcode-scanner-active');

    if (result.barcodes.length > 0) {
      const codigoLeido = result.barcodes[0].displayValue;
      resultadoEscaneo.value = codigoLeido;
      datosTurnoId.value = codigoLeido; 
    }
  } catch (error) {
    document.querySelector('body')?.classList.remove('barcode-scanner-active');
    console.error('Error al escanear:', error);
    simularEscaneoWeb();
  }
};

const simularEscaneoWeb = async () => {
  const alert = await alertController.create({
    header: 'Entorno Web / Simulador',
    message: 'El escáner nativo requiere un dispositivo físico. ¿Deseas simular el escaneo de un ID de turno?',
    inputs: [
      {
        name: 'idTurno',
        type: 'text',
        placeholder: 'ID del Turno (ej. 15)'
      }
    ],
    buttons: [
      { text: 'Cancelar', role: 'cancel' },
      {
        text: 'Simular',
        handler: (data) => {
          if (data.idTurno) {
            resultadoEscaneo.value = `Turno ID: ${data.idTurno}`;
            datosTurnoId.value = data.idTurno;
          }
        }
      }
    ]
  });
  await alert.present();
};

const procesarTurnoEscaneado = async () => {
  try {
    // 1. Extraer limpiamente el número de ID si el QR contiene texto en formato multilínea
    let idNumerico = datosTurnoId.value;
    if (typeof idNumerico === 'string' && idNumerico.includes('Turno ID:')) {
      const match = idNumerico.match(/Turno ID:\s*([0-9]+)/i);
      if (match && match[1]) {
        idNumerico = match[1];
      }
    }

    // 2. Obtener el token guardado para autorizar la petición
    const token = localStorage.getItem('token') || localStorage.getItem('access_token') || localStorage.getItem('capacitor-storage_tudominio_token');
    const tokenLimpio = token ? token.replace(/^["']|["']$/g, '').trim() : '';

    const headers: Record<string, string> = {
      'Content-Type': 'application/json'
    };
    if (tokenLimpio) {
      headers['Authorization'] = `Bearer ${tokenLimpio}`;
    }

    const baseClean = apiUrl.replace(/\/+$/, '').replace(/\/api$/, '');
    const urlFinal = `${baseClean}/api/turnos/validar`;

    const response = await fetch(urlFinal, {
      method: 'POST',
      headers: headers,
      body: JSON.stringify({ qrData: idNumerico })
    });

    if (response.ok) {
      mostrarAlerta('Éxito', '¡Turno validado y atendido con éxito!');
      resultadoEscaneo.value = null;
    } else {
      const errData = await response.json().catch(() => ({}));
      mostrarAlerta('Error', errData.mensaje || 'No se pudo validar el código QR en el servidor.');
    }
  } catch (e) {
    console.error(e);
    mostrarAlerta('Atención', 'Ocurrió un error al intentar comunicarse con el servidor.');
  }
};
</script>

<style scoped>
.scanner-container {
  padding: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  height: 80vh;
}

.info-box {
  margin-bottom: 24px;
  font-size: 0.95rem;
  color: var(--ion-color-step-600, #666);
  line-height: 1.5;
}

.btn-escanear {
  background: #3880ff;
  color: white;
  border: none;
  padding: 16px 24px;
  border-radius: 12px;
  font-weight: bold;
  font-size: 1rem;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(56, 128, 255, 0.3);
  width: 100%;
  max-width: 300px;
}

.resultado-box {
  margin-top: 30px;
  padding: 16px;
  background: var(--ion-card-background, #fff);
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
  width: 100%;
  max-width: 300px;
}

.resultado-box h3 {
  font-size: 1rem;
  margin-bottom: 8px;
}

.resultado-box p {
  font-size: 0.9rem;
  color: #3880ff;
  font-weight: bold;
  margin-bottom: 12px;
  word-break: break-all;
}

.btn-validar {
  background: #2dd36f;
  color: white;
  border: none;
  padding: 10px;
  border-radius: 8px;
  font-weight: bold;
  font-size: 0.9rem;
  cursor: pointer;
  width: 100%;
}
</style>

<!-- Estilo global requerido por el plugin ML Kit para que la cámara sea visible -->
<style>
.barcode-scanner-active {
  visibility: hidden;
}
.barcode-scanner-active ion-router-outlet,
.barcode-scanner-active ion-content {
  background: transparent !important;
}
</style>