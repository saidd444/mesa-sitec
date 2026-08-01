<template>
  <div class="p-8">
    <h1 class="text-3xl font-bold mb-6">{{ isEditing ? 'Editar' : 'Nueva' }} Solicitud</h1>
    
    <div v-if="error" class="bg-red-100 text-red-700 p-4 rounded mb-4">
      {{ error }}
    </div>

    <form @submit.prevent="handleSubmit" class="max-w-2xl">
      <div class="mb-4">
        <label class="block font-bold mb-2">Título</label>
        <input
          v-model="form.titulo"
          type="text"
          data-testid="form-titulo"
          class="w-full p-2 border"
          required
          minlength="5"
          maxlength="120"
        />
        <div v-if="errors.titulo" data-testid="error-titulo" class="text-red-500 text-sm mt-1">{{ errors.titulo }}</div>
      </div>

      <div class="mb-4">
        <label class="block font-bold mb-2">Descripción</label>
        <textarea
          v-model="form.descripcion"
          data-testid="form-descripcion"
          class="w-full p-2 border"
          required
          minlength="10"
          maxlength="4000"
          rows="6"
        ></textarea>
        <div v-if="errors.descripcion" data-testid="error-descripcion" class="text-red-500 text-sm mt-1">{{ errors.descripcion }}</div>
      </div>

      <div class="mb-4">
        <label class="block font-bold mb-2">Categoría</label>
        <select v-model="form.categoriaId" data-testid="form-categoria" class="w-full p-2 border" required>
          <option value="">Seleccionar categoría</option>
          <option v-for="cat in categorias" :key="cat.id" :value="cat.id">
            {{ cat.nombre }}
          </option>
        </select>
        <div v-if="errors.categoria" data-testid="error-categoria" class="text-red-500 text-sm mt-1">{{ errors.categoria }}</div>
      </div>

      <div class="mb-4">
        <label class="block font-bold mb-2">Prioridad</label>
        <select v-model.number="form.prioridad" data-testid="form-prioridad" class="w-full p-2 border" required>
          <option :value="0">Baja</option>
          <option :value="1">Media</option>
          <option :value="2">Alta</option>
          <option :value="3">Crítica</option>
        </select>
      </div>

      <div class="flex gap-2">
        <button type="submit" :disabled="loading" data-testid="form-submit" class="bg-blue-500 text-white px-4 py-2 rounded">
          {{ loading ? 'Guardando...' : 'Guardar' }}
        </button>
        <button type="button" @click="goBack" data-testid="form-cancelar" class="bg-gray-500 text-white px-4 py-2 rounded">
          Cancelar
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import type { Categoria } from '../types/api'
import { apiClient } from '../services/apiClient'

const router = useRouter()
const route = useRoute()

const isEditing = ref(false)
const categorias = ref<Categoria[]>([])
const loading = ref(false)
const error = ref('')
const errors = ref({ titulo: '', descripcion: '', categoria: '' })

const form = ref({
  titulo: '',
  descripcion: '',
  categoriaId: '',
  prioridad: 1
})

const loadCategorias = async () => {
  try {
    categorias.value = [
      { id: '4267975c-685a-4558-ba78-ff1933567e64', nombre: 'Incidente' },
      { id: '4351d844-06cf-4685-9fd0-13677721275c', nombre: 'Consulta' },
      { id: '99346e7f-4510-4243-9065-e691c074ab4b', nombre: 'Falla crítica' },
      { id: 'e384d39b-3900-4484-8199-192bc4dd676f', nombre: 'Requerimiento' }
    ]
  } catch (err) {
    console.error(err)
  }
}

const loadSolicitudParaEditar = async () => {
  try {
    const id = route.params.id as string
    const res = await apiClient.solicitudById(id)
    const sol = res.data
    form.value = {
      titulo: sol.titulo,
      descripcion: sol.descripcion,
      categoriaId: sol.categoria.id,
      prioridad: ['Baja', 'Media', 'Alta', 'Critica'].indexOf(sol.prioridad)
    }
  } catch (err) {
    error.value = 'Error al cargar la solicitud'
  }
}

const handleSubmit = async () => {
  loading.value = true
  error.value = ''
  errors.value = { titulo: '', descripcion: '', categoria: '' }

  try {
    if (isEditing.value) {
      const id = route.params.id as string
      await apiClient.editarSolicitud(id, {
        titulo: form.value.titulo,
        descripcion: form.value.descripcion,
        categoriaId: form.value.categoriaId,
        prioridad: form.value.prioridad
      })
    } else {
      await apiClient.crearSolicitud({
        titulo: form.value.titulo,
        descripcion: form.value.descripcion,
        categoriaId: form.value.categoriaId,
        prioridad: form.value.prioridad
      })
    }
    router.push('/solicitudes')
  } catch (err) {
    error.value = 'Error al guardar'
  } finally {
    loading.value = false
  }
}

const goBack = () => {
  router.back()
}

onMounted(() => {
  loadCategorias()
  if (route.path.includes('editar')) {
    isEditing.value = true
    loadSolicitudParaEditar()
  }
})
</script>