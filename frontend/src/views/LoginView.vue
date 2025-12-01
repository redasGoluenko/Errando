<!-- filepath: frontend/src/views/LoginView.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '@/services/api'

const router = useRouter()
const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleLogin() {
  if (!username.value || !password.value) {
    error.value = 'Username and password are required'
    return
  }

  loading.value = true
  error.value = ''

  try {
    await authService.login({
      username: username.value,
      password: password.value,
    })
    router.push('/dashboard')
  } catch (err: any) {
    error.value = err.response?.data?.message || err.response?.data || 'Login failed'
    console.error('Login error:', err)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-600 to-indigo-700 p-4">
    <div class="bg-white rounded-2xl shadow-2xl w-full max-w-md p-8 animate-fade-in">
      <!-- Header -->
      <h1 class="text-3xl font-bold text-center text-gray-800 mb-8">
        Login
      </h1>

      <!-- Form -->
      <form @submit.prevent="handleLogin" class="space-y-6">
        <!-- Username -->
        <div>
          <label for="username" class="block text-sm font-medium text-gray-700 mb-2">
            Username
          </label>
          <input
            id="username"
            v-model="username"
            type="text"
            placeholder="Enter username"
            required
            class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition duration-200"
          />
        </div>

        <!-- Password -->
        <div>
          <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
            Password
          </label>
          <input
            id="password"
            v-model="password"
            type="password"
            placeholder="Enter password"
            required
            class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition duration-200"
          />
        </div>

        <!-- Submit Button -->
        <button
          type="submit"
          :disabled="loading"
          class="w-full bg-gradient-to-r from-purple-600 to-indigo-600 text-white font-semibold py-3 rounded-lg hover:from-purple-700 hover:to-indigo-700 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 transform hover:-translate-y-0.5 transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
        >
          {{ loading ? 'Loading...' : 'Login' }}
        </button>

        <!-- Error Message -->
        <p v-if="error" class="text-red-500 text-center text-sm mt-4">
          {{ error }}
        </p>
      </form>

      <!-- Register Link -->
      <p class="text-center text-gray-600 mt-6">
        Don't have an account?
        <router-link 
          to="/register" 
          class="text-purple-600 font-semibold hover:text-purple-700 hover:underline transition duration-200"
        >
          Register here
        </router-link>
      </p>
    </div>
  </div>
</template>

<style scoped>
@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fade-in {
  animation: fade-in 0.3s ease-out;
}
</style>