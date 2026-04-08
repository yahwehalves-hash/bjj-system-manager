import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const configuracoesApi = {
  obterGlobal:        ()             => api.get('/api/configuracoes/global'),
  atualizarGlobal:    (dados)        => api.put('/api/configuracoes/global', dados),
  obterEfetiva:       (filialId)     => api.get(`/api/configuracoes/filial/${filialId}/efetiva`),
  atualizarFilial:    (filialId, dados) => api.put(`/api/configuracoes/filial/${filialId}`, dados),
}
