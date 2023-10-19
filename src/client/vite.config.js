import { defineConfig } from 'vite';
import basicSsl from '@vitejs/plugin-basic-ssl'
import http from 'https';

export default defineConfig({
    plugins: [basicSsl()],
  
    build: {
         outDir: "../../dist"
    },

    server: {
        proxy: {
            '/IApi': {
                target: 'https://localhost:5001',
                changeOrigin: true,
                secure: false,
                agent: new http.Agent()
            }
        }
    }
})