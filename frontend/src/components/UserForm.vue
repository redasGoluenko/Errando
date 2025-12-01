<script setup lang="ts">
import { ref, watch } from 'vue'
import type { User, CreateUserRequest, UpdateUserRequest } from '@/services/userService'

interface Props {
  user?: User
  mode: 'create' | 'edit'
}

const props = defineProps<Props>()

const emit = defineEmits<{
  submit: [data: CreateUserRequest | UpdateUserRequest]
  cancel: []
}>()

const username = ref('')
const email = ref('')
const password = ref('')
const role = ref<'Admin' | 'Client' | 'Runner'>('Client')
const error = ref('')

// Pre-fill form if editing
watch(
  () => props.user,
  (newUser) => {
    if (newUser) {
      username.value = newUser.username
      email.value = newUser.email
      role.value = newUser.role
      password.value = '' // Never pre-fill password
    }
  },
  { immediate: true },
)

function handleSubmit() {
  error.value = ''

  // Validation
  if (!username.value || !email.value || !role.value) {
    error.value = 'Username, email, and role are required'
    return
  }

  if (props.mode === 'create' && !password.value) {
    error.value = 'Password is required for new users'
    return
  }

  if (password.value && password.value.length < 6) {
    error.value = 'Password must be at least 6 characters'
    return
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!emailRegex.test(email.value)) {
    error.value = 'Invalid email format'
    return
  }

  // Build request data
  if (props.mode === 'create') {
    const createData: CreateUserRequest = {
      username: username.value,
      email: email.value,
      password: password.value,
      role: role.value,
    }
    console.log('🔵 UserForm EMIT (create):', createData)
    emit('submit', createData)
  } else {
    const updateData: UpdateUserRequest = {
      username: username.value,
      email: email.value,
      role: role.value,
    }
    // Only include password if user wants to change it
    if (password.value) {
      updateData.password = password.value
    }
    console.log('🔵 UserForm EMIT (edit):', updateData)
    emit('submit', updateData)
  }
}
</script>

<template>
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <!-- Error Alert -->
    <div v-if="error" class="p-3 bg-red-50 border-l-4 border-red-500 rounded">
      <p class="text-red-700 text-sm">{{ error }}</p>
    </div>

    <!-- Username -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">Username</label>
      <input
        v-model="username"
        type="text"
        placeholder="Enter username"
        required
        class="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
      />
    </div>

    <!-- Email -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">Email</label>
      <input
        v-model="email"
        type="email"
        placeholder="Enter email"
        required
        class="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
      />
    </div>

    <!-- Password -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">
        {{ mode === 'create' ? 'Password' : 'New Password (leave empty to keep current)' }}
      </label>
      <input
        v-model="password"
        type="password"
        :placeholder="mode === 'create' ? 'Enter password' : 'Enter new password (optional)'"
        :required="mode === 'create'"
        class="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
      />
    </div>

    <!-- Role -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">Role</label>
      <select
        v-model="role"
        required
        class="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
      >
        <option value="Client">Client</option>
        <option value="Runner">Runner</option>
        <option value="Admin">Admin</option>
      </select>
    </div>

    <!-- Buttons -->
    <div class="flex gap-3 pt-4">
      <button
        type="button"
        @click="emit('cancel')"
        class="flex-1 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition duration-200"
      >
        Cancel
      </button>
      <button
        type="submit"
        class="flex-1 px-4 py-2 bg-gradient-to-r from-purple-600 to-indigo-600 text-white rounded-lg hover:from-purple-700 hover:to-indigo-700 transition duration-200"
      >
        {{ mode === 'create' ? 'Create User' : 'Update User' }}
      </button>
    </div>
  </form>
</template>