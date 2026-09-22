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
        
        
        if (Array.isArray(res)) {
          this.productos = res;
          this.categorias = []; 
        } 
        
        else if (res && res.productos) {
          this.productos = res.productos;
          this.categorias = res.categorias || [];
        } else {
          this.productos = [];
        }
      } catch (err: any) {
        console.error("🔍 DETALLE DEL ERROR DE RED:", err);
        this.error = "No se pudo conectar con la API. Revisá que esté encendida y en la IP correcta.";
      } finally {
        this.cargando = false;
      }
    },

    // Acción mejorada para soportar FormData e imagen opcional
    async crear_producto(nuevoProducto: any, archivoFoto?: any) {
      try {
        let datosAEnviar = nuevoProducto;

        // Si hay una foto seleccionada, construimos un FormData
        if (archivoFoto) {
          const formData = new FormData();
          formData.append('categoriaId', nuevoProducto.categoriaId.toString());
          formData.append('nombre', nuevoProducto.nombre);
          if (nuevoProducto.descripcion) {
            formData.append('descripcion', nuevoProducto.descripcion);
          }
          formData.append('precio', nuevoProducto.precio.toString());
          formData.append('disponible', nuevoProducto.disponible ? 'true' : 'false');

          // Adjuntamos el archivo de imagen
          formData.append('imagen', {
            uri: archivoFoto.path || archivoFoto.webPath,
            name: 'producto_foto.jpg',
            type: 'image/jpeg'
          } as any);

          datosAEnviar = formData;
        }

        const creado = await api_crear_producto(datosAEnviar);
        this.productos.push(creado);
        return { success: true };
      } catch (err: any) {
        return { 
          success: false, 
          mensaje: err.mensaje || "Error al crear el producto en el servidor." 
        };
      }
    },

    async modificar_producto(id: number, productoActualizado: any, archivoFoto?: any) {
      try {
        let datosAEnviar = productoActualizado;

        if (archivoFoto) {
          const formData = new FormData();
          formData.append('categoriaId', productoActualizado.categoriaId.toString());
          formData.append('nombre', productoActualizado.nombre);
          if (productoActualizado.descripcion) {
            formData.append('descripcion', productoActualizado.descripcion);
          }
          formData.append('precio', productoActualizado.precio.toString());
          formData.append('disponible', productoActualizado.disponible ? 'true' : 'false');

          formData.append('imagen', {
            uri: archivoFoto.path || archivoFoto.webPath,
            name: 'producto_foto.jpg',
            type: 'image/jpeg'
          } as any);

          datosAEnviar = formData;
        }

        const modificado = await api_modificar_producto(id, datosAEnviar);
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