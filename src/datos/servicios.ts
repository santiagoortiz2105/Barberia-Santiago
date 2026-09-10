export interface Servicio {
  id: number;
  nombre: string;
  precio: string;
  descripcion: string;
}

const SERVICIOS: Servicio[] = [
  { id: 1, nombre: 'Corte Tradicional', precio: '$10.000', descripcion: 'Corte de cabello clásico o moderno.' },
  { id: 2, nombre: 'Barba Completa', precio: '$3.500', descripcion: 'Rebaje, emprolijado y delineado de barba.' },
  { id: 3, nombre: 'Perfilado y Cejas', precio: '$2.000', descripcion: 'Perfilado preciso de contornos a navaja y diseño de cejas.' }
];

export function obtener_servicios(): Promise<{ servicios: Servicio[] }> {
  return new Promise(resolve => setTimeout(() => resolve({ servicios: SERVICIOS }), 300));
}