import { createRouter, createWebHistory } from 'vue-router'
import { authService } from '@/services/api'

import LoginView from '@/views/LoginView.vue'
import RegisterView from '@/views/RegisterView.vue'
import DashboardView from '@/views/DashboardView.vue'
import AdminUsersView from '@/views/AdminUsersView.vue'
import TasksView from '@/views/TasksView.vue'
import TaskDetailView from '@/views/TaskDetailView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login',
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: DashboardView,
      meta: { requiresAuth: true },
    },
    {
      path: '/admin/users',
      name: 'admin-users',
      component: AdminUsersView,
      meta: { requiresAuth: true, requiresAdmin: true },
    },
    {
      path: '/tasks',
      name: 'tasks',
      component: TasksView,
      meta: { requiresAuth: true },
    },
    {
      path: '/tasks/:id',
      name: 'task-detail',
      component: TaskDetailView,
      meta: { requiresAuth: true },
    },
    {
      path: '/runner/tasks',
      name: 'RunnerTasks',
      component: () => import('../views/RunnerTasksView.vue'),
      meta: { requiresAuth: true, role: 'Runner' },
    },
  ],
})

// Navigation guard
router.beforeEach((to, from, next) => {
  const token = authService.getToken()
  const role = authService.getRole()

  if (to.meta.requiresAuth && !token) {
    next('/login')
  } else if (to.meta.requiresAdmin && role !== 'Admin') {
    next('/dashboard')
  } else {
    next()
  }
})

export default router
