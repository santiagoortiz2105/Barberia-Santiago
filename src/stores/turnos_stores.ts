import { defineStore } from 'pinia';
import { obtener_api_url } from '../config/debug';

export const useTurnosStore = defineStore('turnos', {
  state: () => ({
    turnos: [] as any[],
    cargando: false
  }),
  actions: {
    async cargarTurnos() {
      this.cargando = true;
      try {
        const apiUrl = obtener_api_url();
        const baseClean = (apiUrl || 'http://localhost:5211').replace(/\/+$/, '').replace(/\/api$/, '');

        // 🔑 Búsqueda robusta del token en todas las posibles claves locales del celular
        const tokenGuardado = 
          localStorage.getItem('tudominio_token') || 
          localStorage.getItem('token') || 
          localStorage.getItem('access_token');
          
        const tokenLimpio = tokenGuardado ? tokenGuardado.replace(/^["']|["']$/g, '').trim() : '';

        const response = await fetch(`${baseClean}/api/turnos`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${tokenLimpio}`
          }
        });

        if (!response.ok) throw new Error('Error al obtener los turnos del servidor');

        const data = await response.json();
        this.turnos = data || [];
        
        // Guardamos copia local si hubo red
        localStorage.setItem('copia_turnos', JSON.stringify(data));
      } catch (error: any) {
        // Si falla la red, recurrimos a la copia local almacenada
        const copia = localStorage.getItem('copia_turnos');
        if (copia) {
          this.turnos = JSON.parse(copia);
        } else {
          console.error("Error cargando turnos:", error);
          this.turnos = [];
        }
      } finally {
        this.cargando = false;
      }
    }
  }
});