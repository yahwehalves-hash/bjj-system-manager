import axios from 'axios';

// URL base da API — configurada via variável de ambiente
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

// Injeta o JWT em todas as requisições automaticamente
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export const atletasApi = {
  // Listagem com filtros e paginação
  listar: (params) =>
    api.get('/api/atletas', { params }).then((r) => r.data),

  // Busca por ID
  obterPorId: (id) =>
    api.get(`/api/atletas/${id}`).then((r) => r.data),

  // Criação — retorna 202 Accepted com o ID gerado
  criar: (dados) =>
    api.post('/api/atletas', dados).then((r) => r.data),

  // Atualização — retorna 202 Accepted
  atualizar: (id, dados) =>
    api.put(`/api/atletas/${id}`, dados).then((r) => r.data),

  // Exclusão (soft delete) — retorna 202 Accepted
  excluir: (id) =>
    api.delete(`/api/atletas/${id}`).then((r) => r.data),
};
