import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useTemaStore = defineStore('tema', () => {
  const modoOscuro = ref(false);
  const vibracionApagada = ref(false);

  function toggleModoOscuro() {
    modoOscuro.value = !modoOscuro.value;
  }

  function toggleVibracion() {
    vibracionApagada.value = !vibracionApagada.value;
  }

  return {
    modoOscuro,
    vibracionApagada,
    toggleModoOscuro,
    toggleVibracion,
  };
});