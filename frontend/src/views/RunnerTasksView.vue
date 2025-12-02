<!-- filepath: frontend/src/views/RunnerTasksView.vue -->
<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Toast -->
    <Toast :show="showToast" :type="toastType" :message="toastMessage" @close="showToast = false" />

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Back Button -->
      <router-link
        to="/dashboard"
        class="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6 transition"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        Back to Dashboard
      </router-link>

      <h1 class="text-3xl font-bold text-gray-900 mb-2">Runner Dashboard</h1>
      <p class="text-gray-600 mb-8">View and manage your assigned tasks</p>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">Loading tasks...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
        {{ error }}
      </div>

      <!-- Tasks Content -->
      <div v-else class="space-y-8">
        <!-- My Assigned Tasks -->
        <section>
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-2xl font-semibold text-gray-800">My Tasks</h2>
            <span class="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm font-medium">
              {{ myTasks.length }} tasks
            </span>
          </div>

          <!-- Empty State -->
          <div v-if="myTasks.length === 0" class="bg-white rounded-lg shadow-md p-12 text-center">
            <svg class="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
            <h3 class="mt-4 text-lg font-medium text-gray-900">No assigned tasks</h3>
            <p class="mt-2 text-gray-500">Assign yourself a task from the available tasks below</p>
          </div>

          <!-- My Tasks Grid -->
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div
              v-for="task in myTasks"
              :key="task.id"
              class="bg-white rounded-lg shadow-md hover:shadow-lg transition p-6 cursor-pointer"
              @click="viewTask(task.id)"
            >
              <div class="flex items-start justify-between mb-3">
                <h3 class="text-lg font-semibold text-gray-900 flex-1">{{ task.title }}</h3>
                <span
                  :class="[
                    'px-2 py-1 text-xs font-medium rounded-full',
                    isOverdue(task.scheduledTime)
                      ? 'bg-red-100 text-red-800'
                      : 'bg-green-100 text-green-800',
                  ]"
                >
                  {{ isOverdue(task.scheduledTime) ? 'Overdue' : 'Active' }}
                </span>
              </div>

              <p class="text-gray-600 text-sm mb-4 line-clamp-2">{{ task.description }}</p>

              <div class="flex items-center text-sm text-gray-500 mb-4">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                {{ formatDate(task.scheduledTime) }}
              </div>

              <!-- Task Card (if showing runner info) -->
              <div v-if="task.runnerId" class="text-sm text-gray-600 mt-2">
                <span>Assigned to: {{ task.runnerUsername || 'Unknown Runner' }}</span>
              </div>

              <button
                @click.stop="unassignTask(task.id)"
                class="w-full px-4 py-2 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 transition duration-200 font-medium text-sm"
              >
                Unassign Task
              </button>
            </div>
          </div>
        </section>

        <!-- Available Tasks -->
        <section>
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-2xl font-semibold text-gray-800">Available Tasks</h2>
            <span class="px-3 py-1 bg-gray-100 text-gray-800 rounded-full text-sm font-medium">
              {{ availableTasks.length }} tasks
            </span>
          </div>

          <!-- Empty State -->
          <div v-if="availableTasks.length === 0" class="bg-white rounded-lg shadow-md p-12 text-center">
            <svg class="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <h3 class="mt-4 text-lg font-medium text-gray-900">No available tasks</h3>
            <p class="mt-2 text-gray-500">All tasks are currently assigned</p>
          </div>

          <!-- Available Tasks Grid -->
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div
              v-for="task in availableTasks"
              :key="task.id"
              class="bg-white rounded-lg shadow-md hover:shadow-lg transition p-6"
            >
              <div class="flex items-start justify-between mb-3">
                <h3 class="text-lg font-semibold text-gray-900 flex-1">{{ task.title }}</h3>
                <span class="px-2 py-1 text-xs font-medium rounded-full bg-gray-100 text-gray-800">
                  Unassigned
                </span>
              </div>

              <p class="text-gray-600 text-sm mb-4 line-clamp-2">{{ task.description }}</p>

              <div class="flex items-center text-sm text-gray-500 mb-4">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                {{ formatDate(task.scheduledTime) }}
              </div>

              <button
                @click="assignTask(task.id)"
                class="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200 font-medium text-sm"
              >
                Assign to Me
              </button>
            </div>
          </div>
        </section>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { tasksService, type Task } from '@/services/tasksService'
import { authService } from '@/services/api'
import Toast from '@/components/Toast.vue'

const router = useRouter()

// State
const tasks = ref<Task[]>([])
const loading = ref(false)
const error = ref('')

// Toast
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref<'success' | 'error'>('success')

// User info
const userId = authService.getUserId()

// Filtered tasks
const availableTasks = computed(() => tasks.value.filter(t => !t.runnerId))
const myTasks = computed(() => tasks.value.filter(t => t.runnerId === userId))

onMounted(() => {
  fetchTasks()
})

function showNotification(message: string, type: 'success' | 'error' = 'success') {
  toastMessage.value = message
  toastType.value = type
  showToast.value = true
}

async function fetchTasks() {
  loading.value = true
  error.value = ''
  try {
    tasks.value = await tasksService.getAllTasks()
  } catch (err: any) {
    console.error('Failed to fetch tasks:', err)
    error.value = err.response?.data?.message || 'Failed to load tasks'
  } finally {
    loading.value = false
  }
}

async function assignTask(taskId: number) {
  try {
    await tasksService.assignTask(taskId)
    await fetchTasks()
    showNotification('Task assigned successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to assign task:', err)
    showNotification(err.response?.data?.message || 'Failed to assign task', 'error')
  }
}

async function unassignTask(taskId: number) {
  try {
    await tasksService.unassignTask(taskId)
    await fetchTasks()
    showNotification('Task unassigned successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to unassign task:', err)
    showNotification(err.response?.data?.message || 'Failed to unassign task', 'error')
  }
}

function viewTask(taskId: number) {
  router.push(`/tasks/${taskId}`)
}

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

function isOverdue(scheduledTime: string): boolean {
  return new Date(scheduledTime) < new Date()
}
</script>