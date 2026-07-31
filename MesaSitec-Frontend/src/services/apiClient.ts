import axios from 'axios'
import { useAuthStore } from '../stores/authStore'
import type { LoginResponse, Solicitud, ListadoSolicitudesResponse } from '../types/api'

const API_BASE = 'http://localhost:5298/api/v1'

const client = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' }
})

// Interceptor: inyecta token en cada petición
client.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.token) {
    config.headers.Authorization = `Bearer ${auth.token}`
  }
  return config
})

// Interceptor: maneja 401 (token expirado)
client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const auth = useAuthStore()
      auth.logout()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export const apiClient = {
  login: (email: string, password: string) =>
    client.post<LoginResponse>('/auth/login', { email, password }),

  me: () =>
    client.get('/me'),

  solicitudes: (params: any) =>
    client.get<ListadoSolicitudesResponse>('/solicitudes', { params }),

  solicitudById: (id: string) =>
    client.get<Solicitud>(`/solicitudes/${id}`),

  crearSolicitud: (data: any) =>
    client.post<Solicitud>('/solicitudes', data),

  editarSolicitud: (id: string, data: any) =>
    client.put<Solicitud>(`/solicitudes/${id}`, data),

  transicionarSolicitud: (id: string, data: any) =>
    client.post(`/solicitudes/${id}/transiciones`, data)
}