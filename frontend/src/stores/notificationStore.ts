import { defineStore } from 'pinia'
import { ref } from 'vue'
import ChatService from '../services/ChatService'
import { authService } from '../services/api'

const STORAGE_KEY_PREFIX = 'errando_last_seen_'

export const useNotificationStore = defineStore('notification', () => {
  const unreadCount = ref(0)
  const lastSeenMessageTime = ref<Record<number, string>>({}) // Track the last message time we've seen per chat
  const isChecking = ref(false)

  /**
   * Get storage key for current user
   */
  const getStorageKey = () => {
    const userId = authService.getUserId()
    return `${STORAGE_KEY_PREFIX}${userId}`
  }

  /**
   * Load previously seen message times from localStorage
   */
  const loadFromStorage = () => {
    try {
      const key = getStorageKey()
      const stored = localStorage.getItem(key)
      if (stored) {
        lastSeenMessageTime.value = JSON.parse(stored)
      } else {
        lastSeenMessageTime.value = {}
      }
    } catch (error) {
      console.error('Failed to load notification state from storage:', error)
      lastSeenMessageTime.value = {}
    }
  }

  /**
   * Save current message times to localStorage
   */
  const saveToStorage = () => {
    try {
      const key = getStorageKey()
      localStorage.setItem(key, JSON.stringify(lastSeenMessageTime.value))
    } catch (error) {
      console.error('Failed to save notification state to storage:', error)
    }
  }

  /**
   * Initialize notification tracking for a new session
   * Load previous state from storage and check for new messages
   */
  const initializeForCurrentUser = async () => {
    try {
      // Load previously saved state
      loadFromStorage()
      
      // Now check for any new messages that arrived while logged out
      await checkForUnreadMessages()
    } catch (error) {
      console.error('Failed to initialize notifications:', error)
    }
  }

  /**
   * Check for unread messages across all chats
   * A chat is considered to have unread messages if we haven't seen its last message yet
   */
  const checkForUnreadMessages = async () => {
    if (isChecking.value) return
    
    try {
      isChecking.value = true
      const chats = await ChatService.getChats()
      
      let newUnreadCount = 0
      let hasChanges = false
      
      for (const chat of chats) {
        const lastMessage = chat.messages?.[chat.messages.length - 1]
        if (lastMessage) {
          const previousSeenTime = lastSeenMessageTime.value[chat.id]
          
          // Count as unread if:
          // 1. We haven't seen any messages in this chat yet (no previousSeenTime), OR
          // 2. There's a newer message than we've previously seen
          if (!previousSeenTime || new Date(lastMessage.sentAt) > new Date(previousSeenTime)) {
            newUnreadCount++
            lastSeenMessageTime.value[chat.id] = lastMessage.sentAt
            hasChanges = true
          }
        }
      }
      
      unreadCount.value = newUnreadCount
      
      // Save changes to storage if any
      if (hasChanges) {
        saveToStorage()
      }
    } catch (error) {
      console.error('Failed to check for unread messages:', error)
    } finally {
      isChecking.value = false
    }
  }

  /**
   * Mark all messages as read by updating our last seen time
   */
  const markAllAsRead = async () => {
    try {
      const chats = await ChatService.getChats()
      let hasChanges = false
      for (const chat of chats) {
        const lastMessage = chat.messages?.[chat.messages.length - 1]
        if (lastMessage) {
          if (lastSeenMessageTime.value[chat.id] !== lastMessage.sentAt) {
            lastSeenMessageTime.value[chat.id] = lastMessage.sentAt
            hasChanges = true
          }
        }
      }
      unreadCount.value = 0
      
      if (hasChanges) {
        saveToStorage()
      }
    } catch (error) {
      console.error('Failed to mark messages as read:', error)
    }
  }

  /**
   * Mark messages in a specific chat as read
   */
  const markChatAsRead = async (chatId: number, lastMessageTime: string) => {
    lastSeenMessageTime.value[chatId] = lastMessageTime
    saveToStorage()
    // Recount unread
    await checkForUnreadMessages()
  }

  /**
   * Reset notification state (used on logout)
   */
  const reset = () => {
    unreadCount.value = 0
    lastSeenMessageTime.value = {}
    isChecking.value = false
  }

  return {
    unreadCount,
    lastSeenMessageTime,
    isChecking,
    checkForUnreadMessages,
    initializeForCurrentUser,
    markAllAsRead,
    markChatAsRead,
    reset
  }
})
