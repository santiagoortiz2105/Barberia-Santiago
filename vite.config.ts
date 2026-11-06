/// <reference types="vitest" />

import legacy from '@vitejs/plugin-legacy'
import vue from '@vitejs/plugin-vue'
import path from 'path'
import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    legacy()
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      // Solución definitiva para el error de devtools en producción/móvil
      '@vue/devtools-api': path.resolve(__dirname, './src/config/empty-devtools.ts'),
    },
  },
  build: {
    rollupOptions: {
      external: []
    }
  },
  test: {
    globals: true,
    environment: 'jsdom'
  }
})