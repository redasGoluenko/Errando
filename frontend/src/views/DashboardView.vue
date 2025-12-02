<!-- filepath: frontend/src/views/DashboardView.vue -->
<template>
  <div class="min-h-screen bg-gradient-to-br from-purple-50 via-blue-50 to-indigo-50">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">
          Welcome back, {{ username }}! 👋
        </h1>
        <p class="text-gray-600">{{ getDashboardGreeting() }}</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <!-- Admin: Manage Users -->
        <router-link
          v-if="role === 'Admin'"
          to="/admin/users"
          class="block bg-white rounded-lg shadow-md hover:shadow-lg transition p-6"
        >
          <div class="flex items-center gap-4">
            <div class="p-3 bg-red-100 rounded-lg">
              <svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-800">Manage Users</h3>
              <p class="text-sm text-gray-600">View and manage all users</p>
            </div>
          </div>
        </router-link>

        <!-- Tasks Management (Client + Admin) -->
        <router-link
          v-if="role === 'Client' || role === 'Admin'"
          to="/tasks"
          class="block bg-white rounded-lg shadow-md hover:shadow-lg transition p-6"
        >
          <div class="flex items-center gap-4">
            <div class="p-3 bg-blue-100 rounded-lg">
              <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-800">
                {{ role === 'Admin' ? 'All Tasks' : 'My Tasks' }}
              </h3>
              <p class="text-sm text-gray-600">
                {{ role === 'Admin' ? 'View and manage all tasks' : 'Create and manage your tasks' }}
              </p>
            </div>
          </div>
        </router-link>

        <!-- Runner: View Assigned Tasks -->
        <router-link
          v-if="role === 'Runner'"
          to="/runner/tasks"
          class="block bg-white rounded-lg shadow-md hover:shadow-lg transition p-6"
        >
          <div class="flex items-center gap-4">
            <div class="p-3 bg-green-100 rounded-lg">
              <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-800">My Tasks</h3>
              <p class="text-sm text-gray-600">View and manage assigned tasks</p>
            </div>
          </div>
        </router-link>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '@/services/api'

const router = useRouter()

const username = ref(authService.getUsername())
const role = ref(authService.getRole())

onMounted(() => {
  username.value = authService.getUsername()
  role.value = authService.getRole()
  console.log('Dashboard user:', username.value, role.value)
})

function getDashboardGreeting() {
  const hour = new Date().getHours()
  if (hour < 12) return 'Good morning!'
  if (hour < 18) return 'Good afternoon!'
  return 'Good evening!'
}

function logout() {
  authService.logout()
  router.push('/login')
}
</script>