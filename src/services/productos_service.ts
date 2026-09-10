import { ajax_get, ajax_post, ajax_put, ajax_delete } from "@/ajax_service";

export async function api_obtener_resumen_productos() {
    return await ajax_get("productos/resumen");
}

export async function api_crear_producto(producto: any) {
    return await ajax_post("productos", producto);
}

export async function api_modificar_producto(id: number, producto: any) {
    return await ajax_put(`productos/${id}`, producto);
}

export async function api_dar_de_baja_producto(id: number) {
    return await ajax_delete(`productos/${id}`);
}