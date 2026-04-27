<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition">
    <div class="bg-gradient-to-r from-indigo-500 to-indigo-600 px-8 py-6">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-2xl font-bold text-white">Messages</h2>
          <p class="text-indigo-100 text-sm mt-1">Chat with {{ getChatPartnerType() }}</p>
        </div>
        <span class="bg-indigo-700 px-4 py-2 text-white text-lg font-bold rounded-lg">
          {{ chats.length }}
        </span>
      </div>
    </div>

    <div class="p-8">
      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600"></div>
        <p class="text-gray-600 mt-3">Loading messages...</p>
      </div>

      <!-- No Chats State -->
      <div v-else-if="chats.length === 0" class="text-center py-12">
        <svg class="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
        </svg>
        <p class="text-gray-600 text-lg">No messages yet</p>
        <p class="text-gray-500 text-sm mt-1">Start chatting with your task partners</p>
      </div>

      <!-- Chat List -->
      <div v-else class="space-y-3 mb-6">
        <div
          v-for="chat in chats"
          :key="chat.id"
          @click="selectChat(chat)"
          class="p-4 bg-gradient-to-r from-indigo-50 to-transparent rounded-lg border border-indigo-100 hover:border-indigo-300 hover:bg-indigo-100 transition cursor-pointer group"
        >
          <div class="flex items-center justify-between">
            <div class="flex-1 min-w-0">
              <div class="flex items-baseline gap-2">
                <h4 class="text-sm font-semibold text-gray-900 group-hover:text-indigo-600 transition">
                  {{ getOtherUserName(chat) }}
                </h4>
                <span class="text-xs font-medium text-indigo-600 whitespace-nowrap">{{ getOtherUserRole(chat) }}</span>
              </div>
              <p class="text-xs text-gray-600 mt-1 line-clamp-1">{{ getLastMessage(chat) }}</p>
            </div>
            <div class="ml-3 text-indigo-500">→</div>
          </div>
        </div>
      </div>

      <!-- View All Button -->
      <router-link
        to="/chatroom"
        class="w-full px-6 py-3 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition shadow-sm hover:shadow-md text-center block"
      >
        Open Full Chatroom
      </router-link>
    </div>

    <!-- Chat Modal -->
    <ChatModalSingle
      v-if="selectedChat"
      :chat="selectedChat"
      @close="selectedChat = null"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import ChatService, { type Chat } from '../services/ChatService';
import { useAuthStore } from '../stores/authStore';
import ChatModalSingle from './ChatModalSingle.vue';

const authStore = useAuthStore();
const chats = ref<Chat[]>([]);
const selectedChat = ref<Chat | null>(null);
const loading = ref(false);

const getChatPartnerType = (): string => {
  const role = authStore.role;
  if (role === 'Client') return 'Runners';
  if (role === 'Runner') return 'Clients';
  if (role === 'Admin') return 'System Users';
  return 'Users';
};

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

const getLastMessage = (chat: Chat): string => {
  if (!chat.messages || chat.messages.length === 0) {
    return 'No messages';
  }
  const lastMsg = chat.messages[chat.messages.length - 1];
  return lastMsg?.content || 'No messages';
};

const selectChat = (chat: Chat) => {
  selectedChat.value = chat;
};

const loadChats = async () => {
  try {
    loading.value = true;
    chats.value = await ChatService.getChats();
  } catch (error) {
    console.error('Failed to load chats:', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  loadChats();
});
</script>
