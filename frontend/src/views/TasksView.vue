<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { tasksService, type Task, type CreateTaskRequest, type UpdateTaskRequest } from '@/services/tasksService'
import { authService } from '@/services/api'
import Modal from '@/components/Modal.vue'
import TaskForm from '@/components/TaskForm.vue'
import Toast from '@/components/Toast.vue'

// State
const tasks = ref<Task[]>([])
const loading = ref(false)
const error = ref('')

// Toast state
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref<'success' | 'error'>('success')

// Modal states
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDeleteModal = ref(false)
const selectedTask = ref<Task | null>(null)

// User info
const userRole = authService.getRole()
const userId = authService.getUserId()
const username = authService.getUsername()

// Fetch tasks on mount
onMounted(() => {
  fetchTasks()
})

// Helper: Show toast notification
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

// Create task
async function handleCreateTask(data: CreateTaskRequest | UpdateTaskRequest) {
  try {
    if ('clientId' in data) {
      await tasksService.createTask(data as CreateTaskRequest)
      showCreateModal.value = false
      await fetchTasks()
      showNotification('Task created successfully!', 'success')
    }
  } catch (err: any) {
    console.error('Failed to create task:', err)
    error.value = err.response?.data?.message || 'Failed to create task'
    showNotification('Failed to create task', 'error')
  }
}

// Edit task
function openEditModal(task: Task) {
  selectedTask.value = task
  showEditModal.value = true
}

// Update task
async function handleUpdateTask(data: CreateTaskRequest | UpdateTaskRequest) {
  if (!selectedTask.value) return

  try {
    await tasksService.updateTask(selectedTask.value.id, data as UpdateTaskRequest)
    showEditModal.value = false
    selectedTask.value = null
    await fetchTasks()
    showNotification('Task updated successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to update task:', err)
    error.value = err.response?.data?.message || 'Failed to update task'
    showNotification('Failed to update task', 'error')
  }
}

// Delete task
function openDeleteModal(task: Task) {
  selectedTask.value = task
  showDeleteModal.value = true
}

async function handleDeleteTask() {
  if (!selectedTask.value) return

  try {
    await tasksService.deleteTask(selectedTask.value.id)
    showDeleteModal.value = false
    const deletedTitle = selectedTask.value.title
    selectedTask.value = null
    await fetchTasks()
    showNotification(`Task "${deletedTitle}" deleted successfully!`, 'success')
  } catch (err: any) {
    console.error('Failed to delete task:', err)
    error.value = err.response?.data?.message || 'Failed to delete task'
    showNotification('Failed to delete task', 'error')
  }
}

function closeModals() {
  showCreateModal.value = false
  showEditModal.value = false
  showDeleteModal.value = false
  selectedTask.value = null
}

// Format date for display
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

// Check if task is upcoming or past
function isUpcoming(isoString: string): boolean {
  return new Date(isoString) > new Date()
}

// Check if task is overdue
function isOverdue(isoString: string): boolean {
  return new Date(isoString) < new Date()
}

// Can user edit/delete this task?
function canModifyTask(task: Task): boolean {
  if (userRole === 'Admin') return true
  if (userRole === 'Client' && task.clientId === userId) return true
  return false
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
            <h1 class="text-2xl font-bold text-gray-800">Tasks Management</h1>
            <p class="text-sm text-gray-600 mt-1">
              {{ userRole === 'Admin' ? 'All tasks' : 'Your tasks' }}
            </p>
          </div>
          <div class="flex gap-3">
            <button
              v-if="userRole === 'Client' || userRole === 'Admin'"
              @click="showCreateModal = true"
              class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200 font-medium"
            >
              + Create Task
            </button>
            <router-link
              to="/dashboard"
              class="px-4 py-2 text-gray-600 hover:text-gray-800 transition duration-200"
            >
              ← Back to Dashboard
            </router-link>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">Loading tasks...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
        {{ error }}
      </div>

      <!-- Empty State -->
      <div v-else-if="tasks.length === 0" class="bg-white rounded-lg shadow-md p-12 text-center">
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
            d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
          />
        </svg>
        <h3 class="mt-4 text-lg font-medium text-gray-900">No tasks yet</h3>
        <p class="mt-2 text-gray-500">Get started by creating your first task</p>
        <button
          v-if="userRole === 'Client' || userRole === 'Admin'"
          @click="showCreateModal = true"
          class="mt-6 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200 font-medium"
        >
          Create Your First Task
        </button>
      </div>

      <!-- Tasks Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="task in tasks"
          :key="task.id"
          class="bg-white rounded-lg shadow-md hover:shadow-lg transition duration-200 overflow-hidden"
        >
          <!-- Task Header -->
          <div class="p-6">
            <div class="flex items-start justify-between">
              <h3 class="text-lg font-semibold text-gray-900 flex-1">{{ task.title }}</h3>
              <span
                :class="[
                  'px-2 py-1 text-xs rounded-full font-medium',
                  isUpcoming(task.scheduledTime)
                    ? 'bg-green-100 text-green-700'
                    : 'bg-gray-100 text-gray-700',
                ]"
              >
                {{ isUpcoming(task.scheduledTime) ? 'Upcoming' : 'Past' }}
              </span>
            </div>

            <p class="mt-2 text-sm text-gray-600 line-clamp-2">{{ task.description }}</p>

            <!-- Task Meta -->
            <div class="mt-4 space-y-2">
              <div class="flex items-center text-sm text-gray-500">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                  />
                </svg>
                {{ formatDate(task.scheduledTime) }}
              </div>
              <div v-if="userRole === 'Admin'" class="flex items-center text-sm text-gray-500">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                  />
                </svg>
                Client ID: {{ task.clientId }}
              </div>
            </div>
          </div>

          <!-- Task Actions -->
          <div class="bg-gray-50 px-6 py-3 flex items-center justify-between gap-2 border-t">
            <!-- View Details Button -->
            <router-link
              :to="`/tasks/${task.id}`"
              class="flex-1 text-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200 font-medium text-sm"
            >
              View Details
            </router-link>

            <!-- Edit/Delete (only if can modify) -->
            <div v-if="canModifyTask(task)" class="flex items-center gap-2">
              <button
                @click="openEditModal(task)"
                class="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition"
                title="Edit task"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                  />
                </svg>
              </button>
              <button
                @click="openDeleteModal(task)"
                class="p-2 text-red-600 hover:bg-red-50 rounded-lg transition"
                title="Delete task"
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
        </div>
      </div>
    </main>

    <!-- Create Modal -->
    <Modal :show="showCreateModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Create New Task</h3>
      </template>
      <template #body>
        <TaskForm mode="create" @submit="handleCreateTask" @cancel="closeModals" />
      </template>
    </Modal>

    <!-- Edit Modal -->
    <Modal :show="showEditModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Edit Task</h3>
      </template>
      <template #body>
        <TaskForm
          v-if="selectedTask"
          mode="edit"
          :task="selectedTask"
          @submit="handleUpdateTask"
          @cancel="closeModals"
        />
      </template>
    </Modal>

    <!-- Delete Confirmation Modal -->
    <Modal :show="showDeleteModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Delete Task</h3>
      </template>
      <template #body>
        <p class="text-gray-600">
          Are you sure you want to delete the task
          <span class="font-semibold">"{{ selectedTask?.title }}"</span>?
        </p>
        <p class="text-sm text-red-600 mt-2">This action cannot be undone.</p>
        <div class="flex gap-3 mt-6">
          <button
            @click="handleDeleteTask"
            class="flex-1 bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition duration-200 font-medium"
          >
            Delete Task
          </button>
          <button
            @click="closeModals"
            class="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition duration-200 font-medium"
          >
            Cancel
          </button>
        </div>
      </template>
    </Modal>
  </div>
</template>