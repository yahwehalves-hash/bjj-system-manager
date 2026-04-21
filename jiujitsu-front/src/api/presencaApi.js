import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const presencaApi = {
  registrar:       (data) => api.post('/api/presenca', data),
  registrarEmLote: (data) => api.post('/api/presenca/lote', data),

  listarPorTurma:   (turmaId, dataInicio, dataFim) =>
    api.get(`/api/presenca/turma/${turmaId}`, { params: { dataInicio, dataFim } }),

  frequenciaTurma:  (turmaId, dataInicio, dataFim) =>
    api.get(`/api/presenca/turma/${turmaId}/frequencia`, { params: { dataInicio, dataFim } }),

  frequenciaAtleta: (atletaId, dataInicio, dataFim) =>
    api.get(`/api/presenca/atleta/${atletaId}/frequencia`, { params: { dataInicio, dataFim } }),
}
