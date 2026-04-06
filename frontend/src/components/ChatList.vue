<template>
  <div class="flex flex-col h-full">
    <!-- Header -->
    <div class="bg-indigo-600 text-white p-4">
      <h2 class="text-xl font-semibold">Messages</h2>
      <p class="text-sm text-indigo-100 mt-1">{{ chats.length }} conversation(s)</p>
    </div>

    <!-- New Chat Button -->
    <div class="p-3 border-b">
      <button
        @click="showNewChatModal = true"
        class="w-full bg-indigo-500 hover:bg-indigo-600 text-white font-semibold py-2 px-4 rounded transition"
      >
        + New Chat
      </button>
    </div>

    <!-- Chat List -->
    <div v-if="!loading" class="flex-1 overflow-y-auto">
      <div v-if="chats.length === 0" class="p-4 text-center text-gray-500">
        <p>No chats yet</p>
        <p class="text-sm">Click "New Chat" to start a conversation</p>
      </div>

      <div v-else>
        <div
          v-for="chat in chats"
          :key="chat.id"
          @click="$emit('chat-selected', chat.id)"
          class="p-4 border-b hover:bg-gray-50 cursor-pointer transition"
          :class="{ 'bg-indigo-50 border-l-4 border-indigo-600': selectedChatId === chat.id }"
        >
          <div class="flex items-center justify-between mb-2">
            <div class="flex items-baseline gap-2 flex-1">
              <h3 class="font-semibold text-gray-900">
                {{ getOtherUserName(chat) }}
              </h3>
              <span class="text-xs font-medium text-indigo-600">{{ getOtherUserRole(chat) }}</span>
            </div>
            <span class="text-xs text-gray-500 ml-2 whitespace-nowrap">
              {{ formatTime(chat.updatedAt) }}
            </span>
          </div>
          <p class="text-sm text-gray-600 truncate">
            {{ getLastMessage(chat) }}
          </p>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-else class="flex-1 flex items-center justify-center">
      <div class="text-gray-500">
        <p>Loading chats...</p>
      </div>
    </div>

    <!-- New Chat Modal -->
    <NewChatModal
      v-if="showNewChatModal"
      @close="showNewChatModal = false"
      @chat-created="handleChatCreated"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import NewChatModal from '../components/NewChatModal.vue';
import ChatService, { type Chat } from '../services/ChatService';
import { useAuthStore } from '../stores/authStore';

const props = defineProps<{
  chats: Chat[];
  selectedChatId: number | null;
  loading: boolean;
}>();

const emit = defineEmits<{
  'chat-selected': [chatId: number];
  'chat-created': [chat: Chat];
}>();

const authStore = useAuthStore();
const showNewChatModal = ref(false);

/**
 * Get the other user's name in the chat
 */
const getOtherUserName = (chat: Chat): string => {
  if (!chat) return 'Unknown';
  
  // Get current user ID from localStorage (most reliable)
  const currentIdStr = localStorage.getItem('userId');
  const currentId = currentIdStr ? Number(currentIdStr) : null;
  
  if (currentId === null) return 'Unknown';
  
  const user1Id = Number(chat.user1Id);
  const user2Id = Number(chat.user2Id);
  
  if (user1Id === currentId && chat.user2?.username) {
    return chat.user2.username;
  } else if (user2Id === currentId && chat.user1?.username) {
    return chat.user1.username;
  }
  
  return 'Unknown';
};

const getOtherUserRole = (chat: Chat): string => {
  if (!chat) return 'Unknown';
  
  // Get current user ID from localStorage (most reliable)
  const currentIdStr = localStorage.getItem('userId');
  const currentId = currentIdStr ? Number(currentIdStr) : null;
  
  if (currentId === null) return 'Unknown';
  
  const user1Id = Number(chat.user1Id);
  const user2Id = Number(chat.user2Id);
  
  if (user1Id === currentId && chat.user2?.role) {
    return chat.user2.role;
  } else if (user2Id === currentId && chat.user1?.role) {
    return chat.user1.role;
  }
  
  return 'Unknown';
};

/**
 * Get the last message preview
 */
const getLastMessage = (chat: Chat): string => {
  if (!chat.messages || chat.messages.length === 0) {
    return 'No messages';
  }
  
  const lastMsg = chat.messages[chat.messages.length - 1];
  if (!lastMsg) return 'No messages';
  
  const currentIdStr = localStorage.getItem('userId');
  const currentId = currentIdStr ? Number(currentIdStr) : null;
  const prefix = lastMsg.senderId === currentId ? 'You: ' : '';
  return prefix + lastMsg.content;
};

/**
 * Format time for display
 */
const formatTime = (isoTime: string): string => {
  const date = new Date(isoTime);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 1) return 'now';
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;

  return date.toLocaleDateString();
};

/**
 * Handle new chat creation
 */
const handleChatCreated = (newChat: Chat) => {
  emit('chat-created', newChat);
  showNewChatModal.value = false;
};
</script>
