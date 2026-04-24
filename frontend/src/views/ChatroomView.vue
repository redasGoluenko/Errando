<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
    <!-- Header -->
    <div class="bg-white shadow">
      <div class="max-w-6xl mx-auto px-4 py-6">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900">Chatroom</h1>
            <p class="text-gray-600 mt-2">Connect with your task partners</p>
          </div>
          <router-link
            to="/dashboard"
            class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg font-semibold transition"
          >
            ← Back to Dashboard
          </router-link>
        </div>
      </div>
    </div>

    <div class="max-w-6xl mx-auto p-4">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4 h-[calc(100vh-200px)] auto-rows-fr">
        <!-- Sidebar - Chat List -->
        <div class="md:col-span-1 bg-white rounded-lg shadow overflow-hidden flex flex-col">
          <ChatList 
            :chats="chats"
            :selectedChatId="selectedChatId"
            :loading="loading"
            @chat-selected="selectChat"
            @chat-created="onChatCreated"
          />
        </div>

        <!-- Main Area - Chat Window -->
        <div class="md:col-span-3">
          <ChatWindow 
            v-if="selectedChat"
            :chat="selectedChat"
            :loading="messageLoading"
            @message-sent="onMessageSent"
          />
          <div v-else class="bg-white rounded-lg shadow h-full flex items-center justify-center">
            <div class="text-center">
              <p class="text-gray-500 text-lg">Select a chat to start messaging</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, onUnmounted } from 'vue';
import ChatList from '../components/ChatList.vue';
import ChatWindow from '../components/ChatWindow.vue';
import ChatService, { type Chat } from '../services/ChatService';
import { useNotificationStore } from '../stores/notificationStore';

const notificationStore = useNotificationStore();

const chats = ref<Chat[]>([]);
const selectedChatId = ref<number | null>(null);
const loading = ref(false);
const messageLoading = ref(false);
const autoRefreshInterval = ref<ReturnType<typeof setInterval> | null>(null);

const selectedChat = computed(() => {
  if (!selectedChatId.value) return null;
  return chats.value.find(c => c.id === selectedChatId.value) || null;
});

/**
 * Load all chats for the current user
 */
const loadChats = async () => {
  try {
    loading.value = true;
    chats.value = await ChatService.getChats();
    
    // Mark all messages as read since user is viewing the chatroom
    await notificationStore.markAllAsRead();
    
    // If a chat is selected, refresh its messages
    if (selectedChatId.value) {
      const chat = chats.value.find(c => c.id === selectedChatId.value);
      if (chat) {
        // The chat should already have the latest messages from the server
      }
    }
  } catch (error) {
    console.error('Failed to load chats:', error);
  } finally {
    loading.value = false;
  }
};

/**
 * Load full chat details (including all messages)
 */
const loadChatDetails = async (chatId: number) => {
  try {
    messageLoading.value = true;
    const chatDetails = await ChatService.getChat(chatId);
    
    // Update the chat in the list
    const index = chats.value.findIndex(c => c.id === chatId);
    if (index !== -1) {
      chats.value[index] = chatDetails;
      
      // Mark this specific chat's messages as read
      const lastMessage = chatDetails.messages?.[chatDetails.messages.length - 1];
      if (lastMessage) {
        await notificationStore.markChatAsRead(chatId, lastMessage.sentAt);
      }
    }
  } catch (error) {
    console.error(`Failed to load chat details for ${chatId}:`, error);
  } finally {
    messageLoading.value = false;
  }
};

/**
 * Handle chat selection
 */
const selectChat = async (chatId: number) => {
  selectedChatId.value = chatId;
  await loadChatDetails(chatId);
};

/**
 * Handle new chat creation
 */
const onChatCreated = async (newChat: Chat) => {
  // Add the new chat to the list if not already there
  if (!chats.value.find(c => c.id === newChat.id)) {
    chats.value.unshift(newChat);
  }
  selectedChatId.value = newChat.id;
};

/**
 * Handle message sent
 */
const onMessageSent = async (message: any) => {
  if (!selectedChatId.value) return;
  
  // Update the chat with the new message
  const chat = chats.value.find(c => c.id === selectedChatId.value);
  if (chat) {
    chat.messages.push(message);
    chat.updatedAt = new Date().toISOString();
  }
  
  // Move chat to top of list
  const index = chats.value.findIndex(c => c.id === selectedChatId.value);
  if (index > 0) {
    const [movedChat] = chats.value.splice(index, 1);
    if (movedChat) {
      chats.value.unshift(movedChat);
    }
  }
};

/**
 * Set up auto-refresh for messages (polling every 3 seconds)
 */
const setupAutoRefresh = () => {
  autoRefreshInterval.value = setInterval(async () => {
    if (selectedChatId.value) {
      await loadChatDetails(selectedChatId.value);
    }
  }, 3000); // Poll every 3 seconds
};

/**
 * Clean up auto-refresh
 */
const cleanupAutoRefresh = () => {
  if (autoRefreshInterval.value) {
    clearInterval(autoRefreshInterval.value);
    autoRefreshInterval.value = null;
  }
};

onMounted(async () => {
  await loadChats();
  setupAutoRefresh();
});

// Clean up interval on unmount
onUnmounted(() => {
  cleanupAutoRefresh();
});
</script>
