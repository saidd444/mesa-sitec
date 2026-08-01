import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from './pages/Login.vue'
import SolicitudesPage from './pages/Solicitudes.vue'
import SolicitudDetallePage from './pages/SolicitudDetalle.vue'
import SolicitudFormPage from './pages/SolicitudForm.vue'

const routes = [
  { path: '/', redirect: '/solicitudes' },
  { path: '/login', component: LoginPage },
  { path: '/solicitudes', component: SolicitudesPage },
  { path: '/solicitudes/nueva', component: SolicitudFormPage },
  { path: '/solicitudes/:id', component: SolicitudDetallePage },
  { path: '/solicitudes/:id/editar', component: SolicitudFormPage }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router