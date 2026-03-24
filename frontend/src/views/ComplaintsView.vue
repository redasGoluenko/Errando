<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { complaintsService, type Complaint } from '@/services/complaintsService'
import { authService } from '@/services/api'
import Toast from '@/components/Toast.vue'
import Modal from '@/components/Modal.vue'

// State
const complaints = ref<Complaint[]>([])
const loading = ref(false)
const error = ref('')

// Toast
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref<'success' | 'error'>('success')

// Modal
const showDeleteModal = ref(false)
const selectedComplaint = ref<Complaint | null>(null)

// User info
const userRole = authService.getRole()

// Fetch complaints on mount
onMounted(() => {
  if (userRole !== 'Admin') {
    error.value = 'Access denied. Admins only.'
    return
  }
  fetchComplaints()
})

// Helper: Show toast notification
function showNotification(message: string, type: 'success' | 'error' = 'success') {
  toastMessage.value = message
  toastType.value = type
  showToast.value = true
}

// Fetch complaints
async function fetchComplaints() {
  loading.value = true
  error.value = ''
  try {
    complaints.value = await complaintsService.getAllComplaints()
  } catch (err: any) {
    console.error('Failed to fetch complaints:', err)
    error.value = err.response?.data?.message || 'Failed to load complaints'
  } finally {
    loading.value = false
  }
}

// Delete complaint
function openDeleteModal(complaint: Complaint) {
  selectedComplaint.value = complaint
  showDeleteModal.value = true
}

async function handleDeleteComplaint() {
  if (!selectedComplaint.value) return

  try {
    await complaintsService.deleteComplaint(selectedComplaint.value.id)
    showDeleteModal.value = false
    const complaintTask = selectedComplaint.value.taskTitle
    selectedComplaint.value = null
    await fetchComplaints()
    showNotification(`Complaint for "${complaintTask}" deleted successfully!`, 'success')
  } catch (err: any) {
    console.error('Failed to delete complaint:', err)
    showNotification('Failed to delete complaint', 'error')
  }
}

// Resolve complaint
async function handleResolveComplaint(complaint: Complaint) {
  try {
    await complaintsService.resolveComplaint(complaint.id)
    await fetchComplaints()
    showNotification(`Complaint for "${complaint.taskTitle}" marked as resolved!`, 'success')
  } catch (err: any) {
    console.error('Failed to resolve complaint:', err)
    showNotification('Failed to resolve complaint', 'error')
  }
}

function closeModal() {
  showDeleteModal.value = false
  selectedComplaint.value = null
}

// Format date
function formatDate(isoString: string): string {
  const date = new Date(isoString)
  return date.toLocaleString('lt-LT', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
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
            <h1 class="text-2xl font-bold text-gray-800">Complaints Management</h1>
            <p class="text-sm text-gray-600 mt-1">Review complaints from clients about completed tasks</p>
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
      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">Loading complaints...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
        {{ error }}
      </div>

      <!-- Empty State -->
      <div v-else-if="complaints.length === 0" class="bg-white rounded-lg shadow-md p-12 text-center">
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
            d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
        <h3 class="mt-4 text-lg font-medium text-gray-900">No complaints</h3>
        <p class="mt-2 text-gray-500">All tasks completed without complaints</p>
      </div>

      <!-- Complaints List -->
      <div v-else class="space-y-4">
        <div v-for="complaint in complaints" :key="complaint.id" class="bg-white rounded-lg shadow-md p-6">
          <!-- Complaint Header -->
          <div class="flex items-start justify-between mb-4">
            <div class="flex-1">
              <div class="flex items-center gap-2">
                <h3 class="text-lg font-semibold text-gray-900">{{ complaint.taskTitle }}</h3>
                <span
                  v-if="complaint.isResolved"
                  class="px-2 py-1 text-xs font-semibold bg-green-100 text-green-700 rounded"
                >
                  Resolved
                </span>
              </div>
              <p class="text-sm text-gray-600 mt-1">Task ID: {{ complaint.taskId }}</p>
            </div>
            <div class="flex gap-2">
              <button
                v-if="!complaint.isResolved"
                @click="handleResolveComplaint(complaint)"
                class="px-3 py-2 text-green-600 hover:bg-green-50 rounded-lg transition text-sm font-medium"
                title="Mark complaint as resolved"
              >
                ✓ Resolve
              </button>
              <button
                @click="openDeleteModal(complaint)"
                class="p-2 text-red-600 hover:bg-red-50 rounded-lg transition"
                title="Delete complaint"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                  />
                </svg>
              </button>
            </div>
          </div>

          <!-- Complaint Details -->
          <div class="bg-orange-50 border border-orange-200 rounded-lg p-4 mb-4">
            <p class="text-gray-900">{{ complaint.description }}</p>
          </div>

          <!-- Complaint Meta -->
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div>
              <p class="text-gray-600">Client</p>
              <p class="font-semibold text-gray-900">{{ complaint.clientUsername }}</p>
            </div>
            <div>
              <p class="text-gray-600">Runner</p>
              <p class="font-semibold text-gray-900">{{ complaint.runnerUsername }}</p>
            </div>
            <div class="md:col-span-2">
              <p class="text-gray-600">Submitted</p>
              <p class="font-semibold text-gray-900">{{ formatDate(complaint.createdAt) }}</p>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Delete Confirmation Modal -->
    <Modal :show="showDeleteModal" @close="closeModal">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Delete Complaint</h3>
      </template>
      <template #body>
        <p class="text-gray-600">
          Are you sure you want to delete the complaint for
          <span class="font-semibold">"{{ selectedComplaint?.taskTitle }}"</span>?
        </p>
        <p class="text-sm text-red-600 mt-2">This action cannot be undone.</p>
        <div class="flex gap-3 mt-6">
          <button
            @click="handleDeleteComplaint"
            class="flex-1 bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition duration-200 font-medium"
          >
            Delete Complaint
          </button>
          <button
            @click="closeModal"
            class="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition duration-200 font-medium"
          >
            Cancel
          </button>
        </div>
      </template>
    </Modal>
  </div>
</template>
