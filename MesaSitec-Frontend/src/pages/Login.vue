<template>
  <div class="flex items-center justify-center min-h-screen bg-gray-100">
    <div class="bg-white p-8 rounded shadow-md w-96">
      <h1 class="text-2xl font-bold mb-6">MesaSitec Login</h1>
      
      <form @submit.prevent="handleLogin">
        <input
          v-model="email"
          type="email"
          placeholder="Email"
          class="w-full p-2 border mb-4"
        />
        <input
          v-model="password"
          type="password"
          placeholder="Contraseña"
          class="w-full p-2 border mb-4"
        />
        
        <div v-if="error" class="text-red-500 mb-4">{{ error }}</div>
        
        <button
          type="submit"
          :disabled="loading"
          class="w-full bg-blue-500 text-white p-2 rounded"
        >
          {{ loading ? 'Cargando...' : 'Entrar' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { apiClient } from '../services/apiClient'

const router = useRouter()
const auth = useAuthStore()

const email = ref('admin@norte.test')
const password = ref('Sitec.2026')
const loading = ref(false)
const error = ref('')

const handleLogin = async () => {
  loading.value = true
  error.value = ''
  
  try {
    const res = await apiClient.login(email.value, password.value)
    auth.setToken(res.data.accessToken, res.data.usuario)
    router.push('/solicitudes')
  } catch (err) {
    error.value = 'Credenciales inválidas'
  } finally {
    loading.value = false
  }
}
</script>