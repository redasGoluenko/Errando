<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { tasksService, type Task } from '@/services/tasksService'
import { taskItemsService, type TaskItem, type CreateTaskItemRequest, type UpdateTaskItemRequest } from '@/services/taskItemsService'
import { authService } from '@/services/api'
import Modal from '@/components/Modal.vue'
import TaskItemForm from '@/components/TaskItemForm.vue'
import Toast from '@/components/Toast.vue'

const route = useRoute()
const router = useRouter()

// State
const task = ref<Task | null>(null)
const taskItems = ref<TaskItem[]>([])
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
const selectedTaskItem = ref<TaskItem | null>(null)

// User info
const userRole = authService.getRole()
const userId = authService.getUserId()

const taskId = computed(() => parseInt(route.params.id as string))

// Fetch task and items on mount
onMounted(async () => {
  await fetchTask()
  await fetchTaskItems()
})

// Helper: Show toast notification
function showNotification(message: string, type: 'success' | 'error' = 'success') {
  toastMessage.value = message
  toastType.value = type
  showToast.value = true
}

async function fetchTask() {
  loading.value = true
  error.value = ''
  try {
    task.value = await tasksService.getTaskById(taskId.value)
  } catch (err: any) {
    console.error('Failed to fetch task:', err)
    error.value = err.response?.data?.message || 'Failed to load task'
  } finally {
    loading.value = false
  }
}

async function fetchTaskItems() {
  loading.value = true
  error.value = ''
  try {
    taskItems.value = await taskItemsService.getTaskItemsByTaskId(taskId.value)
  } catch (err: any) {
    console.error('Failed to fetch task items:', err)
    error.value = err.response?.data?.message || 'Failed to load task items'
  } finally {
    loading.value = false
  }
}

// Create task item
async function handleCreateTaskItem(data: CreateTaskItemRequest | UpdateTaskItemRequest) {
  try {
    await taskItemsService.createTaskItem(data as CreateTaskItemRequest)
    showCreateModal.value = false
    await fetchTaskItems()
    showNotification('Item added successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to create task item:', err)
    error.value = err.response?.data?.message || 'Failed to create task item'
    showNotification('Failed to add item', 'error')
  }
}

// Edit task item
function openEditModal(item: TaskItem) {
  selectedTaskItem.value = item
  showEditModal.value = true
}

// Update task item
async function handleUpdateTaskItem(data: CreateTaskItemRequest | UpdateTaskItemRequest) {
  if (!selectedTaskItem.value) return

  try {
    await taskItemsService.updateTaskItem(selectedTaskItem.value.id, data as UpdateTaskItemRequest)
    showEditModal.value = false
    selectedTaskItem.value = null
    await fetchTaskItems()
    showNotification('Item updated successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to update task item:', err)
    error.value = err.response?.data?.message || 'Failed to update task item'
    showNotification('Failed to update item', 'error')
  }
}

// Toggle completion
async function toggleComplete(item: TaskItem) {
  try {
    await taskItemsService.toggleComplete(item.id, !item.isCompleted)
    await fetchTaskItems()
    showNotification(
      item.isCompleted ? 'Item marked as incomplete' : 'Item marked as complete',
      'success'
    )
  } catch (err: any) {
    console.error('Failed to toggle completion:', err)
    showNotification('Failed to update item status', 'error')
  }
}

// Delete task item
function openDeleteModal(item: TaskItem) {
  selectedTaskItem.value = item
  showDeleteModal.value = true
}

async function handleDeleteTaskItem() {
  if (!selectedTaskItem.value) return

  try {
    await taskItemsService.deleteTaskItem(selectedTaskItem.value.id)
    showDeleteModal.value = false
    const deletedDescription = selectedTaskItem.value.description
    selectedTaskItem.value = null
    await fetchTaskItems()
    showNotification(`Item "${deletedDescription}" deleted successfully!`, 'success')
  } catch (err: any) {
    console.error('Failed to delete task item:', err)
    error.value = err.response?.data?.message || 'Failed to delete task item'
    showNotification('Failed to delete item', 'error')
  }
}

function closeModals() {
  showCreateModal.value = false
  showEditModal.value = false
  showDeleteModal.value = false
  selectedTaskItem.value = null
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

// Progress calculation
const completedCount = computed(() => taskItems.value.filter((item) => item.isCompleted).length)
const totalCount = computed(() => taskItems.value.length)
const progressPercentage = computed(() =>
  totalCount.value > 0 ? Math.round((completedCount.value / totalCount.value) * 100) : 0
)

// Can user modify?
function canModify(): boolean {
  if (!task.value) return false
  if (userRole === 'Admin') return true
  if (userRole === 'Client' && task.value.clientId === userId) return true
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
          <div class="flex items-center gap-4">
            <button
              @click="router.push('/tasks')"
              class="p-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M15 19l-7-7 7-7"
                />
              </svg>
            </button>
            <div>
              <h1 class="text-2xl font-bold text-gray-800">{{ task?.title || 'Task Details' }}</h1>
              <p class="text-sm text-gray-600 mt-1">
                {{ completedCount }} of {{ totalCount }} items completed ({{ progressPercentage }}%)
              </p>
            </div>
          </div>
          <button
            v-if="canModify()"
            @click="showCreateModal = true"
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200 font-medium"
          >
            + Add Item
          </button>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading State -->
      <div v-if="loading && !task" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">Loading task details...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
        {{ error }}
      </div>

      <!-- Task Content -->
      <div v-else-if="task" class="space-y-6">
        <!-- Task Info Card -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <h2 class="text-xl font-semibold text-gray-900 mb-2">{{ task.title }}</h2>
              <p class="text-gray-600 mb-4">{{ task.description }}</p>
              <div class="flex items-center text-sm text-gray-500">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                  />
                </svg>
                Scheduled: {{ formatDate(task.scheduledTime) }}
              </div>
            </div>
          </div>

          <!-- Progress Bar -->
          <div class="mt-6">
            <div class="flex items-center justify-between text-sm text-gray-600 mb-2">
              <span>Progress</span>
              <span class="font-medium">{{ progressPercentage }}%</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div
                :style="{ width: `${progressPercentage}%` }"
                :class="[
                  'h-2 rounded-full transition-all duration-300',
                  progressPercentage === 100 ? 'bg-green-500' : 'bg-blue-500',
                ]"
              ></div>
            </div>
          </div>
        </div>

        <!-- Task Items List -->
        <div class="bg-white rounded-lg shadow-md overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-200">
            <h3 class="text-lg font-semibold text-gray-900">Task Items</h3>
          </div>

          <!-- Empty State -->
          <div v-if="taskItems.length === 0" class="p-12 text-center">
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
            <h3 class="mt-4 text-lg font-medium text-gray-900">No items yet</h3>
            <p class="mt-2 text-gray-500">Add your first item to get started</p>
            <button
              v-if="canModify()"
              @click="showCreateModal = true"
              class="mt-6 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200 font-medium"
            >
              Add First Item
            </button>
          </div>

          <!-- Items List -->
          <ul v-else class="divide-y divide-gray-200">
            <li
              v-for="item in taskItems"
              :key="item.id"
              class="px-6 py-4 hover:bg-gray-50 transition"
            >
              <div class="flex items-center gap-4">
                <!-- Checkbox -->
                <input
                  type="checkbox"
                  :checked="item.isCompleted"
                  @change="toggleComplete(item)"
                  :disabled="!canModify()"
                  class="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                />

                <!-- Description -->
                <div class="flex-1">
                  <p
                    :class="[
                      'text-gray-900',
                      item.isCompleted && 'line-through text-gray-500',
                    ]"
                  >
                    {{ item.description }}
                  </p>
                </div>

                <!-- Actions -->
                <div v-if="canModify()" class="flex items-center gap-2">
                  <button
                    @click="openEditModal(item)"
                    class="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition"
                    title="Edit item"
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
                    @click="openDeleteModal(item)"
                    class="p-2 text-red-600 hover:bg-red-50 rounded-lg transition"
                    title="Delete item"
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
            </li>
          </ul>
        </div>
      </div>
    </main>

    <!-- Create Modal -->
    <Modal :show="showCreateModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Add Task Item</h3>
      </template>
      <template #body>
        <TaskItemForm
          mode="create"
          :task-id="taskId"
          @submit="handleCreateTaskItem"
          @cancel="closeModals"
        />
      </template>
    </Modal>

    <!-- Edit Modal -->
    <Modal :show="showEditModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Edit Task Item</h3>
      </template>
      <template #body>
        <TaskItemForm
          v-if="selectedTaskItem"
          mode="edit"
          :task-id="taskId"
          :task-item="selectedTaskItem"
          @submit="handleUpdateTaskItem"
          @cancel="closeModals"
        />
      </template>
    </Modal>

    <!-- Delete Confirmation Modal -->
    <Modal :show="showDeleteModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Delete Task Item</h3>
      </template>
      <template #body>
        <p class="text-gray-600">
          Are you sure you want to delete this item:
          <span class="font-semibold">"{{ selectedTaskItem?.description }}"</span>?
        </p>
        <p class="text-sm text-red-600 mt-2">This action cannot be undone.</p>
        <div class="flex gap-3 mt-6">
          <button
            @click="handleDeleteTaskItem"
            class="flex-1 bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition duration-200 font-medium"
          >
            Delete Item
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