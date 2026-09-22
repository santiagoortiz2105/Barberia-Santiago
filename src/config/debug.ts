export function obtener_api_url(): string {
    return import.meta.env.VITE_API_URL_DEBUG || "http://localhost:5211";
}