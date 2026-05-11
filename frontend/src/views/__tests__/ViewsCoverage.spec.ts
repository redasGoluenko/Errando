import { mount, flushPromises } from '@vue/test-utils'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

const mocks = vi.hoisted(() => ({
  route: { query: {}, params: { id: '1' } },
  router: { push: vi.fn() },
  auth: {
    getRole: vi.fn(() => 'Client'),
    getUserId: vi.fn(() => 1),
    getUsername: vi.fn(() => 'client'),
    isAuthenticated: vi.fn(() => true),
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
  },
  tasks: {
    getAllTasks: vi.fn(),
    getTaskById: vi.fn(),
    createTask: vi.fn(),
    updateTask: vi.fn(),
    deleteTask: vi.fn(),
    assignTask: vi.fn(),
    unassignTask: vi.fn(),
  },
  users: {
    getAllUsers: vi.fn(),
    createUser: vi.fn(),
    updateUser: vi.fn(),
    deleteUser: vi.fn(),
  },
  complaints: {
    getAllComplaints: vi.fn(),
    createComplaint: vi.fn(),
    deleteComplaint: vi.fn(),
    resolveComplaint: vi.fn(),
  },
  reviews: {
    createReview: vi.fn(),
    getTaskReview: vi.fn(),
  },
  payments: {
    getAllPayments: vi.fn(),
    getMyPayments: vi.fn(),
    hasPaid: vi.fn(),
    createCheckoutSession: vi.fn(),
  },
  taskItems: {
    getTaskItems: vi.fn(),
    createTaskItem: vi.fn(),
    updateTaskItem: vi.fn(),
    deleteTaskItem: vi.fn(),
    toggleTaskItem: vi.fn(),
  },
  statusLogs: {
    getStatusLogs: vi.fn(),
    createStatusLog: vi.fn(),
    updateStatusLog: vi.fn(),
    deleteStatusLog: vi.fn(),
  },
  chat: {
    getChats: vi.fn(),
    getChat: vi.fn(),
    sendMessage: vi.fn(),
  },
  notificationStore: {
    markAllAsRead: vi.fn(),
    markChatAsRead: vi.fn(),
  },
  getRunnerStats: vi.fn(),
  getClientStats: vi.fn(),
}))

vi.mock('vue-router', () => ({
  useRoute: () => mocks.route,
  useRouter: () => mocks.router,
}))

vi.mock('@/services/api', () => ({
  authService: mocks.auth,
  default: {},
}))

vi.mock('@/services/tasksService', () => ({ tasksService: mocks.tasks }))
vi.mock('@/services/userService', () => ({ userService: mocks.users }))
vi.mock('@/services/complaintsService', () => ({ complaintsService: mocks.complaints }))
vi.mock('@/services/reviewService', () => ({ reviewService: mocks.reviews }))
vi.mock('@/services/paymentService', () => ({ paymentService: mocks.payments }))
vi.mock('@/services/taskItemsService', () => ({ taskItemsService: mocks.taskItems }))
vi.mock('@/services/statusLogsService', () => ({ statusLogsService: mocks.statusLogs }))
vi.mock('@/services/runnerStatsService', () => ({ getRunnerStats: mocks.getRunnerStats }))
vi.mock('@/services/clientStatsService', () => ({ getClientStats: mocks.getClientStats }))
vi.mock('@/services/ChatService', () => ({ default: mocks.chat }))
vi.mock('../../services/ChatService', () => ({ default: mocks.chat }))
vi.mock('../../stores/notificationStore', () => ({
  useNotificationStore: () => mocks.notificationStore,
}))
vi.mock('@stripe/stripe-js', () => ({ loadStripe: vi.fn(() => Promise.resolve(null)) }))

import AdminUsersView from '../AdminUsersView.vue'
import ChatroomView from '../ChatroomView.vue'
import ClientStatsView from '../ClientStatsView.vue'
import ComplaintsView from '../ComplaintsView.vue'
import DashboardView from '../DashboardView.vue'
import PaymentsView from '../PaymentsView.vue'
import RunnerStatsView from '../RunnerStatsView.vue'
import RunnerTasksView from '../RunnerTasksView.vue'
import TaskDetailView from '../TaskDetailView.vue'
import TasksView from '../TasksView.vue'

const now = '2026-05-11T10:00:00Z'

const task = {
  id: 1,
  title: 'Grocery run',
  description: 'Pick up groceries',
  scheduledTime: now,
  status: 'In Progress',
  clientId: 1,
  clientUsername: 'client',
  runnerId: 2,
  runnerUsername: 'runner',
  location: 'Vilnius',
  price: 25,
  photoUrl: null,
  isRecurring: false,
  recurringDayOfWeek: null,
  recurringRepetitions: null,
  expirationDate: null,
  isExpired: false,
  createdAt: now,
  updatedAt: now,
  isCompleted: false,
}

const completedTask = {
  ...task,
  id: 2,
  title: 'Delivered parcel',
  status: 'Completed',
  isCompleted: true,
}

const user = {
  id: 1,
  username: 'client',
  email: 'client@example.com',
  role: 'Client',
  createdAt: now,
}

const runner = {
  id: 2,
  username: 'runner',
  email: 'runner@example.com',
  role: 'Runner',
  createdAt: now,
}

const complaint = {
  id: 1,
  description: 'Item was missing',
  taskId: 1,
  taskTitle: 'Grocery run',
  clientId: 1,
  clientUsername: 'client',
  runnerId: 2,
  runnerUsername: 'runner',
  createdAt: now,
  isResolved: false,
}

const payment = {
  id: 1,
  taskId: 2,
  clientId: 1,
  amount: 25,
  currency: 'eur',
  status: 'succeeded',
  stripePaymentIntentId: 'pi_123',
  createdAt: now,
  updatedAt: now,
}

const stubs = {
  RouterLink: { template: '<a><slot /></a>' },
  Modal: { template: '<section><slot /></section>' },
  Toast: { template: '<div />' },
  TaskForm: { template: '<form />' },
  UserForm: { template: '<form />' },
  ReviewForm: { template: '<form />' },
  PaymentModal: { template: '<div />' },
  TaskItemForm: { template: '<form />' },
  DashboardChatSection: { template: '<div>chat summary</div>' },
  PaymentHistoryCard: { template: '<div>payment history</div>' },
  ChatList: { template: '<aside />' },
  ChatWindow: { template: '<main />' },
}

async function mountView(component: any) {
  const wrapper = mount(component, { global: { stubs } })
  await flushPromises()
  return wrapper
}

describe('view coverage smoke tests', () => {
  beforeEach(() => {
    mocks.route.query = {}
    mocks.route.params = { id: '1' }
    mocks.router.push.mockReset()
    mocks.auth.getRole.mockReturnValue('Client')
    mocks.auth.getUserId.mockReturnValue(1)
    mocks.auth.getUsername.mockReturnValue('client')
    mocks.tasks.getAllTasks.mockResolvedValue([task, completedTask])
    mocks.tasks.getTaskById.mockResolvedValue(task)
    mocks.users.getAllUsers.mockResolvedValue([user, runner])
    mocks.complaints.getAllComplaints.mockResolvedValue([complaint])
    mocks.payments.getAllPayments.mockResolvedValue([payment])
    mocks.payments.getMyPayments.mockResolvedValue([payment])
    mocks.payments.hasPaid.mockResolvedValue(true)
    mocks.taskItems.getTaskItems.mockResolvedValue([
      { id: 1, taskId: 1, description: 'Buy milk', isCompleted: true, createdAt: now, updatedAt: now },
      { id: 2, taskId: 1, description: 'Buy bread', isCompleted: false, createdAt: now, updatedAt: now },
    ])
    mocks.statusLogs.getStatusLogs.mockResolvedValue([
      { id: 1, taskItemId: 1, status: 'Completed', comment: 'Done', createdAt: now, updatedAt: now },
    ])
    mocks.chat.getChats.mockResolvedValue([
      { id: 1, title: 'Task chat', messages: [{ id: 1, content: 'hello', sentAt: now }], updatedAt: now },
    ])
    mocks.chat.getChat.mockResolvedValue({
      id: 1,
      title: 'Task chat',
      messages: [{ id: 1, content: 'hello', sentAt: now }],
      updatedAt: now,
    })
    mocks.getRunnerStats.mockResolvedValue([
      {
        id: 2,
        username: 'runner',
        rating: 4.5,
        totalReviews: 2,
        tasksCompleted: 4,
        activeTasks: 1,
        moneyEarned: 120,
        taskAcceptanceRate: 80,
        totalTasksAssigned: 5,
      },
    ])
    mocks.getClientStats.mockResolvedValue([
      {
        id: 1,
        username: 'client',
        rating: 4,
        totalReviews: 3,
        tasksCreated: 3,
        tasksCompleted: 2,
        activeTasks: 1,
        totalSpent: 50,
        complaintsFiled: 1,
        completionRate: 67,
      },
    ])
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  it('renders admin user management with loaded users', async () => {
    mocks.auth.getRole.mockReturnValue('Admin')
    const wrapper = await mountView(AdminUsersView)
    expect(wrapper.text()).toContain('client')
  })

  it('renders complaints management with loaded complaints', async () => {
    mocks.auth.getRole.mockReturnValue('Admin')
    const wrapper = await mountView(ComplaintsView)
    expect(wrapper.text()).toContain('Item was missing')
  })

  it('renders client tasks with active and completed data', async () => {
    const wrapper = await mountView(TasksView)
    expect(wrapper.text()).toContain('Grocery run')
  })

  it('renders runner tasks with assigned and available task sections', async () => {
    mocks.auth.getRole.mockReturnValue('Runner')
    mocks.auth.getUserId.mockReturnValue(2)
    const wrapper = await mountView(RunnerTasksView)
    expect(wrapper.text()).toContain('Grocery run')
  })

  it('renders task detail with task items and status logs', async () => {
    const wrapper = await mountView(TaskDetailView)
    expect(wrapper.text()).toContain('Pick up groceries')
    expect(wrapper.text()).toContain('Buy milk')
  })

  it('renders payments for a client', async () => {
    const wrapper = await mountView(PaymentsView)
    expect(wrapper.text()).toContain('Succeeded')
  })

  it('renders dashboard for client, runner, and admin roles', async () => {
    let wrapper = await mountView(DashboardView)
    expect(wrapper.text()).toContain('Welcome back, client')

    mocks.auth.getRole.mockReturnValue('Runner')
    mocks.auth.getUserId.mockReturnValue(2)
    wrapper = await mountView(DashboardView)
    expect(wrapper.text()).toContain('Available Tasks')

    mocks.auth.getRole.mockReturnValue('Admin')
    wrapper = await mountView(DashboardView)
    expect(wrapper.text()).toContain('Total Users')
  })

  it('renders stats views with loaded stats', async () => {
    mocks.auth.getRole.mockReturnValue('Admin')
    const clientStats = await mountView(ClientStatsView)
    expect(clientStats.text()).toContain('client')

    mocks.auth.getRole.mockReturnValue('Runner')
    const runnerStats = await mountView(RunnerStatsView)
    expect(runnerStats.text()).toContain('runner')
  })

  it('renders chatroom with existing chats', async () => {
    vi.useFakeTimers()
    const wrapper = await mountView(ChatroomView)
    expect(mocks.chat.getChats).toHaveBeenCalled()
    wrapper.unmount()
    vi.useRealTimers()
  })
})
