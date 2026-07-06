import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://unmultipliable-kelsey-unloyal.ngrok-free.dev',
        changeOrigin: true,
        secure: false,
        bypass: function (req, res, options) {
          if (req.method === 'GET' && req.url.includes('/api/authentication/confirm-email') && req.headers['accept']?.includes('text/html')) {
            return '/index.html';
          }
        }
      }
    }
  }
})
