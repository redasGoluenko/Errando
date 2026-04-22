<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
    <div class="bg-gradient-to-r from-amber-500 to-amber-600 px-8 py-6">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-2xl font-bold text-white">Payment History</h2>
          <p class="text-amber-100 text-sm mt-1">Track your payments</p>
        </div>
        <div class="bg-amber-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
          {{ payments.length }}
        </div>
      </div>
    </div>

    <div class="p-8">
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-amber-600"></div>
        <p class="text-gray-600 mt-3">Loading payment history...</p>
      </div>
      <div v-else-if="payments.length === 0" class="text-center py-12">
        <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p class="text-gray-600 text-lg">No payments yet</p>
        <p class="text-gray-500 text-sm mt-1">Your payments will appear here</p>
      </div>
      <div v-else class="space-y-3">
        <div v-for="payment in payments" :key="payment.id"
          class="p-4 bg-gradient-to-r from-amber-50 to-transparent rounded-lg border border-amber-100 hover:border-amber-300 transition">
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <div class="flex items-center gap-2">
                <h4 class="text-sm font-semibold text-gray-900">Payment #{{ payment.id }}</h4>
                <span :class="[
                  'text-xs font-semibold px-2 py-1 rounded-full',
                  getStatusBadgeClass(payment.status)
                ]">
                  {{ formatStatus(payment.status) }}
                </span>
              </div>
              <p class="text-xs text-gray-600 mt-1">Task ID: {{ payment.taskId }}</p>
            </div>
            <div class="text-right">
              <p class="text-sm font-bold text-gray-900">{{ formatAmount(payment.amount, payment.currency) }}</p>
              <p class="text-xs text-gray-500">{{ formatDate(payment.createdAt) }}</p>
            </div>
          </div>
        </div>
      </div>

      <router-link v-if="payments.length > 0"
        to="/payments"
        class="w-full px-6 py-3 bg-amber-600 text-white font-semibold rounded-lg hover:bg-amber-700 transition shadow-sm hover:shadow-md text-center block mt-6"
      >
        View Full History
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { paymentService, type Payment } from '@/services/paymentService'

const payments = ref<Payment[]>([])
const loading = ref(true)

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
  })
}

const loadPayments = async () => {
  try {
    loading.value = true
    payments.value = await paymentService.getMyPayments()
  } catch (error) {
    console.error('Failed to load payments:', error)
    payments.value = []
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadPayments()
})
</script>
