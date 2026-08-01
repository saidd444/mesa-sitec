<template>
  <div class="p-8">
    <!-- LOADING -->
    <div v-if="loading" class="text-center py-8">
      <p>Cargando solicitud...</p>
    </div>

    <!-- ERROR -->
    <div v-else-if="error" class="bg-red-100 text-red-700 p-4 rounded mb-4">
      {{ error }}
    </div>

    <!-- DATA -->
    <div v-else-if="solicitud" class="max-w-2xl">
      <router-link to="/solicitudes" class="text-blue-500 mb-4">← Volver</router-link>
      
      <h1 class="text-3xl font-bold mb-4">{{ solicitud.titulo }}</h1>
      
      <div class="bg-gray-50 p-6 rounded mb-6">
        <p class="mb-2"><strong>Código:</strong> <span data-testid="detalle-codigo">{{ solicitud.codigo }}</span></p>
        <p class="mb-2"><strong>Título:</strong> <span data-testid="detalle-titulo">{{ solicitud.titulo }}</span></p>
        <p class="mb-2"><strong>Descripción:</strong> <span data-testid="detalle-descripcion">{{ solicitud.descripcion }}</span></p>
        <p class="mb-2"><strong>Estado:</strong> <span data-testid="detalle-estado">{{ solicitud.estado }}</span></p>
        <p class="mb-2"><strong>Prioridad:</strong> <span data-testid="detalle-prioridad">{{ solicitud.prioridad }}</span></p>
        <p class="mb-2"><strong>Categoría:</strong> <span data-testid="detalle-categoria">{{ solicitud.categoria.nombre }}</span></p>
        <p class="mb-2"><strong>Agente:</strong> <span data-testid="detalle-agente">{{ solicitud.agente?.nombre || 'Sin asignar' }}</span></p>
        <p class="mb-2"><strong>Fecha creación:</strong> <span data-testid="detalle-fecha-creacion">{{ new Date(solicitud.fechaCreacion).toLocaleString() }}</span></p>
        <p class="mb-2"><strong>Fecha límite SLA:</strong> <span data-testid="detalle-fecha-limite">{{ new Date(solicitud.fechaLimiteSla).toLocaleString() }}</span></p>
        <p v-if="solicitud.vencida" class="mb-2"><strong>Estado:</strong> <span data-testid="detalle-vencida" class="text-red-600">Vencida</span></p>
      </div>

      <button
        @click="goToEditar"
        data-testid="btn-editar"
        class="bg-blue-500 text-white px-4 py-2 rounded mr-2"
      >
        Editar
      </button>
      <button
        @click="goToSolicitudes"
        class="bg-gray-500 text-white px-4 py-2 rounded"
      >
        Cerrar
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import type { Solicitud } from '../types/api'
import { apiClient } from '../services/apiClient'

const router = useRouter()
const route = useRoute()
const solicitud = ref<Solicitud | null>(null)
const loading = ref(true)
const error = ref('')

const loadSolicitud = async () => {
  loading.value = true
  error.value = ''
  try {
    const id = route.params.id as string
    const res = await apiClient.solicitudById(id)
    solicitud.value = res.data
  } catch (err) {
    error.value = 'Error al cargar la solicitud'
  } finally {
    loading.value = false
  }
}

const goToEditar = () => {
  router.push(`/solicitudes/${solicitud.value?.id}/editar`)
}

const goToSolicitudes = () => {
  router.push('/solicitudes')
}

onMounted(() => loadSolicitud())
</script>