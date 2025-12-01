<!-- filepath: frontend/src/components/Footer.vue -->
<template>
  <footer class="bg-gray-800 text-gray-300 mt-auto">
    <div class="max-w-7xl mx-auto px-4 py-6">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <!-- Company Info -->
        <div>
          <h3 class="text-white font-semibold text-lg mb-3">Errando</h3>
          <p class="text-sm text-gray-400">
            Professional task management system for clients and runners.
          </p>
        </div>

        <!-- Quick Links -->
        <div>
          <h3 class="text-white font-semibold text-lg mb-3">Quick Links</h3>
          <ul class="space-y-2 text-sm">
            <li>
              <router-link to="/tasks" class="hover:text-white transition">
                Tasks
              </router-link>
            </li>
            <li>
              <router-link to="/runner" class="hover:text-white transition">
                Runner Dashboard
              </router-link>
            </li>
          </ul>
        </div>

        <!-- Contact Info -->
        <div>
          <h3 class="text-white font-semibold text-lg mb-3">Contact</h3>
          <ul class="space-y-2 text-sm text-gray-400">
            <li class="flex items-center gap-2">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
              </svg>
              info@errando.com
            </li>
            <li class="flex items-center gap-2">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
              </svg>
              +370 600 12345
            </li>
          </ul>
        </div>
      </div>

      <!-- Copyright -->
      <div class="border-t border-gray-700 mt-6 pt-6 text-center text-sm text-gray-500">
        <p>&copy; {{ new Date().getFullYear() }} Errando. All rights reserved.</p>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
// Footer component
</script>