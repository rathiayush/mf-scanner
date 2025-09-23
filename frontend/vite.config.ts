import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    host: '0.0.0.0', // Listen on all network interfaces
    strictPort: true,
    open: false,
    proxy: {
      '/api': {
        target: 'http://0.0.0.0:5247',
        changeOrigin: true,
        secure: false,
      },
      '/swagger': {
        target: 'http://0.0.0.0:5247',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/swagger/, '/swagger'),
      },
    },
  },
})
