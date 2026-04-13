import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export const authApi = {
  login: (email, senha) =>
    api.post('/api/auth/login', { email, senha }).then((r) => r.data),

  registrar: (nome, email, senha) =>
    api.post('/api/auth/registrar', { nome, email, senha }).then((r) => r.data),

  alterarSenha: (senhaAtual, novaSenha) =>
    api.post('/api/auth/alterar-senha', { senhaAtual, novaSenha }).then((r) => r.data),
};

export const usuariosApi = {
  listar: () =>
    api.get('/api/usuarios').then((r) => r.data),

  alterarRole: (id, role) =>
    api.patch(`/api/usuarios/${id}/role`, { role }).then((r) => r.data),
};
