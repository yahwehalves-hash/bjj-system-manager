import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const usuariosApi = {
  listar: () => api.get('/api/usuarios').then(r => r.data),
  alterarRole: (id, role) => api.patch(`/api/usuarios/${id}/role`, { role }),
}
