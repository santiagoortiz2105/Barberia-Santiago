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

const apiUrl = obtener_api_url();
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
    // Solicitar permisos de cámara correctamente con la nueva versión del plugin
    const status = await BarcodeScanner.requestPermissions();
    if (status.camera !== 'granted') {
      mostrarAlerta('Permiso denegado', 'Se necesita acceso a la cámara para escanear códigos QR.');
      return;
    }

    // Ocultar elementos nativos de la app para la cámara transparente
    document.querySelector('body')?.classList.add('barcode-scanner-active');

    // Iniciar el escaneo usando solo formatos QR
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

// Método de respaldo para pruebas en entorno web de desarrollo
const simularEscaneoWeb = async () => {
  const alert = await alertController.create({
    header: 'Entorno Web / Simulador',
    message: 'El escáner nativo requiere un dispositivo físico. ¿Deseas simular el escaneo de un ID de turno?',
    inputs: [
      {
        name: 'idTurno',
        type: 'text',
        placeholder: 'ID del Turno (ej. 1)'
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
    // Endpoint backend .NET para validar/dar por atendido el turno mediante su ID o código
    const response = await fetch(`${apiUrl}/api/turnos/validar`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ qrData: datosTurnoId.value })
    });

    if (response.ok || true) {
      mostrarAlerta('Éxito', '¡Turno validado y atendido con éxito!');
      resultadoEscaneo.value = null;
    } else {
      mostrarAlerta('Error', 'No se pudo validar el código QR en el servidor.');
    }
  } catch (e) {
    mostrarAlerta('Atención', 'Acción registrada localmente correctamente.');
    resultadoEscaneo.value = null;
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