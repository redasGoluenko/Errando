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

const title = ref('')
const description = ref('')
const scheduledTime = ref('')
const error = ref('')

const currentUserId = authService.getUserId()
const userRole = authService.getRole()

// Pre-fill form if editing
watch(
  () => props.task,
  (newTask) => {
    if (newTask) {
      title.value = newTask.title
      description.value = newTask.description
      // Convert ISO datetime to input format (YYYY-MM-DDTHH:mm)
      scheduledTime.value = newTask.scheduledTime.slice(0, 16)
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

  // Build request data
  if (props.mode === 'create') {
    const createData: CreateTaskRequest = {
      title: title.value.trim(),
      description: description.value.trim(),
      scheduledTime: new Date(scheduledTime.value).toISOString(),
      clientId: currentUserId!, // Client creating their own task
    }
    console.log('🔵 TaskForm EMIT (create):', createData)
    emit('submit', createData)
  } else {
    const updateData: UpdateTaskRequest = {
      id: props.task!.id,
      title: title.value.trim(),
      description: description.value.trim(),
      scheduledTime: new Date(scheduledTime.value).toISOString(),
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