<script setup lang="ts">
import { ref, watch } from 'vue'
import type { Task, CreateTaskRequest, UpdateTaskRequest } from '@/services/tasksService'
import { authService } from '@/services/api'

interface Props {
  task?: Task
  mode: 'create' | 'edit'
}

const props = defineProps<Props>()

const emit = defineEmits<{
  submit: [data: CreateTaskRequest | UpdateTaskRequest]
  cancel: []
}>()

// Form fields
const title = ref('')
const description = ref('')
const scheduledTime = ref('')
const location = ref('')
const price = ref('')
const isRecurring = ref(false)
const recurringDayOfWeek = ref('')
const recurringRepetitions = ref('')
const hasExpirationDate = ref(false)
const expirationDate = ref('')
const error = ref('')

const currentUserId = authService.getUserId()
const userRole = authService.getRole()

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

// Days of week
const daysOfWeek = [
  { value: '0', label: 'Sunday' },
  { value: '1', label: 'Monday' },
  { value: '2', label: 'Tuesday' },
  { value: '3', label: 'Wednesday' },
  { value: '4', label: 'Thursday' },
  { value: '5', label: 'Friday' },
  { value: '6', label: 'Saturday' }
]

// Pre-fill form if editing
watch(
  () => props.task,
  (newTask) => {
    if (newTask) {
      title.value = newTask.title
      description.value = newTask.description
      // Convert ISO datetime to input format (YYYY-MM-DDTHH:mm)
      scheduledTime.value = newTask.scheduledTime.slice(0, 16)
      location.value = newTask.location || ''
      price.value = newTask.price ? newTask.price.toString() : ''
      isRecurring.value = newTask.isRecurring || false
      recurringDayOfWeek.value = newTask.recurringDayOfWeek ? newTask.recurringDayOfWeek.toString() : ''
      recurringRepetitions.value = newTask.recurringRepetitions ? newTask.recurringRepetitions.toString() : ''
      hasExpirationDate.value = !!newTask.expirationDate
      expirationDate.value = newTask.expirationDate ? newTask.expirationDate.slice(0, 16) : ''
    }
  },
  { immediate: true }
)

function handleSubmit() {
  error.value = ''

  // Validation
  if (!title.value.trim()) {
    error.value = 'Title is required'
    return
  }

  if (!description.value.trim()) {
    error.value = 'Description is required'
    return
  }

  if (!scheduledTime.value) {
    error.value = 'Scheduled time is required'
    return
  }

  // Check if scheduled time is in the future
  const scheduledDate = new Date(scheduledTime.value)
  const now = new Date()
  if (scheduledDate < now) {
    error.value = 'Scheduled time must be in the future'
    return
  }

  // Validate expiration date if set
  if (hasExpirationDate.value && expirationDate.value) {
    const expirationDateObj = new Date(expirationDate.value)
    if (expirationDateObj < scheduledDate) {
      error.value = 'Expiration date must be after the scheduled time'
      return
    }
  }

  // Validate periodicity fields
  if (isRecurring.value) {
    if (!recurringDayOfWeek.value) {
      error.value = 'Day of week is required for recurring tasks'
      return
    }
    if (!recurringRepetitions.value || parseInt(recurringRepetitions.value) <= 0) {
      error.value = 'Number of repetitions must be greater than 0'
      return
    }
  }

  // Build request data
  const baseData = {
    title: title.value.trim(),
    description: description.value.trim(),
    scheduledTime: new Date(scheduledTime.value).toISOString(),
    location: location.value.trim() || undefined,
    price: price.value ? parseFloat(price.value) : undefined,
    isRecurring: isRecurring.value,
    recurringDayOfWeek: isRecurring.value ? parseInt(recurringDayOfWeek.value) : undefined,
    recurringRepetitions: isRecurring.value ? parseInt(recurringRepetitions.value) : undefined,
    expirationDate: hasExpirationDate.value && expirationDate.value ? new Date(expirationDate.value).toISOString() : undefined
  }

  if (props.mode === 'create') {
    const createData: CreateTaskRequest = {
      ...baseData,
      clientId: currentUserId!, // Client creating their own task
    }
    console.log('🔵 TaskForm EMIT (create):', createData)
    emit('submit', createData)
  } else {
    const updateData: UpdateTaskRequest = {
      id: props.task!.id,
      ...baseData,
      clientId: props.task!.clientId, // Keep original client
    }
    console.log('🔵 TaskForm EMIT (edit):', updateData)
    emit('submit', updateData)
  }
}

// Get minimum datetime (current time)
const minDateTime = new Date().toISOString().slice(0, 16)
</script>

<template>
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <div v-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
      {{ error }}
    </div>

    <!-- Title -->
    <div>
      <label for="title" class="block text-sm font-medium text-gray-700 mb-1">
        Title <span class="text-red-500">*</span>
      </label>
      <input
        id="title"
        v-model="title"
        type="text"
        placeholder="e.g., Weekly grocery shopping"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        required
      />
    </div>

    <!-- Description -->
    <div>
      <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
        Description <span class="text-red-500">*</span>
      </label>
      <textarea
        id="description"
        v-model="description"
        rows="4"
        placeholder="Describe the task in detail..."
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        required
      ></textarea>
    </div>

    <!-- Scheduled Time -->
    <div>
      <label for="scheduledTime" class="block text-sm font-medium text-gray-700 mb-1">
        Scheduled Time <span class="text-red-500">*</span>
      </label>
      <input
        id="scheduledTime"
        v-model="scheduledTime"
        type="datetime-local"
        :min="minDateTime"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        required
      />
      <p class="text-xs text-gray-500 mt-1">Select when this task should be completed</p>
    </div>

    <!-- Location -->
    <div>
      <label for="location" class="block text-sm font-medium text-gray-700 mb-1">
        Location
      </label>
      <select
        id="location"
        v-model="location"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
      >
        <option value="">Select a city (optional)</option>
        <option v-for="city in lithuanianCities" :key="city" :value="city">
          {{ city }}
        </option>
      </select>
    </div>

    <!-- Price -->
    <div>
      <label for="price" class="block text-sm font-medium text-gray-700 mb-1">
        Price (€)
      </label>
      <input
        id="price"
        v-model="price"
        type="number"
        step="0.01"
        min="0"
        placeholder="e.g., 25.00"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
      />
    </div>

    <!-- Expiration Date -->
    <div class="border-t border-gray-200 pt-4">
      <div class="flex items-center">
        <input
          id="hasExpirationDate"
          v-model="hasExpirationDate"
          type="checkbox"
          class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
        />
        <label for="hasExpirationDate" class="ml-2 text-sm font-medium text-gray-700">
          Set task expiration date
        </label>
      </div>

      <!-- Expiration date input (shown when checkbox is checked) -->
      <div v-if="hasExpirationDate" class="mt-4 bg-blue-50 p-4 rounded-lg">
        <label for="expirationDate" class="block text-sm font-medium text-gray-700 mb-1">
          Expiration Date
        </label>
        <input
          id="expirationDate"
          v-model="expirationDate"
          type="datetime-local"
          :min="scheduledTime"
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
        <p class="text-xs text-gray-500 mt-1">The deadline by which the task should be completed</p>
      </div>
    </div>

    <!-- Periodicity -->
    <div class="border-t border-gray-200 pt-4">
      <div class="flex items-center">
        <input
          id="isRecurring"
          v-model="isRecurring"
          type="checkbox"
          class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
        />
        <label for="isRecurring" class="ml-2 text-sm font-medium text-gray-700">
          Repeat this task weekly
        </label>
      </div>

      <!-- Recurring options (shown when isRecurring is true) -->
      <div v-if="isRecurring" class="mt-4 space-y-4 bg-blue-50 p-4 rounded-lg">
        <!-- Day of Week -->
        <div>
          <label for="dayOfWeek" class="block text-sm font-medium text-gray-700 mb-1">
            Day of Week <span class="text-red-500">*</span>
          </label>
          <select
            id="dayOfWeek"
            v-model="recurringDayOfWeek"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            required
          >
            <option value="">Select a day</option>
            <option v-for="day in daysOfWeek" :key="day.value" :value="day.value">
              {{ day.label }}
            </option>
          </select>
        </div>

        <!-- Repetitions -->
        <div>
          <label for="repetitions" class="block text-sm font-medium text-gray-700 mb-1">
            Number of Repetitions <span class="text-red-500">*</span>
          </label>
          <input
            id="repetitions"
            v-model="recurringRepetitions"
            type="number"
            min="1"
            max="52"
            placeholder="e.g., 4"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            required
          />
          <p class="text-xs text-gray-500 mt-1">Tasks will be created for the specified day of week over the next weeks</p>
        </div>
      </div>
    </div>

    <!-- Buttons -->
    <div class="flex gap-3 pt-4">
      <button
        type="submit"
        class="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition duration-200 font-medium"
      >
        {{ mode === 'create' ? 'Create Task' : 'Update Task' }}
      </button>
      <button
        type="button"
        @click="emit('cancel')"
        class="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition duration-200 font-medium"
      >
        Cancel
      </button>
    </div>
  </form>
</template>