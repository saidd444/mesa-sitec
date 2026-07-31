import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Usuario } from '../types/api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const usuario = ref<Usuario | null>(null)

  const setToken = (t: string, user: Usuario) => {
    token.value = t
    usuario.value = user
    localStorage.setItem('token', t)
  }

  const logout = () => {
    token.value = null
    usuario.value = null
    localStorage.removeItem('token')
  }

  return { token, usuario, setToken, logout }
})