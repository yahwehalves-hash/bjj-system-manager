import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

export const authApi = {
  login: (email, senha) =>
    api.post('/api/auth/login', { email, senha }).then((r) => r.data),

  registrar: (nome, email, senha, role) =>
    api.post('/api/auth/registrar', { nome, email, senha, role }).then((r) => r.data),
};
