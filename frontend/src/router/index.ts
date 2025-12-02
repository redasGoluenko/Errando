import { createRouter, createWebHistory } from 'vue-router'
import { authService } from '@/services/api'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: () => {
        // Redirect based on authentication status
        return authService.isAuthenticated() ? '/dashboard' : '/login'
      }
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/LoginView.vue'),
      beforeEnter: (to, from, next) => {
        if (authService.isAuthenticated()) {
          next('/dashboard')
        } else {
          next()
        }
      }
    },
    {
      path: '/register',
      name: 'Register',
      component: () => import('../views/RegisterView.vue'),
      beforeEnter: (to, from, next) => {
        if (authService.isAuthenticated()) {
          next('/dashboard')
        } else {
          next()
        }
      }
    },
    {
      path: '/dashboard',
      name: 'Dashboard',
      component: () => import('../views/DashboardView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/tasks',
      name: 'Tasks',
      component: () => import('../views/TasksView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/tasks/:id',
      name: 'TaskDetail',
      component: () => import('../views/TaskDetailView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/admin/users',
      name: 'AdminUsers',
      component: () => import('../views/AdminUsersView.vue'),
      meta: { requiresAuth: true, role: 'Admin' }
    },
    {
      path: '/runner',
      redirect: '/runner/tasks'
    },
    {
      path: '/runner/tasks',
      name: 'RunnerTasks',
      component: () => import('../views/RunnerTasksView.vue'),
      meta: { requiresAuth: true, role: 'Runner' }
    }
  ]
})

// Global navigation guard
router.beforeEach((to, from, next) => {
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const isAuthenticated = authService.isAuthenticated()

  if (requiresAuth && !isAuthenticated) {
    next('/login')
  } else if (to.meta.role) {
    const userRole = authService.getRole()
    if (userRole !== to.meta.role && userRole !== 'Admin') {
      next('/dashboard')
    } else {
      next()
    }
  } else {
    next()
  }
})

export default router
