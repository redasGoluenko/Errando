<!-- filepath: frontend/src/components/Navbar.vue -->
<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '@/services/authService'

const router = useRouter()
const isAuthenticated = computed(() => authService.isAuthenticated())
const currentUser = computed(() => authService.getCurrentUser())
const userRole = computed(() => currentUser.value?.role)

const mobileMenuOpen = ref(false)

function toggleMobileMenu() {
  mobileMenuOpen.value = !mobileMenuOpen.value
}

function closeMobileMenu() {
  mobileMenuOpen.value = false
}

function logout() {
  authService.logout()
  closeMobileMenu()
  router.push('/login')
}
</script>

<template>
  <nav class="bg-white shadow-md sticky top-0 z-50">
    <div class="max-w-7xl mx-auto px-4">
      <div class="flex justify-between items-center h-16">
        <!-- Logo -->
        <router-link to="/" class="flex items-center space-x-2" @click="closeMobileMenu">
          <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"
            />
          </svg>
          <span class="text-xl font-bold text-gray-900">Errando</span>
        </router-link>

        <!-- Desktop Navigation -->
        <div class="hidden md:flex items-center space-x-6">
          <template v-if="isAuthenticated">
            <!-- Client/Admin Links -->
            <router-link
              v-if="userRole === 'Client' || userRole === 'Admin'"
              to="/tasks"
              class="text-gray-700 hover:text-blue-600 transition font-medium"
            >
              My Tasks
            </router-link>

            <!-- Runner Links -->
            <router-link
              v-if="userRole === 'Runner'"
              to="/runner"
              class="text-gray-700 hover:text-blue-600 transition font-medium"
            >
              Runner Dashboard
            </router-link>

            <!-- User Menu -->
            <div class="flex items-center space-x-3 border-l pl-6">
              <span class="text-sm text-gray-600">
                {{ currentUser?.username }}
                <span class="text-xs text-gray-400">({{ currentUser?.role }})</span>
              </span>
              <button
                @click="logout"
                class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition duration-200 font-medium"
              >
                Logout
              </button>
            </div>
          </template>

          <!-- Guest Links -->
          <template v-else>
            <router-link
              to="/login"
              class="text-gray-700 hover:text-blue-600 transition font-medium"
            >
              Login
            </router-link>
            <router-link
              to="/register"
              class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200 font-medium"
            >
              Register
            </router-link>
          </template>
        </div>

        <!-- Mobile Menu Button -->
        <button
          @click="toggleMobileMenu"
          class="md:hidden p-2 rounded-lg hover:bg-gray-100 transition"
          aria-label="Toggle menu"
        >
          <svg
            v-if="!mobileMenuOpen"
            class="w-6 h-6 text-gray-700"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M4 6h16M4 12h16M4 18h16"
            />
          </svg>
          <svg
            v-else
            class="w-6 h-6 text-gray-700"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
      </div>

      <!-- Mobile Menu -->
      <div
        v-if="mobileMenuOpen"
        class="md:hidden border-t border-gray-200 py-4 space-y-2 animate-fadeIn"
      >
        <template v-if="isAuthenticated">
          <!-- Client/Admin Links -->
          <router-link
            v-if="userRole === 'Client' || userRole === 'Admin'"
            to="/tasks"
            @click="closeMobileMenu"
            class="block px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-lg transition font-medium"
          >
            My Tasks
          </router-link>

          <!-- Runner Links -->
          <router-link
            v-if="userRole === 'Runner'"
            to="/runner"
            @click="closeMobileMenu"
            class="block px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-lg transition font-medium"
          >
            Runner Dashboard
          </router-link>

          <!-- User Info -->
          <div class="px-4 py-3 bg-gray-50 rounded-lg">
            <p class="text-sm text-gray-600">Logged in as:</p>
            <p class="font-semibold text-gray-900">{{ currentUser?.username }}</p>
            <p class="text-xs text-gray-500">{{ currentUser?.role }}</p>
          </div>

          <!-- Logout -->
          <button
            @click="logout"
            class="w-full px-4 py-3 bg-red-600 text-white rounded-lg hover:bg-red-700 transition font-medium"
          >
            Logout
          </button>
        </template>

        <!-- Guest Links -->
        <template v-else>
          <router-link
            to="/login"
            @click="closeMobileMenu"
            class="block px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-lg transition font-medium"
          >
            Login
          </router-link>
          <router-link
            to="/register"
            @click="closeMobileMenu"
            class="block px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition font-medium text-center"
          >
            Register
          </router-link>
        </template>
      </div>
    </div>
  </nav>
</template>

<style scoped>
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fadeIn {
  animation: fadeIn 0.2s ease-out;
}
</style>