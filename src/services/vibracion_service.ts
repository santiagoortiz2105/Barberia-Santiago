import { Haptics, NotificationType } from '@capacitor/haptics';
import { useTemaStore } from '@/stores/tema_store'; 

export async function vibrar_error() {
  const temaStore = useTemaStore();
  

  if (temaStore.vibracionApagada) return;

  try {
    await Haptics.notification({ type: NotificationType.Error });
  } catch {

  }
}