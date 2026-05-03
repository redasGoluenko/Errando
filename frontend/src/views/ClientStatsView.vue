<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getClientStats, type ClientStats } from '@/services/clientStatsService'
import Toast from '@/components/Toast.vue'

const router = useRouter()

// State
const clients = ref<ClientStats[]>([])
const loading = ref(false)
const error = ref('')
const searchQuery = ref('')

// Toast state
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref<'success' | 'error'>('success')

// Computed: Filtered clients based on search
const filteredClients = computed(() => {
  if (!searchQuery.value.trim()) {
    return clients.value
  }
  return clients.value.filter(client =>
    client.username.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

// Fetch clients on mount
onMounted(() => {
  fetchClients()
})

// Helper: Show toast notification
function showNotification(message: string, type: 'success' | 'error' = 'success') {
  toastMessage.value = message
  toastType.value = type
  showToast.value = true
}

async function fetchClients() {
  loading.value = true
  error.value = ''
  try {
    clients.value = await getClientStats()
  } catch (err: any) {
    console.error('Failed to fetch client stats:', err)
    error.value = err.response?.data?.message || 'Failed to load client stats'
    showNotification('Failed to load client stats', 'error')
  } finally {
    loading.value = false
  }
}

// Format rating with color
function getRatingColor(rating: number): string {
  if (rating >= 4.5) return 'text-green-600'
  if (rating >= 4) return 'text-blue-600'
  if (rating >= 3) return 'text-yellow-600'
  return 'text-red-600'
}

function getRatingBgColor(rating: number): string {
  if (rating >= 4.5) return 'bg-green-50'
  if (rating >= 4) return 'bg-blue-50'
  if (rating >= 3) return 'bg-yellow-50'
  return 'bg-red-50'
}

// Format currency
function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('lt-LT', {
    style: 'currency',
    currency: 'EUR'
  }).format(amount)
}

function goBack() {
  router.push('/dashboard')
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 p-6">
    <div class="max-w-6xl mx-auto">
      <!-- Back Button -->
      <button
        @click="goBack"
        class="mb-6 flex items-center gap-2 text-cyan-600 hover:text-cyan-700 font-medium transition"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        Back to Dashboard
      </button>

      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">Client Statistics</h1>
        <p class="text-gray-600">Browse client profiles and view their ratings</p>
      </div>

      <!-- Search Bar -->
      <div class="mb-6">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search clients by username..."
          class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-cyan-500 focus:border-transparent"
        />
        <p class="text-sm text-gray-500 mt-2">
          {{ filteredClients.length }} client(s) found
        </p>
      </div>

      <!-- Error Message -->
      <div
        v-if="error"
        class="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg"
      >
        {{ error }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-cyan-600"></div>
        </div>
        <p class="text-gray-600 mt-4">Loading client statistics...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredClients.length === 0" class="text-center py-12">
        <svg
          class="mx-auto h-12 w-12 text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M17 20h5v-2a3 3 0 00-5.856-1.487M15 10a3 3 0 11-6 0 3 3 0 016 0zM12 14a8 8 0 00-8 8v2h16v-2a8 8 0 00-8-8z"
          />
        </svg>
        <p class="text-gray-600 mt-4">
          {{ searchQuery ? 'No clients found matching your search' : 'No clients available' }}
        </p>
      </div>

      <!-- Clients Table/Cards -->
      <div v-else class="grid grid-cols-1 gap-4">
        <div
          v-for="client in filteredClients"
          :key="client.id"
          :class="['rounded-lg border border-gray-200 hover:shadow-lg transition-shadow', getRatingBgColor(client.rating)]"
        >
          <div class="p-6">
            <div class="flex items-start justify-between">
              <!-- Client Info -->
              <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-900 mb-2">{{ client.username }}</h3>

                <!-- Stats Grid -->
                <div class="grid grid-cols-2 gap-4 md:grid-cols-4">
                  <!-- Rating -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Rating</p>
                    <div class="flex items-center gap-2">
                      <span :class="['text-2xl font-bold', getRatingColor(client.rating)]">
                        {{ client.rating.toFixed(2) }}
                      </span>
                      <span class="text-yellow-400">★</span>
                    </div>
                  </div>

                  <!-- Total Reviews -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Reviews</p>
                    <div class="flex items-center gap-2">
                      <svg
                        class="w-5 h-5 text-purple-600"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z"
                        />
                      </svg>
                      <span class="text-2xl font-bold text-gray-900">{{ client.totalReviews }}</span>
                    </div>
                  </div>

                  <!-- Tasks Created -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Created</p>
                    <div class="flex items-center gap-2">
                      <svg
                        class="w-5 h-5 text-blue-600"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                        />
                      </svg>
                      <span class="text-2xl font-bold text-gray-900">{{ client.tasksCreated }}</span>
                    </div>
                  </div>

                  <!-- Tasks Completed -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Completed</p>
                    <div class="flex items-center gap-2">
                      <svg
                        class="w-5 h-5 text-green-600"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                        />
                      </svg>
                      <span class="text-2xl font-bold text-gray-900">{{ client.tasksCompleted }}</span>
                    </div>
                  </div>

                  <!-- Active Tasks -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Active</p>
                    <div class="flex items-center gap-2">
                      <svg
                        class="w-5 h-5 text-cyan-600"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M13 10V3L4 14h7v7l9-11h-7z"
                        />
                      </svg>
                      <span class="text-2xl font-bold text-gray-900">{{ client.activeTasks }}</span>
                    </div>
                  </div>

                  <!-- Completion Rate -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Completion Rate</p>
                    <div class="flex items-center gap-2">
                      <svg
                        class="w-5 h-5 text-orange-600"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                        />
                      </svg>
                      <span class="text-2xl font-bold text-gray-900">{{ client.completionRate }}%</span>
                    </div>
                  </div>

                  <!-- Total Spent -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Total Spent</p>
                    <div class="flex items-center gap-1">
                      <svg
                        class="w-5 h-5 text-green-600"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                        />
                      </svg>
                      <span class="text-lg font-bold text-green-600">
                        {{ formatCurrency(client.totalSpent) }}
                      </span>
                    </div>
                  </div>

                  <!-- Complaints Filed -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Complaints</p>
                    <div class="flex items-center gap-2">
                      <svg
                        class="w-5 h-5 text-red-600"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M12 8v4m0 4v.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                        />
                      </svg>
                      <span class="text-2xl font-bold text-gray-900">{{ client.complaintsFiled }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Toast Notification -->
    <Toast
      :show="showToast"
      :message="toastMessage"
      :type="toastType"
      @close="showToast = false"
    />
  </div>
</template>
