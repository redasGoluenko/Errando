<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useNotificationStore } from '@/stores/notificationStore'

const router = useRouter()
const notificationStore = useNotificationStore()

const hasUnread = computed(() => notificationStore.unreadCount > 0)

const goToChat = async () => {
  await notificationStore.markAllAsRead()
  router.push('/chatroom')
}
</script>

<template>
  <div class="relative">
    <button
      @click="goToChat"
      class="relative p-2 text-gray-700 hover:text-blue-600 hover:bg-gray-100 rounded-lg transition duration-200"
      :title="`You have ${notificationStore.unreadCount} unread ${notificationStore.unreadCount === 1 ? 'message' : 'messages'}`"
      aria-label="Messages"
    >
      <!-- Bell Icon -->
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
        />
      </svg>

      <!-- Notification Badge -->
      <span
        v-if="hasUnread"
        class="absolute top-1 right-1 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full animate-pulse"
      >
        {{ notificationStore.unreadCount > 99 ? '99+' : notificationStore.unreadCount }}
      </span>
    </button>
  </div>
</template>

<style scoped>
@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}

.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>
