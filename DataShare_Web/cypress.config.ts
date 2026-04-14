import { defineConfig } from "cypress";

export default defineConfig({
  projectId: 'i5z5rz',
  allowCypressEnv: true,
  
  e2e: {
    setupNodeEvents(on, config) {
            
    },
    baseUrl: 'http://localhost:4200',    
  },
});