<template>
  <div class="p-8">
    <h1 class="text-3xl font-bold mb-6">Solicitudes</h1>
    
    <!-- LOADING STATE -->
    <div v-if="loading" class="text-center py-8">
      <p>Cargando solicitudes...</p>
    </div>
    
    <!-- EMPTY STATE -->
    <div v-else-if="solicitudes.length === 0" class="text-center py-8">
      <p class="text-gray-500">No hay solicitudes</p>
    </div>
    
    <!-- DATA STATE -->
    <div v-else class="overflow-x-auto">
      <table class="w-full border-collapse border">
        <thead>
          <tr class="bg-gray-200">
            <th class="border p-2">Código</th>
            <th class="border p-2">Título</th>
            <th class="border p-2">Estado</th>
            <th class="border p-2">Prioridad</th>
            <th class="border p-2">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="sol in solicitudes" :key="sol.id" class="border">
            <td class="border p-2">{{ sol.codigo }}</td>
            <td class="border p-2">{{ sol.titulo }}</td>
            <td class="border p-2">{{ sol.estado }}</td>
            <td class="border p-2">{{ sol.prioridad }}</td>
            <td class="border p-2">
              <button @click="verDetalle(sol.id)" class="bg-blue-500 text-white px-2 py-1 rounded">
                Ver
              </button>
            </td>
          </tr>
        </tbody>
      </table>
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