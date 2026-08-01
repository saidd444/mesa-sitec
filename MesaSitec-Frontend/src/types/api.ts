export interface Categoria {
  id: string
  nombre: string
}

export interface AgenteResumen {
  id: string
  nombre: string
}

export interface Usuario {
  id: string
  email: string
  nombre: string
  rol: 'Admin' | 'Agente' | 'Solicitante'
  tenantId: string
  tenantNombre: string
}

export interface LoginResponse {
  accessToken: string
  expiraEn: number
  usuario: Usuario
}

export interface Solicitud {
  id: string
  codigo: string
  titulo: string
  descripcion: string
  estado: 'Nueva' | 'Asignada' | 'EnProceso' | 'Resuelta' | 'Cerrada' | 'Cancelada'
  prioridad: 'Baja' | 'Media' | 'Alta' | 'Critica'
  categoria: Categoria
  agente: AgenteResumen | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface ListadoSolicitudesResponse {
  items: Solicitud[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}