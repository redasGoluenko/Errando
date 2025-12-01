<!-- filepath: frontend/src/views/DashboardView.vue -->
<script setup lang="ts">
import { useRouter } from 'vue-router'
import { authService } from '@/services/api'

const router = useRouter()
const username = authService.getUsername()
const role = authService.getRole()

function handleLogout() {
  authService.logout()
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Header -->
    <header class="bg-white shadow-md">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 flex items-center justify-between">
        <h1 class="text-2xl font-bold text-gray-800">Dashboard</h1>
        <button
          @click="handleLogout"
          class="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 transition duration-200"
        >
          Logout
        </button>
      </div>
    </header>

    <!-- Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="bg-white rounded-xl shadow-lg p-6">
        <h2 class="text-xl font-semibold text-gray-800 mb-4">
          Welcome back, <span class="text-purple-600">{{ username }}</span>!
        </h2>
        
        <div class="flex items-center gap-2 mb-6">
          <span class="text-gray-600">Role:</span>
          <span class="px-3 py-1 bg-purple-100 text-purple-700 rounded-full text-sm font-medium">
            {{ role }}
          </span>
        </div>

        <div class="border-t pt-6">
          <p class="text-gray-600">
            This is your dashboard. More features coming soon...
          </p>
        </div>
      </div>
    </main>
  </div>
</template>