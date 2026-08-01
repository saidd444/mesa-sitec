<template>
  <nav data-testid="app-nav" class="bg-blue-600 text-white p-4 flex justify-between items-center">
    <h1 class="text-xl font-bold">MesaSitec</h1>
    
    <div class="flex items-center gap-4">
      <div>
        <span data-testid="nav-usuario-nombre">{{ auth.usuario?.nombre }}</span>
        <span class="ml-2" data-testid="nav-usuario-rol">({{ auth.usuario?.rol }})</span>
      </div>
      <button
        @click="logout"
        data-testid="btn-logout"
        class="bg-red-500 px-3 py-1 rounded hover:bg-red-600"
      >
        Logout
      </button>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { useAuthStore } from '../stores/authStore'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

const logout = () => {
  auth.logout()
  router.push('/login')
}
</script>