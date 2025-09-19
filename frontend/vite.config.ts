import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5247',
        changeOrigin: true,
        secure: false,
      },
      '/swagger': {
        target: 'http://localhost:5247',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/swagger/, '/swagger'),
      },
    },
  },
})
