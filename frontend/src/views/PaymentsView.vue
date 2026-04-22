<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { paymentService, type Payment } from '@/services/paymentService'
import { authService } from '@/services/api'
import Toast from '@/components/Toast.vue'

// State
const payments = ref<Payment[]>([])
const loading = ref(false)
const error = ref('')
const searchQuery = ref('')

// Toast state
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref<'success' | 'error'>('success')

// User info
const userRole = authService.getRole()
const username = authService.getUsername()

// Computed: Filter payments by search query
const filteredPayments = computed(() => {
  if (!searchQuery.value.trim()) {
    return payments.value
  }
  
  const query = searchQuery.value.toLowerCase()
  return payments.value.filter(p =>
    p.id.toString().includes(query) ||
    p.taskId.toString().includes(query) ||
    p.amount.toString().includes(query) ||
    p.status.toLowerCase().includes(query)
  )
})

// Computed: Group payments by status for summary
const paymentStats = computed(() => {
  return {
    total: payments.value.length,
    succeeded: payments.value.filter(p => p.status === 'succeeded').length,
    pending: payments.value.filter(p => p.status === 'pending').length,
    failed: payments.value.filter(p => p.status === 'failed').length,
    refunded: payments.value.filter(p => p.status === 'refunded').length,
    totalAmount: payments.value
      .filter(p => p.status === 'succeeded')
      .reduce((sum, p) => sum + p.amount, 0)
  }
})

// Fetch payments on mount
onMounted(() => {
  fetchPayments()
})

// Helper: Show toast notification
function showNotification(message: string, type: 'success' | 'error' = 'success') {
  toastMessage.value = message
  toastType.value = type
  showToast.value = true
}

async function fetchPayments() {
  loading.value = true
  error.value = ''
  try {
    payments.value = await paymentService.getMyPayments()
  } catch (err: any) {
    console.error('Failed to fetch payments:', err)
    error.value = err.response?.data?.message || 'Failed to load payments'
    showNotification('Failed to load payments', 'error')
  } finally {
    loading.value = false
  }
}

const getStatusBadgeClass = (status: string) => {
  switch (status) {
    case 'succeeded':
      return 'bg-green-100 text-green-700'
    case 'pending':
      return 'bg-yellow-100 text-yellow-700'
    case 'failed':
      return 'bg-red-100 text-red-700'
    case 'refunded':
      return 'bg-gray-100 text-gray-700'
    default:
      return 'bg-gray-100 text-gray-700'
  }
}

const formatStatus = (status: string) => {
  return status.charAt(0).toUpperCase() + status.slice(1)
}

const formatAmount = (amount: number, currency: string) => {
  const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency.toUpperCase(),
  })
  return formatter.format(amount)
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Toast Notification -->
    <Toast :show="showToast" :type="toastType" :message="toastMessage" @close="showToast = false" />

    <!-- Header -->
    <header class="bg-white shadow-md">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-800">Payment History</h1>
            <p class="text-sm text-gray-600 mt-1">Track all your payments</p>
          </div>
          <router-link
            to="/dashboard"
            class="px-4 py-2 text-gray-600 hover:text-gray-800 transition duration-200"
          >
            ← Back to Dashboard
          </router-link>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Statistics Cards -->
      <div v-if="payments.length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4 mb-8">
        <div class="bg-white rounded-lg shadow-md p-6">
          <p class="text-gray-600 text-sm font-medium">Total Payments</p>
          <p class="text-3xl font-bold text-gray-900 mt-2">{{ paymentStats.total }}</p>
        </div>
        <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-green-500">
          <p class="text-gray-600 text-sm font-medium">Succeeded</p>
          <p class="text-3xl font-bold text-green-600 mt-2">{{ paymentStats.succeeded }}</p>
        </div>
        <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-yellow-500">
          <p class="text-gray-600 text-sm font-medium">Pending</p>
          <p class="text-3xl font-bold text-yellow-600 mt-2">{{ paymentStats.pending }}</p>
        </div>
        <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-red-500">
          <p class="text-gray-600 text-sm font-medium">Failed</p>
          <p class="text-3xl font-bold text-red-600 mt-2">{{ paymentStats.failed }}</p>
        </div>
        <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-blue-500">
          <p class="text-gray-600 text-sm font-medium">Total Paid</p>
          <p class="text-2xl font-bold text-blue-600 mt-2">{{ formatAmount(paymentStats.totalAmount, 'usd') }}</p>
        </div>
      </div>

      <!-- Search Bar -->
      <div v-if="payments.length > 0" class="mb-6">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search by Payment ID, Task ID, amount, or status..."
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">Loading payments...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
        {{ error }}
      </div>

      <!-- Empty State -->
      <div v-else-if="payments.length === 0" class="bg-white rounded-lg shadow-md p-12 text-center">
        <svg
          class="mx-auto h-16 w-16 text-gray-400"
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
        <h3 class="mt-4 text-lg font-medium text-gray-900">No payments yet</h3>
        <p class="mt-2 text-gray-500">You haven't made any payments yet</p>
      </div>

      <!-- No search results -->
      <div v-else-if="filteredPayments.length === 0" class="bg-white rounded-lg shadow-md p-12 text-center">
        <svg
          class="mx-auto h-16 w-16 text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
          />
        </svg>
        <h3 class="mt-4 text-lg font-medium text-gray-900">No payments found</h3>
        <p class="mt-2 text-gray-500">Try adjusting your search criteria</p>
      </div>

      <!-- Payments Table -->
      <div v-else class="bg-white rounded-lg shadow-md overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50 border-b border-gray-200">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">Payment ID</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">Task ID</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">Amount</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">Status</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">Date</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200">
              <tr v-for="payment in filteredPayments" :key="payment.id" class="hover:bg-gray-50 transition">
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="text-sm font-medium text-gray-900">#{{ payment.id }}</span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="text-sm text-gray-600">{{ payment.taskId }}</span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="text-sm font-semibold text-gray-900">{{ formatAmount(payment.amount, payment.currency) }}</span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span :class="['text-xs font-semibold px-3 py-1 rounded-full', getStatusBadgeClass(payment.status)]">
                    {{ formatStatus(payment.status) }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="text-sm text-gray-600">{{ formatDate(payment.createdAt) }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </main>
  </div>
</template>
