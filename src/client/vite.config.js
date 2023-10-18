import { defineConfig } from 'vite';
import http from 'https';

export default defineConfig({
    plugins: [],
  
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