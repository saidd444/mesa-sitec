import axios from 'axios'
import { useAuthStore } from '../stores/authStore'
import type { LoginResponse, Solicitud, ListadoSolicitudesResponse, Usuario } from '../types/api'

const API_BASE = 'http://localhost:5298/api/v1'

interface LoginRequest {
  email: string
  password: string
}

interface SolicitudFilters {
  estado?: string
  prioridad?: string
  categoriaId?: string
  agenteId?: string
  q?: string
  vencidas?: boolean
  page?: number
  pageSize?: number
  sort?: string
}

interface CrearSolicitudPayload {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: number
}

interface EditarSolicitudPayload {
  titulo?: string
  descripcion?: string
  categoriaId?: string
  prioridad?: number
}

interface TransicionarPayload {
  nuevoEstado: number
  motivo?: string
  agenteId?: string
}

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
  login: (req: LoginRequest) =>
    client.post<LoginResponse>('/auth/login', req),

  me: () =>
    client.get<Usuario>('/me'),

  solicitudes: (filters: SolicitudFilters) =>
    client.get<ListadoSolicitudesResponse>('/solicitudes', { params: filters }),

  solicitudById: (id: string) =>
    client.get<Solicitud>(`/solicitudes/${id}`),

  crearSolicitud: (payload: CrearSolicitudPayload) =>
    client.post<Solicitud>('/solicitudes', payload),

  editarSolicitud: (id: string, payload: EditarSolicitudPayload) =>
    client.put<Solicitud>(`/solicitudes/${id}`, payload),

  transicionarSolicitud: (id: string, payload: TransicionarPayload) =>
    client.post<Solicitud>(`/solicitudes/${id}/transiciones`, payload)
}