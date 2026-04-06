import axios from 'axios';

const API_BASE_URL = 'http://localhost:5064/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to all requests
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export interface User {
  id: number;
  username: string;
  email: string;
  role: string;
}

export interface ChatMessage {
  id: number;
  chatId: number;
  senderId: number;
  senderUsername: string;
  content: string;
  sentAt: string;
}

export interface Chat {
  id: number;
  user1Id: number;
  user1: User | null;
  user2Id: number;
  user2: User | null;
  taskId: number | null;
  createdAt: string;
  updatedAt: string;
  messages: ChatMessage[];
}

export class ChatService {
  /**
   * Get all chats for the current user
   */
  static async getChats(): Promise<Chat[]> {
    try {
      const response = await apiClient.get('/chats');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch chats:', error);
      throw error;
    }
  }

  /**
   * Get a specific chat with all its messages
   */
  static async getChat(chatId: number): Promise<Chat> {
    try {
      const response = await apiClient.get(`/chats/${chatId}`);
      return response.data;
    } catch (error) {
      console.error(`Failed to fetch chat ${chatId}:`, error);
      throw error;
    }
  }

  /**
   * Create a new chat or get an existing one with another user
   */
  static async createOrGetChat(otherUserId: number, taskId?: number): Promise<Chat> {
    try {
      const response = await apiClient.post('/chats', {
        otherUserId,
        taskId
      });
      return response.data;
    } catch (error) {
      console.error('Failed to create/get chat:', error);
      throw error;
    }
  }

  /**
   * Send a message in a chat
   */
  static async sendMessage(chatId: number, content: string): Promise<ChatMessage> {
    try {
      const response = await apiClient.post(`/chats/${chatId}/messages`, {
        content
      });
      return response.data;
    } catch (error) {
      console.error(`Failed to send message in chat ${chatId}:`, error);
      throw error;
    }
  }

  /**
   * Get potential chat participants (relevant users based on task assignments)
   * - Clients see runners assigned to their tasks
   * - Runners see clients whose tasks are assigned to them
   */
  static async getChatParticipants(): Promise<User[]> {
    try {
      const response = await apiClient.get('/chats/participants');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch chat participants:', error);
      throw error;
    }
  }
}

export default ChatService;
