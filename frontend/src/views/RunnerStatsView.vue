<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getRunnerStats, type RunnerStats } from '@/services/runnerStatsService'
import Toast from '@/components/Toast.vue'

const router = useRouter()

// State
const runners = ref<RunnerStats[]>([])
const loading = ref(false)
const error = ref('')
const searchQuery = ref('')

// Toast state
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref<'success' | 'error'>('success')

// Computed: Filtered runners based on search
const filteredRunners = computed(() => {
  if (!searchQuery.value.trim()) {
    return runners.value
  }
  return runners.value.filter(runner =>
    runner.username.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

// Fetch runners on mount
onMounted(() => {
  fetchRunners()
})

// Helper: Show toast notification
function showNotification(message: string, type: 'success' | 'error' = 'success') {
  toastMessage.value = message
  toastType.value = type
  showToast.value = true
}

async function fetchRunners() {
  loading.value = true
  error.value = ''
  try {
    runners.value = await getRunnerStats()
  } catch (err: any) {
    console.error('Failed to fetch runner stats:', err)
    error.value = err.response?.data?.message || 'Failed to load runner stats'
    showNotification('Failed to load runner stats', 'error')
  } finally {
    loading.value = false
  }
}

// Format currency
function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('lt-LT', {
    style: 'currency',
    currency: 'EUR'
  }).format(amount)
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
        class="mb-6 flex items-center gap-2 text-blue-600 hover:text-blue-700 font-medium transition"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        Back to Dashboard
      </button>

      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">Runner Statistics</h1>
        <p class="text-gray-600">Browse runner profiles and view their performance metrics</p>
      </div>

      <!-- Search Bar -->
      <div class="mb-6">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search runners by username..."
          class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
        <p class="text-sm text-gray-500 mt-2">
          {{ filteredRunners.length }} runner(s) found
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
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
        <p class="text-gray-600 mt-4">Loading runner statistics...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredRunners.length === 0" class="text-center py-12">
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
          {{ searchQuery ? 'No runners found matching your search' : 'No runners available' }}
        </p>
      </div>

      <!-- Runners Table/Cards -->
      <div v-else class="grid grid-cols-1 gap-4">
        <div
          v-for="runner in filteredRunners"
          :key="runner.id"
          :class="['rounded-lg border border-gray-200 hover:shadow-lg transition-shadow', getRatingBgColor(runner.rating)]"
        >
          <div class="p-6">
            <div class="flex items-start justify-between">
              <!-- Runner Info -->
              <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-900 mb-2">{{ runner.username }}</h3>

                <!-- Stats Grid -->
                <div class="grid grid-cols-3 gap-4">
                  <!-- Rating -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Rating</p>
                    <div class="flex items-center gap-2">
                      <span :class="['text-2xl font-bold', getRatingColor(runner.rating)]">
                        {{ runner.rating.toFixed(2) }}
                      </span>
                      <span class="text-yellow-400">★</span>
                    </div>
                  </div>

                  <!-- Tasks Completed -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Completed Tasks</p>
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
                          d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                        />
                      </svg>
                      <span class="text-2xl font-bold text-gray-900">{{ runner.tasksCompleted }}</span>
                    </div>
                  </div>

                  <!-- Money Earned -->
                  <div>
                    <p class="text-sm text-gray-600 mb-1">Money Earned</p>
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
                          d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                        />
                      </svg>
                      <span class="text-2xl font-bold text-green-600">
                        {{ formatCurrency(runner.moneyEarned) }}
                      </span>
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
