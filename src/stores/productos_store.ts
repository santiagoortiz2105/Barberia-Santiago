import { defineStore } from "pinia";
import { 
  api_obtener_resumen_productos, 
  api_crear_producto, 
  api_modificar_producto, 
  api_dar_de_baja_producto 
} from "@/services/productos_service";

interface Producto {
  id: number;
  categoriaId: number;
  nombre: string;
  descripcion?: string;
  precio: number;
  imagenUrl?: string;
  disponible: boolean;
}

interface Categoria {
  id: number;
  nombre: string;
}

interface ProductosState {
  productos: Producto[];
  categorias: Categoria[];
  cargando: boolean;
  error: string | null;
}

export const useProductosStore = defineStore("productos", {
  state: (): ProductosState => ({
    productos: [],
    categorias: [],
    cargando: false,
    error: null,
  }),
  getters: {
    hay_productos: (state: ProductosState) => state.productos.length > 0,
  },
  actions: {
    async cargar_productos() {
      this.cargando = true;
      this.error = null;
      try {
        const res = await api_obtener_resumen_productos();
        if (res && res.productos) {
          this.productos = res.productos;
          this.categorias = res.categorias || [];
        }
      } catch (err: any) {
        this.error = "No se pudo conectar con la API. Revisá que esté encendida y en la IP correcta.";
      } finally {
        this.cargando = false;
      }
    },

    async crear_producto(nuevoProducto: any) {
      try {
        const creado = await api_crear_producto(nuevoProducto);
        this.productos.push(creado);
        return { success: true };
      } catch (err: any) {
        return { 
          success: false, 
          mensaje: err.mensaje || "Error al crear el producto en el servidor." 
        };
      }
    },

    async modificar_producto(id: number, productoActualizado: any) {
      try {
        const modificado = await api_modificar_producto(id, productoActualizado);
        const index = this.productos.findIndex(p => p.id === id);
        if (index !== -1) {
          this.productos[index] = modificado;
        }
        return { success: true };
      } catch (err: any) {
        return { 
          success: false, 
          mensaje: err.mensaje || "Error al modificar el producto." 
        };
      }
    },

    async dar_de_baja_producto(id: number) {
      try {
        await api_dar_de_baja_producto(id);
        this.productos = this.productos.filter(p => p.id !== id);
        return { success: true };
      } catch (err: any) {
        return { 
          success: false, 
          mensaje: err.mensaje || "Error al dar de baja el producto." 
        };
      }
    }
  }
});