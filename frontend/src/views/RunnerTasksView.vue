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

      <h1 class="text-3xl font-bold text-gray-900 mb-2">
        {{ activeTab === 'my-tasks' ? 'My Assigned Tasks' : activeTab === 'available' ? 'Available Tasks' : 'Completed Tasks' }}
      </h1>
      <p class="text-gray-600 mb-8">
        {{ activeTab === 'my-tasks' ? 'View and manage your assigned tasks' : activeTab === 'available' ? 'Find and assign unassigned tasks' : 'View your completed tasks' }}
      </p>

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
      <div v-else>
        <!-- My Assigned Tasks Tab -->
        <section v-if="activeTab === 'my-tasks'">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-2xl font-semibold text-gray-800">My Assigned Tasks</h2>
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
            <p class="mt-2 text-gray-500">Go back to the dashboard and click "Available Tasks" to find tasks to assign</p>
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

              <!-- Task Photo -->
              <div class="mb-4 h-40 bg-gray-100 rounded-md border border-gray-200 overflow-hidden">
                <img
                  v-if="task.photoUrl"
                  :src="`http://localhost:5064${task.photoUrl}`"
                  :alt="task.title"
                  class="w-full h-full object-cover"
                />
              </div>

              <!-- Task Meta Info -->
              <div class="space-y-2 mb-4 text-sm text-gray-600">
                <div class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {{ formatDate(task.scheduledTime) }}
                </div>
                <div v-if="task.location" class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  {{ task.location }}
                </div>
                <div v-if="task.price" class="flex items-center font-medium text-green-600">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  €{{ task.price.toFixed(2) }}
                </div>
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

        <!-- Available Tasks Tab -->
        <section v-if="activeTab === 'available'">
          <div class="flex items-center justify-between mb-6">
            <div>
              <h2 class="text-2xl font-semibold text-gray-800">Available Tasks</h2>
              <p class="text-sm text-gray-600 mt-1">{{ availableTasks.length }} {{ availableTasks.length === 1 ? 'task' : 'tasks' }} available</p>
            </div>
            <span class="px-3 py-1 bg-gray-100 text-gray-800 rounded-full text-sm font-medium">
              {{ availableTasks.length }} tasks
            </span>
          </div>

          <!-- Filters -->
          <div class="bg-white rounded-lg shadow-md p-6 mb-6">
            <h3 class="text-lg font-semibold text-gray-900 mb-4">Filter Tasks</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <!-- Location Filter -->
              <div>
                <label for="filter-location" class="block text-sm font-medium text-gray-700 mb-2">
                  Location
                </label>
                <select
                  id="filter-location"
                  v-model="selectedLocation"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">All Locations</option>
                  <option v-for="city in lithuanianCities" :key="city" :value="city">
                    {{ city }}
                  </option>
                </select>
              </div>

              <!-- Min Price Filter -->
              <div>
                <label for="filter-min-price" class="block text-sm font-medium text-gray-700 mb-2">
                  Min Price (€)
                </label>
                <input
                  id="filter-min-price"
                  v-model="minPrice"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="No minimum"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <!-- Max Price Filter -->
              <div>
                <label for="filter-max-price" class="block text-sm font-medium text-gray-700 mb-2">
                  Max Price (€)
                </label>
                <input
                  id="filter-max-price"
                  v-model="maxPrice"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="No maximum"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>

            <!-- Reset Filters Button -->
            <button
              v-if="selectedLocation || minPrice || maxPrice"
              @click="selectedLocation = ''; minPrice = ''; maxPrice = ''"
              class="mt-4 px-4 py-2 text-sm text-blue-600 hover:text-blue-700 font-medium transition"
            >
              ↺ Reset Filters
            </button>
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

              <!-- Task Photo -->
              <div class="mb-4 h-40 bg-gray-100 rounded-md border border-gray-200 overflow-hidden">
                <img
                  v-if="task.photoUrl"
                  :src="`http://localhost:5064${task.photoUrl}`"
                  :alt="task.title"
                  class="w-full h-full object-cover"
                />
              </div>

              <!-- Task Meta Info -->
              <div class="space-y-2 mb-4 text-sm text-gray-600">
                <div class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {{ formatDate(task.scheduledTime) }}
                </div>
                <div v-if="task.location" class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  {{ task.location }}
                </div>
                <div v-if="task.price" class="flex items-center font-medium text-green-600">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  €{{ task.price.toFixed(2) }}
                </div>
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

        <!-- Completed Tasks Tab -->
        <section v-if="activeTab === 'completed'">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-2xl font-semibold text-gray-800">Completed Tasks</h2>
            <span class="px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm font-medium">
              {{ completedTasks.length }} tasks
            </span>
          </div>

          <!-- Empty State -->
          <div v-if="completedTasks.length === 0" class="bg-white rounded-lg shadow-md p-12 text-center">
            <svg class="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <h3 class="mt-4 text-lg font-medium text-gray-900">No completed tasks</h3>
            <p class="mt-2 text-gray-500">Go back to the dashboard and click "My Tasks" to work on assigned tasks</p>
          </div>

          <!-- Completed Tasks Grid -->
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div
              v-for="task in completedTasks"
              :key="task.id"
              class="bg-white rounded-lg shadow-md hover:shadow-lg transition p-6"
            >
              <div class="flex items-start justify-between mb-3">
                <h3 class="text-lg font-semibold text-gray-900 flex-1">{{ task.title }}</h3>
                <span class="px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">
                  ✓ Completed
                </span>
              </div>

              <p class="text-gray-600 text-sm mb-4 line-clamp-2">{{ task.description }}</p>

              <!-- Task Photo -->
              <div class="mb-4 h-40 bg-gray-100 rounded-md border border-gray-200 overflow-hidden">
                <img
                  v-if="task.photoUrl"
                  :src="`http://localhost:5064${task.photoUrl}`"
                  :alt="task.title"
                  class="w-full h-full object-cover"
                />
              </div>

              <!-- Task Meta Info -->
              <div class="space-y-2 mb-4 text-sm text-gray-600">
                <div class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {{ formatDate(task.scheduledTime) }}
                </div>
                <div v-if="task.location" class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  {{ task.location }}
                </div>
                <div v-if="task.price" class="flex items-center font-medium text-green-600">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  €{{ task.price.toFixed(2) }}
                </div>
              </div>

              <!-- Action Buttons -->
              <div class="flex gap-2">
                <button
                  @click="openReviewModal(task)"
                  class="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition duration-200 font-medium text-sm"
                >
                  Leave Review
                </button>
                <button
                  @click="openComplaintModal(task)"
                  class="flex-1 px-4 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700 transition duration-200 font-medium text-sm"
                >
                  Leave Complaint
                </button>
                <button
                  @click="openDeleteModal(task)"
                  class="p-2 text-red-600 hover:bg-red-50 rounded-lg transition"
                  title="Delete task"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </section>
      </div>
    </main>

    <!-- Leave Complaint Modal -->
    <Modal :show="showComplaintModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Leave a Complaint</h3>
      </template>
      <template #body>
        <div v-if="selectedTask">
          <p class="text-gray-600 mb-4">
            Task: <span class="font-semibold">{{ selectedTask.title }}</span>
          </p>
          <textarea
            v-model="complaintText"
            placeholder="Describe your complaint..."
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
            rows="5"
          />
          <p class="text-sm text-gray-500 mt-2">Maximum 1000 characters</p>
          <div class="flex gap-3 mt-6">
            <button
              @click="handleSubmitComplaint"
              class="flex-1 bg-orange-600 text-white px-4 py-2 rounded-lg hover:bg-orange-700 transition duration-200 font-medium"
            >
              Submit Complaint
            </button>
            <button
              @click="closeModals"
              class="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition duration-200 font-medium"
            >
              Cancel
            </button>
          </div>
        </div>
      </template>
    </Modal>

    <!-- Delete Task Modal -->
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

    <!-- Leave Review Modal -->
    <Modal :show="showReviewModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Leave a Review</h3>
      </template>
      <template #body>
        <ReviewForm
          v-if="selectedTask"
          :task="selectedTask"
          :reviewee-id="selectedTask.clientId"
          :reviewee-username="selectedTask.clientUsername"
          @submit="handleSubmitReview"
          @cancel="closeModals"
        />
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { tasksService, type Task } from '@/services/tasksService'
import { complaintsService, type CreateComplaintRequest } from '@/services/complaintsService'
import { reviewService, type CreateReviewRequest } from '@/services/reviewService'
import { authService } from '@/services/api'
import Toast from '@/components/Toast.vue'
import Modal from '@/components/Modal.vue'
import ReviewForm from '@/components/ReviewForm.vue'

const router = useRouter()
const route = useRoute()

// State
const tasks = ref<Task[]>([])
const loading = ref(false)
const error = ref('')
const activeTab = ref<'my-tasks' | 'available' | 'completed'>((route.query.tab as 'my-tasks' | 'available' | 'completed') || 'my-tasks')

// Toast
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref<'success' | 'error'>('success')

// Modal states
const showComplaintModal = ref(false)
const showDeleteModal = ref(false)
const showReviewModal = ref(false)
const selectedTask = ref<Task | null>(null)
const complaintText = ref('')

// User info
const userId = authService.getUserId()

// Filter state for available tasks
const selectedLocation = ref('')
const minPrice = ref('')
const maxPrice = ref('')

// Lithuanian cities list
const lithuanianCities = [
  'Vilnius',
  'Kaunas',
  'Klaipėda',
  'Šiauliai',
  'Panevėžys',
  'Alytus',
  'Mažeikiai',
  'Jonava',
  'Ukmergė',
  'Telšiai',
  'Visaginas',
  'Plungė',
  'Tauragė',
  'Radviliškis',
  'Gargždai',
  'Marijampolė',
  'Utena',
  'Druskininkai',
  'Anykščiai',
  'Biržai'
]

// Filtered tasks
const availableTasks = computed(() => {
  let filtered = tasks.value.filter(t => !t.runnerId)

  // Filter by location
  if (selectedLocation.value) {
    filtered = filtered.filter(t => t.location === selectedLocation.value)
  }

  // Filter by min price
  if (minPrice.value) {
    const min = parseFloat(minPrice.value)
    filtered = filtered.filter(t => !t.price || t.price >= min)
  }

  // Filter by max price
  if (maxPrice.value) {
    const max = parseFloat(maxPrice.value)
    filtered = filtered.filter(t => !t.price || t.price <= max)
  }

  return filtered
})

const myTasks = computed(() => tasks.value.filter(t => t.runnerId === userId))
const completedTasks = computed(() => tasks.value.filter(t => t.runnerId === userId && t.isCompleted))

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

function openComplaintModal(task: Task) {
  selectedTask.value = task
  complaintText.value = ''
  showComplaintModal.value = true
}

function openDeleteModal(task: Task) {
  selectedTask.value = task
  showDeleteModal.value = true
}

function openReviewModal(task: Task) {
  selectedTask.value = task
  showReviewModal.value = true
}

function closeModals() {
  showComplaintModal.value = false
  showDeleteModal.value = false
  showReviewModal.value = false
  selectedTask.value = null
  complaintText.value = ''
}

async function handleSubmitComplaint() {
  if (!selectedTask.value || !complaintText.value.trim()) {
    showNotification('Please enter a complaint', 'error')
    return
  }

  try {
    const request: CreateComplaintRequest = {
      description: complaintText.value,
      taskId: selectedTask.value.id
    }
    await complaintsService.createComplaint(request)
    closeModals()
    await fetchTasks()
    showNotification('Complaint submitted successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to submit complaint:', err)
    showNotification(err.response?.data?.message || 'Failed to submit complaint', 'error')
  }
}

async function handleDeleteTask() {
  if (!selectedTask.value) return
  
  try {
    await tasksService.deleteTask(selectedTask.value.id)
    closeModals()
    await fetchTasks()
    showNotification('Task deleted successfully!', 'success')
  } catch (err: any) {
    console.error('Failed to delete task:', err)
    showNotification(err.response?.data?.message || 'Failed to delete task', 'error')
  }
}

async function handleSubmitReview(data: CreateReviewRequest) {
  try {
    await reviewService.createReview(data)
    closeModals()
    await fetchTasks()
    showNotification('Review submitted successfully! Rating has been calculated.', 'success')
  } catch (err: any) {
    console.error('Failed to submit review:', err)
    showNotification(err.response?.data?.message || 'Failed to submit review', 'error')
  }
}
</script>