<!-- filepath: frontend/src/views/DashboardView.vue -->
<template>
  <div class="min-h-screen bg-gradient-to-br from-purple-50 via-blue-50 to-indigo-50">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">
          Welcome back, {{ username }}! 👋
        </h1>
        <p class="text-gray-600">{{ getDashboardGreeting() }}</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <!-- Admin: Manage Users -->
        <router-link
          v-if="role === 'Admin'"
          to="/admin/users"
          class="block bg-white rounded-lg shadow-md hover:shadow-lg transition p-6"
        >
          <div class="flex items-center gap-4">
            <div class="p-3 bg-red-100 rounded-lg">
              <svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-800">Manage Users</h3>
              <p class="text-sm text-gray-600">View and manage all users</p>
            </div>
          </div>
        </router-link>

        <!-- Client: My Tasks Preview -->
        <div v-if="role === 'Client'" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-800">My Tasks</h3>
            <span class="px-2 py-1 text-sm font-medium bg-blue-100 text-blue-700 rounded">
              {{ tasks.filter(t => !t.isCompleted).length }}
            </span>
          </div>
          
          <div class="space-y-3 mb-4">
            <div v-if="loading" class="text-gray-500 text-sm">Loading tasks...</div>
            <div v-else-if="myTasks.length === 0" class="text-gray-500 text-sm">No active tasks</div>
            <div v-else>
              <div v-for="task in myTasks" :key="task.id" class="p-3 bg-gray-50 rounded-lg border border-gray-200">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ task.title }}</h4>
                <p class="text-xs text-gray-600 mt-1 truncate">{{ task.description }}</p>
              </div>
            </div>
          </div>
          
          <router-link
            to="/tasks"
            class="w-full px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition text-center"
          >
            Show All
          </router-link>
        </div>

        <!-- Tasks Management (Admin) -->
        <router-link
          v-if="role === 'Admin'"
          to="/tasks"
          class="block bg-white rounded-lg shadow-md hover:shadow-lg transition p-6"
        >
          <div class="flex items-center gap-4">
            <div class="p-3 bg-blue-100 rounded-lg">
              <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-800">All Tasks</h3>
              <p class="text-sm text-gray-600">View and manage all tasks</p>
            </div>
          </div>
        </router-link>

        <!-- Complaints (Admin only) -->
        <router-link
          v-if="role === 'Admin'"
          to="/admin/complaints"
          class="block bg-white rounded-lg shadow-md hover:shadow-lg transition p-6"
        >
          <div class="flex items-center gap-4">
            <div class="p-3 bg-orange-100 rounded-lg">
              <svg class="w-8 h-8 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4v.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-800">Complaints</h3>
              <p class="text-sm text-gray-600">Review client complaints</p>
            </div>
          </div>
        </router-link>

        <!-- Client: Completed Tasks Preview -->
        <div v-if="role === 'Client'" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-800">Completed Tasks</h3>
            <span class="px-2 py-1 text-sm font-medium bg-green-100 text-green-700 rounded">
              {{ tasks.filter(t => t.isCompleted).length }}
            </span>
          </div>
          
          <div class="space-y-3 mb-4">
            <div v-if="loading" class="text-gray-500 text-sm">Loading tasks...</div>
            <div v-else-if="completedTasks.length === 0" class="text-gray-500 text-sm">No completed tasks</div>
            <div v-else>
              <div v-for="task in completedTasks" :key="task.id" class="p-3 bg-gray-50 rounded-lg border border-gray-200">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ task.title }}</h4>
                <p class="text-xs text-gray-600 mt-1 truncate">{{ task.description }}</p>
              </div>
            </div>
          </div>
          
          <router-link
            to="/tasks?tab=completed"
            class="w-full px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-lg hover:bg-green-700 transition text-center"
          >
            Show All
          </router-link>
        </div>

        <!-- Runner: My Assigned Tasks -->
        <div v-if="role === 'Runner'" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-800">My Tasks</h3>
            <span class="px-2 py-1 text-sm font-medium bg-blue-100 text-blue-700 rounded">
              {{ allRunnerAssignedTasks.length }}
            </span>
          </div>
          
          <div class="space-y-3 mb-4">
            <div v-if="loading" class="text-gray-500 text-sm">Loading tasks...</div>
            <div v-else-if="runnerAssignedTasks.length === 0" class="text-gray-500 text-sm">No assigned tasks</div>
            <div v-else>
              <div v-for="task in runnerAssignedTasks" :key="task.id" class="p-3 bg-gray-50 rounded-lg border border-gray-200">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ task.title }}</h4>
                <p class="text-xs text-gray-600 mt-1 truncate">{{ task.description }}</p>
              </div>
            </div>
          </div>
          
          <router-link
            to="/runner/tasks?tab=my-tasks"
            class="w-full px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition text-center"
          >
            View All
          </router-link>
        </div>

        <!-- Runner: Available Tasks -->
        <div v-if="role === 'Runner'" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-800">Available Tasks</h3>
            <span class="px-2 py-1 text-sm font-medium bg-gray-100 text-gray-700 rounded">
              {{ allRunnerAvailableTasks.length }}
            </span>
          </div>
          
          <div class="space-y-3 mb-4">
            <div v-if="loading" class="text-gray-500 text-sm">Loading tasks...</div>
            <div v-else-if="runnerAvailableTasks.length === 0" class="text-gray-500 text-sm">All tasks assigned</div>
            <div v-else>
              <div v-for="task in runnerAvailableTasks" :key="task.id" class="p-3 bg-gray-50 rounded-lg border border-gray-200">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ task.title }}</h4>
                <p class="text-xs text-gray-600 mt-1 truncate">{{ task.description }}</p>
              </div>
            </div>
          </div>
          
          <router-link
            to="/runner/tasks?tab=available"
            class="w-full px-4 py-2 bg-gray-600 text-white text-sm font-medium rounded-lg hover:bg-gray-700 transition text-center"
          >
            View All
          </router-link>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '@/services/api'
import { tasksService, type Task } from '@/services/tasksService'

const router = useRouter()

const username = ref(authService.getUsername())
const role = ref(authService.getRole())
const userId = authService.getUserId()
const tasks = ref<Task[]>([])
const loading = ref(false)

// Computed: First 3 non-completed tasks (for clients)
const myTasks = computed(() =>
  tasks.value.filter(t => !t.isCompleted).slice(0, 3)
)

// Computed: First 3 completed tasks (for clients)
const completedTasks = computed(() =>
  tasks.value.filter(t => t.isCompleted).slice(0, 3)
)

// Computed: All runner assigned tasks (for counting)
const allRunnerAssignedTasks = computed(() =>
  tasks.value.filter(t => t.runnerId === userId)
)

// Computed: First 3 runner assigned tasks (for display)
const runnerAssignedTasks = computed(() =>
  allRunnerAssignedTasks.value.slice(0, 3)
)

// Computed: All runner available tasks (for counting)
const allRunnerAvailableTasks = computed(() =>
  tasks.value.filter(t => !t.runnerId)
)

// Computed: First 3 runner available tasks (for display)
const runnerAvailableTasks = computed(() =>
  allRunnerAvailableTasks.value.slice(0, 3)
)

onMounted(() => {
  username.value = authService.getUsername()
  role.value = authService.getRole()
  console.log('Dashboard user:', username.value, role.value)
  
  // Fetch tasks for clients and runners
  if (role.value === 'Client' || role.value === 'Runner') {
    fetchTasks()
  }
})

async function fetchTasks() {
  loading.value = true
  try {
    tasks.value = await tasksService.getAllTasks()
  } catch (err) {
    console.error('Failed to fetch tasks:', err)
  } finally {
    loading.value = false
  }
}

function getDashboardGreeting() {
  const hour = new Date().getHours()
  if (hour < 12) return 'Good morning!'
  if (hour < 18) return 'Good afternoon!'
  return 'Good evening!'
}

function logout() {
  authService.logout()
  router.push('/login')
}
</script>