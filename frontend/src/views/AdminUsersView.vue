<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { userService, type User, type CreateUserRequest, type UpdateUserRequest } from '@/services/userService'
import Modal from '@/components/Modal.vue'
import UserForm from '@/components/UserForm.vue'
import Toast from '@/components/Toast.vue' // ← ADD

// State
const users = ref<User[]>([])
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
const selectedUser = ref<User | null>(null)

// Fetch users on mount
onMounted(() => {
  fetchUsers()
})

// Helper: Show toast notification
function showNotification(message: string, type: 'success' | 'error' = 'success') {
  toastMessage.value = message
  toastType.value = type
  showToast.value = true
}

async function fetchUsers() {
  loading.value = true
  error.value = ''
  try {
    users.value = await userService.getAllUsers()
  } catch (err: any) {
    console.error('Failed to fetch users:', err)
    error.value = err.response?.data?.message || 'Failed to load users'
  } finally {
    loading.value = false
  }
}

// Create user
async function handleCreateUser(data: CreateUserRequest | UpdateUserRequest) {
  try {
    if ('password' in data && data.password) {
      await userService.createUser(data as CreateUserRequest)
      showCreateModal.value = false
      await fetchUsers()
      showNotification('User created successfully!', 'success') // ← ADD
    } else {
      error.value = 'Password is required for new users'
    }
  } catch (err: any) {
    console.error('Failed to create user:', err)
    error.value = err.response?.data?.message || 'Failed to create user'
    showNotification('Failed to create user', 'error') // ← ADD
  }
}

// Edit user
function openEditModal(user: User) {
  selectedUser.value = user
  showEditModal.value = true
}

// Update user
async function handleUpdateUser(data: CreateUserRequest | UpdateUserRequest) {
  if (!selectedUser.value) return

  try {
    await userService.updateUser(selectedUser.value.id, data as UpdateUserRequest)
    showEditModal.value = false
    selectedUser.value = null
    await fetchUsers()
    showNotification('User updated successfully!', 'success') // ← ADD
  } catch (err: any) {
    console.error('Failed to update user:', err)
    error.value = err.response?.data?.message || 'Failed to update user'
    showNotification('Failed to update user', 'error') // ← ADD
  }
}

// Delete user
function openDeleteModal(user: User) {
  selectedUser.value = user
  showDeleteModal.value = true
}

async function handleDeleteUser() {
  if (!selectedUser.value) return

  try {
    await userService.deleteUser(selectedUser.value.id)
    showDeleteModal.value = false
    const deletedUsername = selectedUser.value.username
    selectedUser.value = null
    await fetchUsers()
    showNotification(`User "${deletedUsername}" deleted successfully!`, 'success') // ← ADD
  } catch (err: any) {
    console.error('Failed to delete user:', err)
    error.value = err.response?.data?.message || 'Failed to delete user'
    showNotification('Failed to delete user', 'error') // ← ADD
  }
}

function closeModals() {
  showCreateModal.value = false
  showEditModal.value = false
  showDeleteModal.value = false
  selectedUser.value = null
}

// Role badge colors
function getRoleBadgeClass(role: string) {
  switch (role) {
    case 'Admin':
      return 'bg-red-100 text-red-700'
    case 'Runner':
      return 'bg-blue-100 text-blue-700'
    case 'Client':
      return 'bg-green-100 text-green-700'
    default:
      return 'bg-gray-100 text-gray-700'
  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Toast Notification -->
    <Toast
      :show="showToast"
      :type="toastType"
      :message="toastMessage"
      @close="showToast = false"
    />

    <!-- Header -->
    <header class="bg-white shadow-md">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-800">Admin Panel</h1>
            <p class="text-sm text-gray-600 mt-1">Manage users and their roles</p>
          </div>
          <router-link
            to="/dashboard"
            class="px-4 py-2 text-gray-600 hover:text-gray-800 transition duration-200"
          >
            ← Back to Dashboard
          </router-link>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Error Alert -->
      <div v-if="error" class="mb-6 p-4 bg-red-50 border-l-4 border-red-500 rounded-lg">
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <svg class="w-5 h-5 text-red-500 mr-3" fill="currentColor" viewBox="0 0 20 20">
              <path
                fill-rule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                clip-rule="evenodd"
              />
            </svg>
            <p class="text-red-700 text-sm font-medium">{{ error }}</p>
          </div>
          <button @click="error = ''" class="text-red-500 hover:text-red-700">
            <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path
                fill-rule="evenodd"
                d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                clip-rule="evenodd"
              />
            </svg>
          </button>
        </div>
      </div>

      <!-- Card -->
      <div class="bg-white rounded-xl shadow-lg overflow-hidden">
        <!-- Card Header -->
        <div class="p-6 border-b flex items-center justify-between">
          <h2 class="text-lg font-semibold text-gray-800">Users ({{ users.length }})</h2>
          <button
            @click="showCreateModal = true"
            class="px-4 py-2 bg-gradient-to-r from-purple-600 to-indigo-600 text-white rounded-lg hover:from-purple-700 hover:to-indigo-700 transition duration-200 flex items-center gap-2"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            Create User
          </button>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="p-12 text-center">
          <svg
            class="animate-spin h-8 w-8 text-purple-600 mx-auto"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path
              class="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
          </svg>
          <p class="text-gray-600 mt-4">Loading users...</p>
        </div>

        <!-- Table -->
        <div v-else-if="users.length > 0" class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50 border-b">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Username</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200">
              <tr v-for="user in users" :key="user.id" class="hover:bg-gray-50 transition duration-150">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ user.id }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ user.username }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{{ user.email }}</td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span :class="['px-3 py-1 text-xs font-medium rounded-full', getRoleBadgeClass(user.role)]">
                    {{ user.role }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <button
                    @click="openEditModal(user)"
                    class="text-indigo-600 hover:text-indigo-900 mr-4 transition duration-200"
                    title="Edit"
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
                    @click="openDeleteModal(user)"
                    class="text-red-600 hover:text-red-900 transition duration-200"
                    title="Delete"
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
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Empty State -->
        <div v-else class="p-12 text-center">
          <svg
            class="w-16 h-16 text-gray-400 mx-auto mb-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
            />
          </svg>
          <p class="text-gray-600 text-lg">No users found</p>
          <p class="text-gray-500 text-sm mt-1">Create your first user to get started</p>
        </div>
      </div>
    </main>

    <!-- Create User Modal -->
    <Modal :show="showCreateModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Create New User</h3>
      </template>
      <template #body>
        <UserForm mode="create" @submit="handleCreateUser" @cancel="closeModals" />
      </template>
    </Modal>

    <!-- Edit User Modal -->
    <Modal :show="showEditModal" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Edit User</h3>
      </template>
      <template #body>
        <UserForm 
          v-if="selectedUser" 
          mode="edit" 
          :user="selectedUser" 
          @submit="handleUpdateUser" 
          @cancel="closeModals" 
        />
      </template>
    </Modal>

    <!-- Delete Confirmation Modal -->
    <Modal :show="showDeleteModal" max-width="sm" @close="closeModals">
      <template #header>
        <h3 class="text-xl font-semibold text-gray-900">Delete User</h3>
      </template>
      <template #body>
        <div class="text-center">
          <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 mb-4">
            <svg class="h-6 w-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
              />
            </svg>
          </div>
          <h3 class="text-lg font-medium text-gray-900 mb-2">Are you sure?</h3>
          <p class="text-sm text-gray-600 mb-6">
            Do you really want to delete user <strong>{{ selectedUser?.username }}</strong>? This action cannot be undone.
          </p>
          <div class="flex gap-3">
            <button
              @click="closeModals"
              class="flex-1 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition duration-200"
            >
              Cancel
            </button>
            <button
              @click="handleDeleteUser"
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

