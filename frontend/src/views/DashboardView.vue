<!-- filepath: frontend/src/views/DashboardView.vue -->
<template>
  <div class="min-h-screen bg-gradient-to-br from-purple-50 via-blue-50 to-indigo-50">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <!-- Header Section -->
      <div class="mb-12">
        <h1 class="text-4xl font-bold text-gray-900 mb-2">
          Welcome back, {{ username }}! 👋
        </h1>
        <p class="text-lg text-gray-600">{{ getDashboardGreeting() }}</p>
      </div>

      <!-- Client Dashboard Layout -->
      <div v-if="role === 'Client'" class="space-y-8">
        <!-- Quick Stats Row -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-600 text-sm font-medium">Active Tasks</p>
                <p class="text-4xl font-bold text-blue-600 mt-2">{{ tasks.filter(t => !t.isCompleted).length }}</p>
              </div>
              <div class="bg-blue-100 p-4 rounded-lg">
                <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-600 text-sm font-medium">Completed Tasks</p>
                <p class="text-4xl font-bold text-green-600 mt-2">{{ tasks.filter(t => t.isCompleted).length }}</p>
              </div>
              <div class="bg-green-100 p-4 rounded-lg">
                <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        <!-- Main Content Grid -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <!-- My Tasks Card -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
            <div class="bg-gradient-to-r from-blue-500 to-blue-600 px-8 py-6">
              <div class="flex items-center justify-between">
                <div>
                  <h2 class="text-2xl font-bold text-white">My Tasks</h2>
                  <p class="text-blue-100 text-sm mt-1">Tasks assigned to you</p>
                </div>
                <span class="bg-blue-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
                  {{ tasks.filter(t => !t.isCompleted).length }}
                </span>
              </div>
            </div>

            <div class="p-8">
              <div v-if="loading" class="text-center py-12">
                <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
                <p class="text-gray-600 mt-3">Loading tasks...</p>
              </div>
              <div v-else-if="myTasks.length === 0" class="text-center py-12">
                <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                <p class="text-gray-600 text-lg">No active tasks</p>
                <p class="text-gray-500 text-sm mt-1">Great job! You're all caught up</p>
              </div>
              <div v-else class="space-y-3 mb-6">
                <div v-for="task in myTasks" :key="task.id" class="p-4 bg-gradient-to-r from-blue-50 to-transparent rounded-lg border border-blue-100 hover:border-blue-300 transition cursor-pointer group">
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <h4 class="text-sm font-semibold text-gray-900 group-hover:text-blue-600 transition">{{ task.title }}</h4>
                      <p class="text-xs text-gray-600 mt-1 line-clamp-2">{{ task.description }}</p>
                    </div>
                    <div class="ml-3 text-blue-500">→</div>
                  </div>
                </div>
              </div>

              <router-link
                to="/tasks"
                class="w-full px-6 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition shadow-sm hover:shadow-md text-center block"
              >
                View All Tasks
              </router-link>
            </div>
          </div>

          <!-- Completed Tasks Card -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
            <div class="bg-gradient-to-r from-green-500 to-green-600 px-8 py-6">
              <div class="flex items-center justify-between">
                <div>
                  <h2 class="text-2xl font-bold text-white">Completed Tasks</h2>
                  <p class="text-green-100 text-sm mt-1">Your finished work</p>
                </div>
                <span class="bg-green-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
                  {{ tasks.filter(t => t.isCompleted).length }}
                </span>
              </div>
            </div>

            <div class="p-8">
              <div v-if="loading" class="text-center py-12">
                <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-green-600"></div>
                <p class="text-gray-600 mt-3">Loading tasks...</p>
              </div>
              <div v-else-if="completedTasks.length === 0" class="text-center py-12">
                <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p class="text-gray-600 text-lg">No completed tasks yet</p>
                <p class="text-gray-500 text-sm mt-1">Get started on your tasks to complete them</p>
              </div>
              <div v-else class="space-y-3 mb-6">
                <div v-for="task in completedTasks" :key="task.id" class="p-4 bg-gradient-to-r from-green-50 to-transparent rounded-lg border border-green-100 hover:border-green-300 transition cursor-pointer group">
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <h4 class="text-sm font-semibold text-gray-900 group-hover:text-green-600 transition line-through text-gray-600">{{ task.title }}</h4>
                      <p class="text-xs text-gray-600 mt-1 line-clamp-2">{{ task.description }}</p>
                    </div>
                    <div class="ml-3 text-green-500">✓</div>
                  </div>
                </div>
              </div>

              <router-link
                to="/tasks?tab=completed"
                class="w-full px-6 py-3 bg-green-600 text-white font-semibold rounded-lg hover:bg-green-700 transition shadow-sm hover:shadow-md text-center block"
              >
                View All Completed
              </router-link>
            </div>
          </div>
        </div>
      </div>

      <!-- Runner Dashboard Layout -->
      <div v-if="role === 'Runner'" class="space-y-8">
        <!-- Quick Stats Row -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-600 text-sm font-medium">My Tasks</p>
                <p class="text-4xl font-bold text-blue-600 mt-2">{{ allRunnerAssignedTasks.length }}</p>
              </div>
              <div class="bg-blue-100 p-4 rounded-lg">
                <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-600 text-sm font-medium">Available Tasks</p>
                <p class="text-4xl font-bold text-amber-600 mt-2">{{ allRunnerAvailableTasks.length }}</p>
              </div>
              <div class="bg-amber-100 p-4 rounded-lg">
                <svg class="w-8 h-8 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-600 text-sm font-medium">Completed Tasks</p>
                <p class="text-4xl font-bold text-green-600 mt-2">{{ allRunnerCompletedTasks.length }}</p>
              </div>
              <div class="bg-green-100 p-4 rounded-lg">
                <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        <!-- Main Content Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <!-- My Assigned Tasks Card -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
            <div class="bg-gradient-to-r from-blue-500 to-blue-600 px-8 py-6">
              <div class="flex items-center justify-between">
                <div>
                  <h2 class="text-2xl font-bold text-white">My Tasks</h2>
                  <p class="text-blue-100 text-sm mt-1">Tasks assigned to you</p>
                </div>
                <span class="bg-blue-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
                  {{ allRunnerAssignedTasks.length }}
                </span>
              </div>
            </div>

            <div class="p-8">
              <div v-if="loading" class="text-center py-12">
                <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
                <p class="text-gray-600 mt-3">Loading tasks...</p>
              </div>
              <div v-else-if="runnerAssignedTasks.length === 0" class="text-center py-12">
                <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                <p class="text-gray-600 text-lg">No assigned tasks</p>
                <p class="text-gray-500 text-sm mt-1">Check available tasks to take on new work</p>
              </div>
              <div v-else class="space-y-3 mb-6">
                <div v-for="task in runnerAssignedTasks" :key="task.id" class="p-4 bg-gradient-to-r from-blue-50 to-transparent rounded-lg border border-blue-100 hover:border-blue-300 transition cursor-pointer group">
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <h4 class="text-sm font-semibold text-gray-900 group-hover:text-blue-600 transition">{{ task.title }}</h4>
                      <p class="text-xs text-gray-600 mt-1 line-clamp-2">{{ task.description }}</p>
                    </div>
                    <div class="ml-3 text-blue-500">→</div>
                  </div>
                </div>
              </div>

              <router-link
                to="/runner/tasks?tab=my-tasks"
                class="w-full px-6 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition shadow-sm hover:shadow-md text-center block"
              >
                View All Tasks
              </router-link>
            </div>
          </div>

          <!-- Available & Completed Tasks -->
          <div class="space-y-8">
            <!-- Available Tasks Card -->
            <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
              <div class="bg-gradient-to-r from-amber-500 to-amber-600 px-8 py-6">
                <div class="flex items-center justify-between">
                  <div>
                    <h2 class="text-2xl font-bold text-white">Available Tasks</h2>
                    <p class="text-amber-100 text-sm mt-1">Find new work</p>
                  </div>
                  <span class="bg-amber-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
                    {{ allRunnerAvailableTasks.length }}
                  </span>
                </div>
              </div>

              <div class="p-8">
                <div v-if="loading" class="text-center py-12">
                  <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-amber-600"></div>
                  <p class="text-gray-600 mt-3">Loading tasks...</p>
                </div>
                <div v-else-if="runnerAvailableTasks.length === 0" class="text-center py-12">
                  <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                  </svg>
                  <p class="text-gray-600 text-lg">All tasks assigned</p>
                  <p class="text-gray-500 text-sm mt-1">Great job! Check back later for more tasks</p>
                </div>
                <div v-else class="space-y-3 mb-6">
                  <div v-for="task in runnerAvailableTasks" :key="task.id" class="p-4 bg-gradient-to-r from-amber-50 to-transparent rounded-lg border border-amber-100 hover:border-amber-300 transition cursor-pointer group">
                    <div class="flex items-start justify-between">
                      <div class="flex-1">
                        <h4 class="text-sm font-semibold text-gray-900 group-hover:text-amber-600 transition">{{ task.title }}</h4>
                        <p class="text-xs text-gray-600 mt-1 line-clamp-2">{{ task.description }}</p>
                      </div>
                      <div class="ml-3 text-amber-500">→</div>
                    </div>
                  </div>
                </div>

                <router-link
                  to="/runner/tasks?tab=available"
                  class="w-full px-6 py-3 bg-amber-600 text-white font-semibold rounded-lg hover:bg-amber-700 transition shadow-sm hover:shadow-md text-center block"
                >
                  View All Available
                </router-link>
              </div>
            </div>

            <!-- Completed Tasks Card -->
            <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
              <div class="bg-gradient-to-r from-green-500 to-green-600 px-8 py-6">
                <div class="flex items-center justify-between">
                  <div>
                    <h2 class="text-2xl font-bold text-white">Completed Tasks</h2>
                    <p class="text-green-100 text-sm mt-1">Your finished work</p>
                  </div>
                  <span class="bg-green-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
                    {{ allRunnerCompletedTasks.length }}
                  </span>
                </div>
              </div>

              <div class="p-8">
                <div v-if="loading" class="text-center py-12">
                  <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-green-600"></div>
                  <p class="text-gray-600 mt-3">Loading tasks...</p>
                </div>
                <div v-else-if="runnerCompletedTasks.length === 0" class="text-center py-12">
                  <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <p class="text-gray-600 text-lg">No completed tasks yet</p>
                  <p class="text-gray-500 text-sm mt-1">Complete assigned tasks to see them here</p>
                </div>
                <div v-else class="space-y-3 mb-6">
                  <div v-for="task in runnerCompletedTasks" :key="task.id" class="p-4 bg-gradient-to-r from-green-50 to-transparent rounded-lg border border-green-100 hover:border-green-300 transition cursor-pointer group">
                    <div class="flex items-start justify-between">
                      <div class="flex-1">
                        <h4 class="text-sm font-semibold text-gray-900 group-hover:text-green-600 transition line-through text-gray-600">{{ task.title }}</h4>
                        <p class="text-xs text-gray-600 mt-1 line-clamp-2">{{ task.description }}</p>
                      </div>
                      <div class="ml-3 text-green-500">✓</div>
                    </div>
                  </div>
                </div>

                <router-link
                  to="/runner/tasks?tab=completed"
                  class="w-full px-6 py-3 bg-green-600 text-white font-semibold rounded-lg hover:bg-green-700 transition shadow-sm hover:shadow-md text-center block"
                >
                  View All Completed
                </router-link>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Admin Dashboard Layout -->
      <div v-else-if="role === 'Admin'" class="space-y-8">
        <!-- Quick Stats Row -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-600 text-sm font-medium">Total Users</p>
                <p class="text-4xl font-bold text-red-600 mt-2">{{ allUsers.length }}</p>
              </div>
              <div class="bg-red-100 p-4 rounded-lg">
                <svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 12H9m4 8H9m6-4H9m4 4H9m6 0a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-600 text-sm font-medium">Total Tasks</p>
                <p class="text-4xl font-bold text-blue-600 mt-2">{{ tasks.length }}</p>
              </div>
              <div class="bg-blue-100 p-4 rounded-lg">
                <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-600 text-sm font-medium">Total Complaints</p>
                <p class="text-4xl font-bold text-orange-600 mt-2">{{ allComplaints.length }}</p>
              </div>
              <div class="bg-orange-100 p-4 rounded-lg">
                <svg class="w-8 h-8 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4v.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        <!-- Main Content Grid -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <!-- Users Card -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
            <div class="bg-gradient-to-r from-red-500 to-red-600 px-8 py-6">
              <div class="flex items-center justify-between">
                <div>
                  <h2 class="text-2xl font-bold text-white">Users</h2>
                  <p class="text-red-100 text-sm mt-1">Manage all users</p>
                </div>
                <span class="bg-red-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
                  {{ allUsers.length }}
                </span>
              </div>
            </div>

            <div class="p-8">
              <div v-if="loading" class="text-center py-12">
                <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-red-600"></div>
                <p class="text-gray-600 mt-3">Loading users...</p>
              </div>
              <div v-else-if="adminUsers.length === 0" class="text-center py-12">
                <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 12H9m4 8H9m6-4H9m4 4H9m6 0a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p class="text-gray-600 text-lg">No users</p>
                <p class="text-gray-500 text-sm mt-1">Create users to get started</p>
              </div>
              <div v-else class="space-y-3 mb-6">
                <div v-for="user in adminUsers" :key="user.id" class="p-4 bg-gradient-to-r from-red-50 to-transparent rounded-lg border border-red-100 hover:border-red-300 transition cursor-pointer group">
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <h4 class="text-sm font-semibold text-gray-900 group-hover:text-red-600 transition">{{ user.username }}</h4>
                      <p class="text-xs text-gray-600 mt-1 line-clamp-2">{{ user.email }}</p>
                    </div>
                    <div class="ml-3 text-red-500">→</div>
                  </div>
                </div>
              </div>

              <router-link
                to="/admin/users"
                class="w-full px-6 py-3 bg-red-600 text-white font-semibold rounded-lg hover:bg-red-700 transition shadow-sm hover:shadow-md text-center block"
              >
                View All Users
              </router-link>
            </div>
          </div>

          <!-- Tasks Card -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
            <div class="bg-gradient-to-r from-blue-500 to-blue-600 px-8 py-6">
              <div class="flex items-center justify-between">
                <div>
                  <h2 class="text-2xl font-bold text-white">Tasks</h2>
                  <p class="text-blue-100 text-sm mt-1">Manage all tasks</p>
                </div>
                <span class="bg-blue-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
                  {{ tasks.length }}
                </span>
              </div>
            </div>

            <div class="p-8">
              <div v-if="loading" class="text-center py-12">
                <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
                <p class="text-gray-600 mt-3">Loading tasks...</p>
              </div>
              <div v-else-if="adminTasks.length === 0" class="text-center py-12">
                <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                <p class="text-gray-600 text-lg">No tasks</p>
                <p class="text-gray-500 text-sm mt-1">Create tasks to get started</p>
              </div>
              <div v-else class="space-y-3 mb-6">
                <div v-for="task in adminTasks" :key="task.id" class="p-4 bg-gradient-to-r from-blue-50 to-transparent rounded-lg border border-blue-100 hover:border-blue-300 transition cursor-pointer group">
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <h4 class="text-sm font-semibold text-gray-900 group-hover:text-blue-600 transition">{{ task.title }}</h4>
                      <p class="text-xs text-gray-600 mt-1 line-clamp-2">{{ task.description }}</p>
                    </div>
                    <div class="ml-3 text-blue-500">→</div>
                  </div>
                </div>
              </div>

              <router-link
                to="/tasks"
                class="w-full px-6 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition shadow-sm hover:shadow-md text-center block"
              >
                View All Tasks
              </router-link>
            </div>
          </div>

          <!-- Complaints Card -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
            <div class="bg-gradient-to-r from-orange-500 to-orange-600 px-8 py-6">
              <div class="flex items-center justify-between">
                <div>
                  <h2 class="text-2xl font-bold text-white">Complaints</h2>
                  <p class="text-orange-100 text-sm mt-1">Review complaints</p>
                </div>
                <span class="bg-orange-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
                  {{ allComplaints.length }}
                </span>
              </div>
            </div>

            <div class="p-8">
              <div v-if="loading" class="text-center py-12">
                <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-orange-600"></div>
                <p class="text-gray-600 mt-3">Loading complaints...</p>
              </div>
              <div v-else-if="adminComplaints.length === 0" class="text-center py-12">
                <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4v.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p class="text-gray-600 text-lg">No complaints</p>
                <p class="text-gray-500 text-sm mt-1">Great! No complaints yet</p>
              </div>
              <div v-else class="space-y-3 mb-6">
                <div v-for="complaint in adminComplaints" :key="complaint.id" class="p-4 bg-gradient-to-r from-orange-50 to-transparent rounded-lg border border-orange-100 hover:border-orange-300 transition cursor-pointer group">
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <h4 class="text-sm font-semibold text-gray-900 group-hover:text-orange-600 transition">{{ complaint.taskTitle }}</h4>
                      <p class="text-xs text-gray-600 mt-1 line-clamp-2">{{ complaint.clientUsername }} - {{ complaint.runnerUsername }}</p>
                    </div>
                    <div class="ml-3 text-orange-500">→</div>
                  </div>
                </div>
              </div>

              <router-link
                to="/admin/complaints"
                class="w-full px-6 py-3 bg-orange-600 text-white font-semibold rounded-lg hover:bg-orange-700 transition shadow-sm hover:shadow-md text-center block"
              >
                View All Complaints
              </router-link>
            </div>
          </div>
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

// Computed: First 6 runner assigned tasks (for display)
const runnerAssignedTasks = computed(() =>
  allRunnerAssignedTasks.value.slice(0, 6)
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
    const tasksData = await tasksService.getAllTasks()
    tasks.value = tasksData
  } catch (err) {
    console.error('Failed to fetch tasks:', err)
  }

  try {
    const usersData = await userService.getAllUsers()
    users.value = usersData
  } catch (err) {
    console.error('Failed to fetch users:', err)
  }

  try {
    const complaintsData = await complaintsService.getAllComplaints()
    complaints.value = complaintsData
  } catch (err) {
    console.error('Failed to fetch complaints:', err)
  }

  loading.value = false
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