import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const notificacoesApi = {
  listarTemplates:  ()          => api.get('/api/notificacoes/templates'),
  criarTemplate:    (data)      => api.post('/api/notificacoes/templates', data),
  atualizarTemplate:(id, data)  => api.put(`/api/notificacoes/templates/${id}`, data),
  removerTemplate:  (id)        => api.delete(`/api/notificacoes/templates/${id}`),

  // WhatsApp
  whatsappConfig:  ()     => api.get('/api/notificacoes/whatsapp/config'),
  whatsappStatus:  ()     => api.get('/api/notificacoes/whatsapp/status'),
  whatsappConectar: ()    => api.post('/api/notificacoes/whatsapp/conectar'),
  whatsappTestar:  (data) => api.post('/api/notificacoes/whatsapp/testar', data),
}
