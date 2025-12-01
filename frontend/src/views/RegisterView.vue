<!-- filepath: frontend/src/views/RegisterView.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '@/services/api'

const router = useRouter()
const username = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const error = ref('')
const success = ref('')
const loading = ref(false)

async function handleRegister() {
  // Validation
  if (!username.value || !email.value || !password.value || !confirmPassword.value) {
    error.value = 'All fields are required'
    return
  }

  if (password.value !== confirmPassword.value) {
    error.value = 'Passwords do not match'
    return
  }

  if (password.value.length < 6) {
    error.value = 'Password must be at least 6 characters'
    return
  }

  // Email validation (basic)
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!emailRegex.test(email.value)) {
    error.value = 'Invalid email format'
    return
  }

  loading.value = true
  error.value = ''
  success.value = ''

  try {
    console.log('🚀 Register attempt:', { username: username.value, email: email.value })
    
    await authService.register({
      username: username.value,
      email: email.value,
      password: password.value,
    })

    console.log('✅ Registration success')
    success.value = 'Registration successful! Redirecting to login...'
    
    // Redirect to login after 2 seconds
    setTimeout(() => {
      router.push('/login')
    }, 2000)

  } catch (err: any) {
    console.error('❌ Registration error:', err)
    
    if (!err.response) {
      error.value = 'Network error - Cannot reach server'
    } else if (err.response.status === 400) {
      error.value = err.response?.data?.message || err.response?.data || 'Invalid registration data'
    } else if (err.response.status === 409) {
      error.value = 'Username or email already exists'
    } else {
      error.value = err.response?.data?.message || 'Registration failed. Please try again.'
    }
  } finally {
    loading.value = false
  }
}

// Clear messages when user starts typing
function clearMessages() {
  if (error.value || success.value) {
    error.value = ''
    success.value = ''
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-600 to-indigo-700 p-4">
    <div class="bg-white rounded-2xl shadow-2xl w-full max-w-md p-8 animate-fade-in">
      <!-- Header -->
      <h1 class="text-3xl font-bold text-center text-gray-800 mb-8">
        Create Account
      </h1>

      <!-- Success Alert -->
      <div 
        v-if="success" 
        class="mb-6 p-4 bg-green-50 border-l-4 border-green-500 rounded-lg"
      >
        <div class="flex items-center">
          <svg class="w-5 h-5 text-green-500 mr-3" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
          </svg>
          <p class="text-green-700 text-sm font-medium">{{ success }}</p>
        </div>
      </div>

      <!-- Error Alert -->
      <div 
        v-if="error" 
        class="mb-6 p-4 bg-red-50 border-l-4 border-red-500 rounded-lg animate-shake"
      >
        <div class="flex items-center">
          <svg class="w-5 h-5 text-red-500 mr-3" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
          </svg>
          <p class="text-red-700 text-sm font-medium">{{ error }}</p>
        </div>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleRegister" class="space-y-5">
        <!-- Username -->
        <div>
          <label for="username" class="block text-sm font-medium text-gray-700 mb-2">
            Username
          </label>
          <input
            id="username"
            v-model="username"
            @input="clearMessages"
            type="text"
            placeholder="Choose a username"
            required
            class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition duration-200"
          />
        </div>

        <!-- Email -->
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
            Email
          </label>
          <input
            id="email"
            v-model="email"
            @input="clearMessages"
            type="email"
            placeholder="Enter your email"
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
            @input="clearMessages"
            type="password"
            placeholder="Create a password (min 6 chars)"
            required
            class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition duration-200"
          />
        </div>

        <!-- Confirm Password -->
        <div>
          <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-2">
            Confirm Password
          </label>
          <input
            id="confirmPassword"
            v-model="confirmPassword"
            @input="clearMessages"
            type="password"
            placeholder="Re-enter your password"
            required
            class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition duration-200"
          />
        </div>

        <!-- Submit Button -->
        <button
          type="submit"
          :disabled="loading || !!success"
          class="w-full bg-gradient-to-r from-purple-600 to-indigo-600 text-white font-semibold py-3 rounded-lg hover:from-purple-700 hover:to-indigo-700 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 transform hover:-translate-y-0.5 transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
        >
          <span v-if="loading" class="flex items-center justify-center">
            <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Creating account...
          </span>
          <span v-else-if="success">✓ Success!</span>
          <span v-else>Create Account</span>
        </button>
      </form>

      <!-- Login Link -->
      <p class="text-center text-gray-600 mt-6">
        Already have an account?
        <router-link 
          to="/login" 
          class="text-purple-600 font-semibold hover:text-purple-700 hover:underline transition duration-200"
        >
          Login here
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

@keyframes shake {
  0%, 100% {
    transform: translateX(0);
  }
  10%, 30%, 50%, 70%, 90% {
    transform: translateX(-5px);
  }
  20%, 40%, 60%, 80% {
    transform: translateX(5px);
  }
}

.animate-fade-in {
  animation: fade-in 0.3s ease-out;
}

.animate-shake {
  animation: shake 0.5s ease-in-out;
}
</style>