<script setup lang="ts">
import { ref, watch } from 'vue'
import type { TaskItem } from '@/services/taskItemsService'

interface Props {
  taskId: number
  taskItem?: TaskItem
  isEdit?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isEdit: false,
})

const emit = defineEmits<{
  (e: 'submit', data: any): void
  (e: 'cancel'): void
}>()

const description = ref('')
const isCompleted = ref(false)
const error = ref('')

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

function validate(): boolean {
  error.value = ''
  if (!description.value.trim()) {
    error.value = 'Description is required'
    return false
  }
  return true
}

async function handleSubmit() {
  if (!validate()) {
    return
  }

  // ← FIXED: Don't include 'id' in the object
  if (props.isEdit) {
    // For UPDATE: only send description and isCompleted
    emit('submit', {
      description: description.value,
      isCompleted: isCompleted.value,
    })
  } else {
    // For CREATE: send taskId, description, and isCompleted
    emit('submit', {
      taskId: props.taskId,
      description: description.value,
      isCompleted: isCompleted.value,
    })
  }
}
</script>

<!-- Template stays the same -->
<template>
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <div v-if="error" class="text-red-600 text-sm">{{ error }}</div>

    <div>
      <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
        Description <span class="text-red-500">*</span>
      </label>
      <textarea
        id="description"
        v-model="description"
        rows="3"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
        placeholder="Enter task item description"
        required
      ></textarea>
    </div>

    <div class="flex items-center">
      <input
        id="isCompleted"
        v-model="isCompleted"
        type="checkbox"
        class="w-4 h-4 text-purple-600 border-gray-300 rounded focus:ring-purple-500"
      />
      <label for="isCompleted" class="ml-2 text-sm text-gray-700">
        Mark as completed
      </label>
    </div>

    <div class="flex gap-3">
      <button
        type="submit"
        class="flex-1 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition font-medium"
      >
        {{ isEdit ? 'Update' : 'Create' }}
      </button>
      <button
        type="button"
        @click="emit('cancel')"
        class="flex-1 px-4 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 transition font-medium"
      >
        Cancel
      </button>
    </div>
  </form>
</template>