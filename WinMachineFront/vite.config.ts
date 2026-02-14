import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    sourcemap: true, // Use standard source maps instead of eval
  },
  server: {
    hmr: {
      overlay: false, // Optionally disable the error overlay if it's causing issues, though usually helpful
    }
  }
})
