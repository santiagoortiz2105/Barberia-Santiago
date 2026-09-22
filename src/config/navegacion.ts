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
  icono: string;
  grupo: 'principal' | 'configuracion';
  orden: number;
  componente: any;
  menu_roles?: string[]; // <-- Añadido para controlar el acceso por rol
}

export const navegacion: ItemNavegacion[] = [
  { 
    id: 'inicio', 
    titulo: 'Inicio', 
    ruta: '/app/inicio', 
    icono: homeOutline, 
    grupo: 'principal', 
    orden: 10, 
    componente: InicioView,
    menu_roles: ['TODOS'] 
  },
  { 
    id: 'servicios', 
    titulo: 'Servicios', 
    ruta: '/app/servicios', 
    icono: cutOutline, 
    grupo: 'principal', 
    orden: 20, 
    componente: ServiciosView,
    menu_roles: ['TODOS'] // Todos pueden ver los servicios
  },
  { 
    id: 'barberos', 
    titulo: 'Barberos', 
    ruta: '/app/barberos', 
    icono: peopleOutline, 
    grupo: 'principal', 
    orden: 30, 
    componente: BarberosView,
    menu_roles: ['administrador', 'barbero'] // Solo admin y barbero ven la lista de colegas/personal
  },
  { 
    id: 'turnos', 
    titulo: 'Turnos', 
    ruta: '/app/turnos', 
    icono: calendarOutline, 
    grupo: 'principal', 
    orden: 40, 
    componente: TurnosView,
    menu_roles: ['TODOS'] // Permitido para todos; dentro de la vista se gestiona si ve los generales o sus propios turnos
  },
  { 
    id: 'micuenta', 
    titulo: 'Mi Cuenta', 
    ruta: '/app/micuenta', 
    icono: personOutline, 
    grupo: 'configuracion', 
    orden: 50, 
    componente: MiCuentaView,
    menu_roles: ['TODOS'] // Todos pueden ver su propia cuenta
  }
];