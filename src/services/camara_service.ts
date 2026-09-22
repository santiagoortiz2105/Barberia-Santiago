import { Camera, CameraResultType, CameraSource } from '@capacitor/camera';

export async function tomar_foto(): Promise<string | null> {
  try {
    const photo = await Camera.getPhoto({
      quality: 90,
      allowEditing: false,
      resultType: CameraResultType.Base64,
      source: CameraSource.Camera
    });

    return photo.base64String ? `data:image/jpeg;base64,${photo.base64String}` : null;
  } catch (error) {
    console.error("Error al tomar la foto:", error);
    return null;
  }
}

export async function seleccionar_de_galeria(): Promise<string | null> {
  try {
    const photo = await Camera.getPhoto({
      quality: 90,
      allowEditing: false,
      resultType: CameraResultType.Base64,
      source: CameraSource.Photos
    });

    return photo.base64String ? `data:image/jpeg;base64,${photo.base64String}` : null;
  } catch (error) {
    console.error("Error al seleccionar de la galería:", error);
    return null;
  }
}