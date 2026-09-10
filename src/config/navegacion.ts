import InicioView from '../views/InicioView.vue';
import ServiciosView from '../views/ServiciosView.vue';
import BarberosView from '../views/BarberosView.vue';
import TurnosView from '../views/TurnosView.vue';
import MiCuentaView from '../views/MiCuentaView.vue';
import { homeOutline, cutOutline, peopleOutline, calendarOutline, personOutline } from 'ionicons/icons';

export interface ItemNavegacion {
  id: string;
  titulo: string;
  ruta: string;
  icono: String;
  grupo: 'principal' | 'configuracion';
  orden: number;
  componente: any;
}

export const navegacion: ItemNavegacion[] = [
  { id: 'inicio', titulo: 'Inicio', ruta: '/app/inicio', icono: homeOutline, grupo: 'principal', orden: 10, componente: InicioView },
  { id: 'servicios', titulo: 'Servicios', ruta: '/app/servicios', icono: cutOutline, grupo: 'principal', orden: 20, componente: ServiciosView },
  { id: 'barberos', titulo: 'Barberos', ruta: '/app/barberos', icono: peopleOutline, grupo: 'principal', orden: 30, componente: BarberosView },
  { id: 'turnos', titulo: 'Turnos', ruta: '/app/turnos', icono: calendarOutline, grupo: 'principal', orden: 40, componente: TurnosView },
  { id: 'micuenta', titulo: 'Mi Cuenta', ruta: '/app/micuenta', icono: personOutline, grupo: 'configuracion', orden: 50, componente: MiCuentaView }
];