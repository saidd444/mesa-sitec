<template>
  <div class="p-8">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold">Solicitudes</h1>
      <router-link to="/solicitudes/nueva">
        <button data-testid="btn-nueva-solicitud" class="bg-green-500 text-white px-4 py-2 rounded">
          + Nueva
        </button>
      </router-link>
    </div>
    
    <!-- LOADING STATE -->
    <div v-if="loading" data-testid="listado-cargando" class="text-center py-8">
      <p>Cargando solicitudes...</p>
    </div>
    
    <!-- EMPTY STATE -->
    <div v-else-if="solicitudes.length === 0" data-testid="listado-vacio" class="text-center py-8">
      <p class="text-gray-500">No hay solicitudes</p>
    </div>
    
    <!-- DATA STATE -->
    <div v-else class="overflow-x-auto">
      <table data-testid="tabla-solicitudes" class="w-full border-collapse border">
        <thead>
          <tr class="bg-gray-200">
            <th class="border p-2">Código</th>
            <th class="border p-2">Título</th>
            <th class="border p-2">Estado</th>
            <th class="border p-2">Prioridad</th>
            <th class="border p-2">SLA</th>
            <th class="border p-2">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="sol in solicitudes" :key="sol.id" data-testid="fila-solicitud" :data-codigo="sol.codigo" class="border hover:bg-gray-50">
            <td class="border p-2" data-testid="celda-codigo">{{ sol.codigo }}</td>
            <td class="border p-2">{{ sol.titulo }}</td>
            <td class="border p-2" data-testid="celda-estado">{{ sol.estado }}</td>
            <td class="border p-2" data-testid="celda-prioridad">{{ sol.prioridad }}</td>
            <td class="border p-2" data-testid="celda-sla">
              {{ new Date(sol.fechaLimiteSla).toLocaleDateString() }}
              <span v-if="sol.vencida" data-testid="badge-vencida" class="ml-2 bg-red-500 text-white px-2 py-1 rounded text-xs">
                Vencida
              </span>
            </td>
            <td class="border p-2">
              <button @click="verDetalle(sol.id)" class="bg-blue-500 text-white px-2 py-1 rounded">
                Ver
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      
      <div data-testid="paginacion-info" class="mt-4 text-center text-gray-600">
        Página 1 de 1 — {{ solicitudes.length }} resultados
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import type { Solicitud } from '../types/api'
import { apiClient } from '../services/apiClient'

const router = useRouter()
const solicitudes = ref<Solicitud[]>([])
const loading = ref(true)

const loadSolicitudes = async () => {
  loading.value = true
  try {
    const res = await apiClient.solicitudes({})
    solicitudes.value = res.data.items
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

const verDetalle = (id: string) => {
  router.push(`/solicitudes/${id}`)
}

onMounted(() => loadSolicitudes())
</script>