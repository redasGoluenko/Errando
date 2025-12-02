<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { tasksService, type Task } from '@/services/tasksService'
import { taskItemsService, type TaskItem, type CreateTaskItemRequest, type UpdateTaskItemRequest } from '@/services/taskItemsService'
import { statusLogsService, type StatusLog, type CreateStatusLogRequest } from '@/services/statusLogsService'
import { authService } from '@/services/api'
import Modal from '@/components/Modal.vue'
import TaskItemForm from '@/components/TaskItemForm.vue'
import Toast from '@/components/Toast.vue'

const route = useRoute()
const router = useRouter()
const taskId = ref(parseInt(route.params.id as string))

const task = ref<Task | null>(null)
const taskItems = ref<TaskItem[]>([])
const statusLogs = ref<Map<number, StatusLog[]>>(new Map())
const loading = ref(true)
const error = ref('')

const userRole = authService.getRole()
const userId = authService.getUserId()

// Add missing refs
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDeleteModal = ref(false)
const showStatusModal = ref(false)
const showEditItemModal = ref(false)
const showDeleteItemModal = ref(false)
const selectedTaskItem = ref<TaskItem | null>(null)
const newStatus = ref('Pending')
const newComment = ref('')
const itemDescription = ref('')
const showToast = ref(false)
const toastType = ref<'success' | 'error'>('success')
const toastMessage = ref('')

// Determine back route based on user role
const backRoute = computed(() => {
  return userRole === 'Runner' ? '/runner/tasks' : '/tasks'
})

const backLabel = computed(() => {
  return userRole === 'Runner' ? 'Runner Dashboard' : 'Tasks'
})

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

// Load status logs for a task item
const loadingLogs = ref<Set<number>>(new Set())

// Open status modal
function openStatusModal(item: TaskItem) {
  selectedTaskItem.value = item
  newStatus.value = 'InProgress'
  newComment.value = ''
  showStatusModal.value = true
}

async function loadStatusLogs(taskItemId: number) {
  loadingLogs.value.add(taskItemId)
  try {
    const logs = await statusLogsService.getStatusLogs(taskItemId)
    statusLogs.value.set(taskItemId, logs)
  } catch (err: any) {
    console.error('Failed to load status logs:', err)
  } finally {
    loadingLogs.value.delete(taskItemId)
  }
}

// Add status log
async function handleAddStatusLog() {
  if (!selectedTaskItem.value) return

  try {
    const request: CreateStatusLogRequest = {
      taskItemId: selectedTaskItem.value.id,
      status: newStatus.value,
      comment: newComment.value || undefined
    }

    await statusLogsService.createStatusLog(request)
    showStatusModal.value = false
    
    // Reload status logs for this item
    await loadStatusLogs(selectedTaskItem.value.id)
    
    showNotification('Status update added successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to add status log:', err)
    showNotification(err.response?.data?.message || 'Failed to add status update', 'error')
  }
}

// ADD THIS FUNCTION (alias for handleAddStatusLog)
async function handleUpdateStatus() {
  await handleAddStatusLog()
}

function closeModals() {
  showCreateModal.value = false
  showEditModal.value = false
  showDeleteModal.value = false
  showStatusModal.value = false
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
const completedCount = computed(() => {
  return taskItems.value.filter(item => item.isCompleted).length
})

const totalCount = computed(() => {
  return taskItems.value.length
})

const progressPercentage = computed(() => {
  if (totalCount.value === 0) return 0
  return Math.round((completedCount.value / totalCount.value) * 100)
})

// Can user modify?
function canModify(): boolean {
  if (!task.value) return false
  if (userRole === 'Admin') return true
  if (userRole === 'Client' && task.value.clientId === userId) return true
  return false
}

function getStatusColor(status: string): string {
  switch (status.toLowerCase()) {
    case 'completed':
      return 'text-green-600 bg-green-50'
    case 'inprogress':
    case 'in progress':
      return 'text-blue-600 bg-blue-50'
    case 'pending':
    default:
      return 'text-gray-600 bg-gray-50'
  }
}

function formatTimestamp(isoString: string): string {
  const date = new Date(isoString)
  return date.toLocaleString('lt-LT', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// Edit task item
function openEditItemModal(item: TaskItem) {
  selectedTaskItem.value = item
  itemDescription.value = item.description
  showEditItemModal.value = true
}

// Delete task item
function openDeleteItemModal(item: TaskItem) {
  selectedTaskItem.value = item
  showDeleteItemModal.value = true
}

async function handleEditItem() {
  if (!selectedTaskItem.value) return

  try {
    await taskItemsService.updateTaskItem(selectedTaskItem.value.id, {
      description: itemDescription.value,
      isCompleted: selectedTaskItem.value.isCompleted,
      taskId: selectedTaskItem.value.taskId,
    })

    showEditItemModal.value = false
    await fetchTaskItems()
    showNotification('Task item updated successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to update task item:', err)
    showNotification(err.response?.data?.message || 'Failed to update task item', 'error')
  }
}

async function handleDeleteItem() {
  if (!selectedTaskItem.value) return

  try {
    await taskItemsService.deleteTaskItem(selectedTaskItem.value.id)
    showDeleteItemModal.value = false
    await fetchTaskItems()
    showNotification('Task item deleted successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to delete task item:', err)
    showNotification(err.response?.data?.message || 'Failed to delete task item', 'error')
  }
}

// Add toggle completion function
async function toggleItemCompletion(item: TaskItem) {
  try {
    const updatedItem: UpdateTaskItemRequest = {
      description: item.description,
      isCompleted: !item.isCompleted,
      taskId: item.taskId  // ← ADD THIS
    }
    
    await taskItemsService.updateTaskItem(item.id, updatedItem)
    await fetchTaskItems()
    
    showNotification(
      item.isCompleted ? 'Item marked as incomplete' : 'Item marked as complete',
      'success'
    )
  } catch (err: any) {
    console.error('Failed to toggle item:', err)
    showNotification('Failed to update item', 'error')
  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Toast Notification -->
    <Toast :show="showToast" :type="toastType" :message="toastMessage" @close="showToast = false" />

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Back Button -->
      <router-link
        :to="backRoute"
        class="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6 transition"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        Back to {{ backLabel }}
      </router-link>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
        <p class="text-red-600 font-medium">{{ error }}</p>
        <router-link :to="backRoute" class="mt-4 inline-block text-blue-600 hover:text-blue-700">
          ← Go back
        </router-link>
      </div>

      <!-- Task Details -->
      <div v-else-if="task" class="space-y-6">
        <!-- Task Header Card -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-start justify-between mb-4">
            <div class="flex-1">
              <h1 class="text-3xl font-bold text-gray-900 mb-2">{{ task.title }}</h1>
              <p class="text-gray-600">{{ task.description }}</p>
            </div>
            <button
              v-if="canModify()"
              @click="showCreateModal = true"
              class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition font-medium"
            >
              + Add Item
            </button>
          </div>

          <!-- Progress Bar -->
          <div class="mt-6">
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm font-medium text-gray-700">Progress</span>
              <span class="text-sm font-medium text-gray-700">
                {{ completedCount }} / {{ totalCount }} completed ({{ progressPercentage }}%)
              </span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-3">
              <div
                class="bg-blue-600 h-3 rounded-full transition-all duration-300"
                :style="{ width: `${progressPercentage}%` }"
              ></div>
            </div>
          </div>

          <!-- Task Meta Info -->
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-6">
            <!-- Scheduled Time -->
            <div class="bg-gray-50 rounded-lg p-4">
              <div class="flex items-center gap-3 mb-2">
                <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span class="text-sm font-medium text-gray-600">Scheduled Time</span>
              </div>
              <p class="text-gray-900 ml-8">{{ formatDate(task.scheduledTime) }}</p>
            </div>

            <!-- Status -->
            <div class="bg-gray-50 rounded-lg p-4">
              <div class="flex items-center gap-3 mb-2">
                <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span class="text-sm font-medium text-gray-600">Status</span>
              </div>
              <p class="text-gray-900 ml-8">{{ task.status }}</p>
            </div>

            <!-- Assigned Runner -->
            <div class="bg-gray-50 rounded-lg p-4">
              <div class="flex items-center gap-3 mb-2">
                <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <span class="text-sm font-medium text-gray-600">Assigned Runner</span>
              </div>
              <p class="text-gray-900 ml-8">
                <template v-if="task.runnerId">
                  <span class="font-semibold">{{ task.runnerUsername || 'Unknown Runner' }}</span>
                  <span class="text-sm text-gray-500 ml-2">(ID: {{ task.runnerId }})</span>
                </template>
                <span v-else class="text-gray-400 italic">Not assigned yet</span>
              </p>
            </div>
          </div>
        </div>

        <!-- Task Items -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <h2 class="text-2xl font-bold text-gray-900 mb-6">Task Items</h2>

          <!-- Empty State -->
          <div v-if="taskItems.length === 0" class="text-center py-12">
            <svg class="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
            <h3 class="mt-4 text-lg font-medium text-gray-900">No items yet</h3>
            <p class="mt-2 text-gray-500">Add items to this task to get started</p>
          </div>

          <!-- Task Items List -->
          <div v-else class="space-y-3">
            <div
              v-for="item in taskItems"
              :key="item.id"
              class="bg-gray-50 rounded-lg overflow-hidden"
            >
              <div class="flex items-start gap-4 p-4">
                <!-- Checkbox (Runner can toggle) -->
                <div class="flex-shrink-0 mt-1">
                  <input
                    type="checkbox"
                    :checked="item.isCompleted"
                    @change="toggleItemCompletion(item)"
                    :disabled="userRole !== 'Runner' && userRole !== 'Admin'"
                    class="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-blue-500 cursor-pointer disabled:cursor-not-allowed disabled:opacity-50"
                  />
                </div>

                <!-- Item Content -->
                <div class="flex-1">
                  <p
                    :class="[
                      'text-gray-900',
                      item.isCompleted && 'line-through text-gray-500'
                    ]"
                  >
                    {{ item.description }}
                  </p>
                  <div v-if="item.isCompleted" class="mt-2">
                    <span class="px-2 py-0.5 bg-green-100 text-green-700 rounded-full font-medium text-xs">
                      ✓ Completed
                    </span>
                  </div>
                </div>

                <!-- Action Buttons -->
                <div class="flex gap-2">
                  <!-- Add Status Log (Runner/Admin) -->
                  <button
                    v-if="userRole === 'Runner' || userRole === 'Admin'"
                    @click="openStatusModal(item)"
                    class="p-2 text-green-600 hover:bg-green-50 rounded-lg transition"
                    title="Add status log"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
                    </svg>
                  </button>

                  <!-- Edit/Delete buttons (Client/Admin only) -->
                  <template v-if="userRole === 'Client' || userRole === 'Admin'">
                    <button
                      @click="openEditItemModal(item)"
                      class="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition"
                      title="Edit item"
                    >
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                    </button>
                    <button
                      @click="openDeleteItemModal(item)"
                      class="p-2 text-red-600 hover:bg-red-50 rounded-lg transition"
                      title="Delete item"
                    >
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </template>
                </div>
              </div>

              <!-- Status Logs for this item -->
              <div v-if="statusLogs.get(item.id)?.length" class="border-t border-gray-200 bg-white p-4">
                <h4 class="text-sm font-semibold text-gray-700 mb-3">Status Updates</h4>
                <div class="space-y-2">
                  <div
                    v-for="log in statusLogs.get(item.id)"
                    :key="log.id"
                    class="flex items-start gap-3 text-sm"
                  >
                    <div class="flex-shrink-0 w-2 h-2 mt-1.5 rounded-full bg-blue-500"></div>
                    <div class="flex-1">
                      <div class="flex items-center gap-2 mb-1">
                        <span
                          :class="[
                            'px-2 py-0.5 text-xs font-medium rounded-full',
                            log.status === 'Completed' ? 'bg-green-100 text-green-800' :
                            log.status === 'InProgress' ? 'bg-blue-100 text-blue-800' :
                            log.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                            'bg-gray-100 text-gray-800'
                          ]"
                        >
                          {{ log.status }}
                        </span>
                        <span class="text-xs text-gray-500">{{ formatDate(log.timestamp) }}</span>  <!-- ← CHANGE createdAt to timestamp -->
                      </div>
                      <p v-if="log.comment" class="text-gray-600">{{ log.comment }}</p>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Load Status Logs Button -->
              <div v-else class="border-t border-gray-200 bg-white p-3">
                <button
                  @click="loadStatusLogs(item.id)"
                  :disabled="loadingLogs.has(item.id)"
                  class="text-sm text-blue-600 hover:text-blue-700 font-medium disabled:opacity-50"
                >
                  {{ loadingLogs.has(item.id) ? 'Loading...' : 'View Status Updates' }}
                </button>
              </div>
            </div>
          </div>
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

    <!-- Update Status Modal (Runner) -->
    <Modal :show="showStatusModal" @close="showStatusModal = false">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Update Status</h3>
      </template>
      <template #body>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Status
            </label>
            <select
              v-model="newStatus"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="Pending">Pending</option>
              <option value="InProgress">In Progress</option>
              <option value="Completed">Completed</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Comment (optional)
            </label>
            <textarea
              v-model="newComment"
              rows="3"
              placeholder="Add a note about this status change..."
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            ></textarea>
          </div>

          <div class="flex gap-3 pt-4">
            <button
              @click="showStatusModal = false"
              class="flex-1 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition duration-200"
            >
              Cancel
            </button>
            <button
              @click="handleUpdateStatus"
              class="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200"
            >
              Update Status
            </button>
          </div>
        </div>
      </template>
    </Modal>

    <!-- Edit Item Modal (Client/Admin) -->
    <Modal :show="showEditItemModal" @close="showEditItemModal = false">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Edit Task Item</h3>
      </template>
      <template #body>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Description
            </label>
            <input
              v-model="itemDescription"
              type="text"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Enter task item description"
            />
          </div>

          <div class="flex gap-3 pt-4">
            <button
              @click="showEditItemModal = false"
              class="flex-1 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition duration-200"
            >
              Cancel
            </button>
            <button
              @click="handleEditItem"
              class="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200"
            >
              Save Changes
            </button>
          </div>
        </div>
      </template>
    </Modal>

    <!-- Delete Item Modal -->
    <Modal :show="showDeleteItemModal" @close="showDeleteItemModal = false">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Delete Task Item</h3>
      </template>
      <template #body>
        <div class="space-y-4">
          <p class="text-gray-600">
            Are you sure you want to delete this task item? This action cannot be undone.
          </p>

          <div class="flex gap-3 pt-4">
            <button
              @click="showDeleteItemModal = false"
              class="flex-1 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition duration-200"
            >
              Cancel
            </button>
            <button
              @click="handleDeleteItem"
              class="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition duration-200"
            >
              Delete
            </button>
          </div>
        </div>
      </template>
    </Modal>
  </div>
</template>