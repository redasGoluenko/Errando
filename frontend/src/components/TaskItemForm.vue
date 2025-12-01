<script setup lang="ts">
import { ref, watch } from 'vue'
import type { TaskItem, CreateTaskItemRequest, UpdateTaskItemRequest } from '@/services/taskItemsService'

interface Props {
  taskItem?: TaskItem
  taskId: number
  mode: 'create' | 'edit'
}

const props = defineProps<Props>()

const emit = defineEmits<{
  submit: [data: CreateTaskItemRequest | UpdateTaskItemRequest]
  cancel: []
}>()

const description = ref('')
const isCompleted = ref(false)
const error = ref('')

// Pre-fill form if editing
watch(
  () => props.taskItem,
  (newItem) => {
    if (newItem) {
      description.value = newItem.description
      isCompleted.value = newItem.isCompleted
    }
  },
  { immediate: true }
)

function handleSubmit() {
  error.value = ''

  // Validation
  if (!description.value.trim()) {
    error.value = 'Description is required'
    return
  }

  // Build request data
  if (props.mode === 'create') {
    const createData: CreateTaskItemRequest = {
      description: description.value.trim(),
      isCompleted: isCompleted.value,
      taskId: props.taskId,
    }
    console.log('🔵 TaskItemForm EMIT (create):', createData)
    emit('submit', createData)
  } else {
    const updateData: UpdateTaskItemRequest = {
      id: props.taskItem!.id,
      description: description.value.trim(),
      isCompleted: isCompleted.value,
      taskId: props.taskId,
    }
    console.log('🔵 TaskItemForm EMIT (edit):', updateData)
    emit('submit', updateData)
  }
}
</script>

<template>
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <div v-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
      {{ error }}
    </div>

    <!-- Description -->
    <div>
      <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
        Item Description <span class="text-red-500">*</span>
      </label>
      <textarea
        id="description"
        v-model="description"
        rows="3"
        placeholder="e.g., Buy 2 liters of milk"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        required
      ></textarea>
    </div>

    <!-- Completed Checkbox (only for edit mode) -->
    <div v-if="mode === 'edit'" class="flex items-center">
      <input
        id="isCompleted"
        v-model="isCompleted"
        type="checkbox"
        class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
      />
      <label for="isCompleted" class="ml-2 text-sm text-gray-700">
        Mark as completed
      </label>
    </div>

    <!-- Buttons -->
    <div class="flex gap-3 pt-4">
      <button
        type="submit"
        class="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition duration-200 font-medium"
      >
        {{ mode === 'create' ? 'Add Item' : 'Update Item' }}
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