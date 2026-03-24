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
        <div v-if="role === 'Admin'" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-800">All Users</h3>
            <span class="px-2 py-1 text-sm font-medium bg-red-100 text-red-700 rounded">
              {{ allUsers.length }}
            </span>
          </div>
          
          <div class="space-y-3 mb-4">
            <div v-if="loading" class="text-gray-500 text-sm">Loading users...</div>
            <div v-else-if="adminUsers.length === 0" class="text-gray-500 text-sm">No users</div>
            <div v-else>
              <div v-for="user in adminUsers" :key="user.id" class="p-3 bg-gray-50 rounded-lg border border-gray-200">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ user.username }}</h4>
                <p class="text-xs text-gray-600 mt-1 truncate">{{ user.email }}</p>
              </div>
            </div>
          </div>
          
          <router-link
            to="/admin/users"
            class="w-full px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition text-center"
          >
            Show All
          </router-link>
        </div>

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

        <!-- Admin: All Tasks -->
        <div v-if="role === 'Admin'" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-800">All Tasks</h3>
            <span class="px-2 py-1 text-sm font-medium bg-blue-100 text-blue-700 rounded">
              {{ tasks.length }}
            </span>
          </div>
          
          <div class="space-y-3 mb-4">
            <div v-if="loading" class="text-gray-500 text-sm">Loading tasks...</div>
            <div v-else-if="adminTasks.length === 0" class="text-gray-500 text-sm">No tasks</div>
            <div v-else>
              <div v-for="task in adminTasks" :key="task.id" class="p-3 bg-gray-50 rounded-lg border border-gray-200">
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

        <!-- Admin: Complaints -->
        <div v-if="role === 'Admin'" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-800">Complaints</h3>
            <span class="px-2 py-1 text-sm font-medium bg-orange-100 text-orange-700 rounded">
              {{ allComplaints.length }}
            </span>
          </div>
          
          <div class="space-y-3 mb-4">
            <div v-if="loading" class="text-gray-500 text-sm">Loading complaints...</div>
            <div v-else-if="adminComplaints.length === 0" class="text-gray-500 text-sm">No complaints</div>
            <div v-else>
              <div v-for="complaint in adminComplaints" :key="complaint.id" class="p-3 bg-gray-50 rounded-lg border border-gray-200">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ complaint.taskTitle }}</h4>
                <p class="text-xs text-gray-600 mt-1 truncate">{{ complaint.clientUsername }} - {{ complaint.runnerUsername }}</p>
              </div>
            </div>
          </div>
          
          <router-link
            to="/admin/complaints"
            class="w-full px-4 py-2 bg-orange-600 text-white text-sm font-medium rounded-lg hover:bg-orange-700 transition text-center"
          >
            Show All
          </router-link>
        </div>

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

        <!-- Runner: Completed Tasks -->
        <div v-if="role === 'Runner'" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-800">Completed Tasks</h3>
            <span class="px-2 py-1 text-sm font-medium bg-green-100 text-green-700 rounded">
              {{ allRunnerCompletedTasks.length }}
            </span>
          </div>
          
          <div class="space-y-3 mb-4">
            <div v-if="loading" class="text-gray-500 text-sm">Loading tasks...</div>
            <div v-else-if="runnerCompletedTasks.length === 0" class="text-gray-500 text-sm">No completed tasks</div>
            <div v-else>
              <div v-for="task in runnerCompletedTasks" :key="task.id" class="p-3 bg-gray-50 rounded-lg border border-gray-200">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ task.title }}</h4>
                <p class="text-xs text-gray-600 mt-1 truncate">{{ task.description }}</p>
              </div>
            </div>
          </div>
          
          <router-link
            to="/runner/tasks?tab=completed"
            class="w-full px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-lg hover:bg-green-700 transition text-center"
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
import { userService, type User } from '@/services/userService'
import { complaintsService, type Complaint } from '@/services/complaintsService'

const router = useRouter()

const username = ref(authService.getUsername())
const role = ref(authService.getRole())
const userId = authService.getUserId()
const tasks = ref<Task[]>([])
const users = ref<User[]>([])
const complaints = ref<Complaint[]>([])
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

// Computed: All runner completed tasks (for counting)
const allRunnerCompletedTasks = computed(() =>
  tasks.value.filter(t => t.runnerId === userId && t.isCompleted)
)

// Computed: First 3 runner completed tasks (for display)
const runnerCompletedTasks = computed(() =>
  allRunnerCompletedTasks.value.slice(0, 3)
)

// Computed: All users (for counting)
const allUsers = computed(() =>
  users.value
)

// Computed: First 3 users (for display)
const adminUsers = computed(() =>
  allUsers.value.slice(0, 3)
)

// Computed: First 3 tasks (for display in admin)
const adminTasks = computed(() =>
  tasks.value.slice(0, 3)
)

// Computed: All complaints (for counting)
const allComplaints = computed(() =>
  complaints.value
)

// Computed: First 3 complaints (for display)
const adminComplaints = computed(() =>
  allComplaints.value.slice(0, 3)
)

onMounted(() => {
  username.value = authService.getUsername()
  role.value = authService.getRole()
  console.log('Dashboard user:', username.value, role.value)
  
  // Fetch tasks for clients and runners
  if (role.value === 'Client' || role.value === 'Runner') {
    fetchTasks()
  }
  
  // Fetch admin data
  if (role.value === 'Admin') {
    fetchAdminData()
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

async function fetchAdminData() {
  loading.value = true
  try {
    const [tasksData, usersData, complaintsData] = await Promise.all([
      tasksService.getAllTasks(),
      userService.getAllUsers(),
      complaintsService.getAllComplaints()
    ])
    tasks.value = tasksData
    users.value = usersData
    complaints.value = complaintsData
  } catch (err) {
    console.error('Failed to fetch admin data:', err)
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