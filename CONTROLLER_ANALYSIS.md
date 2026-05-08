# Backend Controller Analysis

## 1. TasksController
**File Path:** [backend/Controllers/TasksController.cs](backend/Controllers/TasksController.cs)

### Overview
Manages task operations with comprehensive authorization and business logic for recurring tasks, assignments, and soft deletes.

### Class-Level Authorization
- `[Authorize]` - All endpoints require authentication
- Dependencies: `AppDbContext`, `IEmailService`, `IImageStorageService`, `ILogger<TasksController>`

### Methods & Actions

| Method | HTTP | Endpoint | Parameters | Return Type | Role/Auth | Key Logic |
|--------|------|----------|------------|-------------|-----------|-----------|
| `GetTasks()` | GET | `api/tasks` | None | `IEnumerable<object>` | All | Role-based filtering: Clients see own tasks, Runners see unassigned + own, Admin sees all. Excludes soft-deleted & expired tasks. |
| `GetTask(id)` | GET | `api/tasks/{id}` | `id: int` | `object` | All | Role-based access control. Runners cannot see tasks without TaskItems. |
| `CreateTask(dto)` | POST | `api/tasks` | `CreateTaskDto` | `object` | All | Validates ClientId, supports recurring task creation with configurable frequency, sends email notification. |
| `UpdateTask(id, dto)` | PATCH | `api/tasks/{id}` | `id: int, UpdateTaskDto` | `TodoTask` | Client/Admin | Only task owner (Client) can update unless Admin. |
| `DeleteTask(id)` | DELETE | `api/tasks/{id}` | `id: int` | `IActionResult` | All | Hard delete for Admin; soft delete (IsDeletedByClient/IsDeletedByRunner) for others. Client requires payment confirmation. |
| `UploadTaskPhoto(file)` | POST | `api/tasks/upload-photo` | `IFormFile` | `object` | All | Delegates to `IImageStorageService`. Max 1MB, supports JPG/PNG/WebP/GIF. |
| `AssignTask(id)` | PATCH | `api/tasks/{id}/assign` | `id: int` | `TodoTask` | Runner | Runner assigns themselves to unassigned task, sends email to client. |
| `UnassignTask(id)` | PATCH | `api/tasks/{id}/unassign` | `id: int` | `TodoTask` | Runner/Admin | Cannot unassign if TaskItems already marked completed, sends email to client. |

### Authorization/Role-Based Checks
- **Client**: Can create tasks, update own tasks, can only delete after payment
- **Runner**: Can assign/unassign tasks, can see unassigned + own tasks (with TaskItems only), can delete own
- **Admin**: Full access, hard deletes, sees all tasks

### Business Logic Paths
1. **Recurring Task Creation**: Validates recurrence params, calculates weekly occurrences, creates multiple task entries
2. **Task Expiration**: Marks tasks expired if `ExpirationDate` passed, sends notifications
3. **Soft Deletion**: Clients/Runners mark deleted rather than hard delete; Admin can hard delete
4. **Email Notifications**: Sent on creation, assignment, unassignment, expiration

---

## 2. ReviewsController
**File Path:** [backend/Controllers/ReviewsController.cs](backend/Controllers/ReviewsController.cs)

### Overview
Handles review creation and retrieval with sentiment analysis for rating adjustment using Lithuanian keywords.

### Class-Level Authorization
- `[Authorize]` - All endpoints require authentication
- Dependencies: `AppDbContext`

### Methods & Actions

| Method | HTTP | Endpoint | Parameters | Return Type | Role/Auth | Key Logic |
|--------|------|----------|------------|-------------|-----------|-----------|
| `CreateReview(dto)` | POST | `api/reviews` | `CreateReviewDto` | `ReviewDto` | All | Validates both reviewer & reviewee involved in task, calculates final rating with sentiment bonus/penalty, updates user rating. |
| `GetReview(id)` | GET | `api/reviews/{id}` | `id: int` | `ReviewDto` | All | Returns review with reviewer & reviewee details. |
| `GetReviewsByTask(taskId)` | GET | `api/reviews/task/{taskId}` | `taskId: int` | `List<ReviewDto>` | All | Gets all reviews for specific task. |
| `GetUserReviews(userId)` | GET | `api/reviews/user/{userId}` | `userId: int` | `GetUserReviewsDto` | All | Gets all reviews for user with stats (average rating, total reviews). |

### Authorization/Role-Based Checks
- **Implicit**: User must be involved in the task (client or runner) to leave a review
- **Cross-role validation**: Reviewer must be opposite party (client reviews runner or vice versa)
- **Duplicate prevention**: One review per user per task

### Business Logic Paths
1. **Rating Calculation**: 
   - Base: star rating (1-5)
   - Sentiment analysis: +/- bonuses for Lithuanian keywords (e.g., "puikiai" +0.5m, "blogai" -0.4m)
   - Timeliness bonus: +0.5m (7+ days early), +0.3m (3+ days early), +0.2m (1+ day early)
   - Lateness penalty: -1m (7+ days late), -0.5m (3+ days late), -0.3m (1+ day late)
   - Clamped to 1-5 range
2. **User Rating Update**: Recalculates user average from all final ratings

---

## 3. ComplaintsController
**File Path:** [backend/Controllers/ComplaintsController.cs](backend/Controllers/ComplaintsController.cs)

### Overview
Manages user complaints about completed tasks with admin resolution workflow.

### Class-Level Authorization
- `[Authorize]` - All endpoints require authentication
- Dependencies: `AppDbContext`

### Methods & Actions

| Method | HTTP | Endpoint | Parameters | Return Type | Role/Auth | Key Logic |
|--------|------|----------|------------|-------------|-----------|-----------|
| `GetComplaints()` | GET | `api/complaints` | None | `IEnumerable<ComplaintDto>` | Admin Only | Returns all complaints sorted by creation date (newest first). |
| `CreateComplaint(dto)` | POST | `api/complaints` | `CreateComplaintDto` | `ComplaintDto` | Client/Runner | Validates task is completed, prevents duplicate complaints, sets appropriate client/runner context. |
| `DeleteComplaint(id)` | DELETE | `api/complaints/{id}` | `id: int` | `IActionResult` | Admin Only | Hard deletes complaint. |
| `ResolveComplaint(id)` | PATCH | `api/complaints/{id}/resolve` | `id: int` | `ComplaintDto` | Admin Only | Marks complaint as resolved. |

### Authorization/Role-Based Checks
- **Get**: Admin only
- **Create**: Client or Runner (must be party in task, cannot complain about own side)
- **Delete/Resolve**: Admin only

### Business Logic Paths
1. **Complaint Validation**: 
   - Task must be completed (all TaskItems marked completed)
   - Reviewer must be involved (client or runner)
   - One complaint per user per task
2. **Context Assignment**: Complaint records which party filed it (client or runner)

---

## 4. StatusLogsController
**File Path:** [backend/Controllers/StatusLogsController.cs](backend/Controllers/StatusLogsController.cs)

### Overview
Tracks status updates for TaskItems with runner-specific logging and audit trail.

### Class-Level Authorization
- `[Authorize]` - All endpoints require authentication
- Dependencies: `AppDbContext`

### Methods & Actions

| Method | HTTP | Endpoint | Parameters | Return Type | Role/Auth | Key Logic |
|--------|------|----------|------------|-------------|-----------|-----------|
| `GetStatusLogs(taskItemId?)` | GET | `api/statuslogs?taskItemId=X` | `taskItemId: int?` | `IEnumerable<object>` | All | Returns logs for specific TaskItem or all logs if not filtered, ordered newest first. |
| `CreateStatusLog(dto)` | POST | `api/statuslogs` | `CreateStatusLogDto` | `object` | Runner/Admin | Creates log entry attributed to current user (runner). |
| `UpdateStatusLog(id, dto)` | PUT | `api/statuslogs/{id}` | `id: int, UpdateStatusLogDto` | `object` | Runner/Admin | Owner (runner) or Admin can edit; original timestamp preserved. |
| `DeleteStatusLog(id)` | DELETE | `api/statuslogs/{id}` | `id: int` | `IActionResult` | Runner/Admin | Owner or Admin can delete. Returns 204 No Content. |

### Authorization/Role-Based Checks
- **Create**: Runner or Admin (attributed to current user)
- **Update/Delete**: Owner of log or Admin only (403 Forbidden if not owner)
- **Read**: All authenticated users

### Business Logic Paths
1. **Ownership Verification**: Each log tracks runner who created it; updates restricted to owner/admin
2. **Immutable Timestamps**: Creation timestamp never updated, only status/comment

---

## 5. TaskItemsController
**File Path:** [backend/Controllers/TaskItemsController.cs](backend/Controllers/TaskItemsController.cs)

### Overview
Manages checklist items within tasks with completion tracking and email notifications.

### Class-Level Authorization
- `[Authorize]` - All endpoints require authentication
- Dependencies: `AppDbContext`, `IEmailService`

### Methods & Actions

| Method | HTTP | Endpoint | Parameters | Return Type | Role/Auth | Key Logic |
|--------|------|----------|------------|-------------|-----------|-----------|
| `GetTaskItems(taskId)` | GET | `api/taskitems?taskId=X` | `taskId: int` | `IEnumerable<object>` | All | Returns all items for task, ordered by ID. |
| `CreateTaskItem(dto)` | POST | `api/taskitems` | `CreateTaskItemDto` | `object` | Client/Admin | Only task owner (client) or Admin can create. |
| `UpdateTaskItem(id, dto)` | PUT | `api/taskitems/{id}` | `id: int, UpdateTaskItemDto` | `object` | Runner/Client/Admin | Task owner, assigned runner, or Admin can update. Sends completion email when all items done. |
| `DeleteTaskItem(id)` | DELETE | `api/taskitems/{id}` | `id: int` | `IActionResult` | Client/Admin | Only task owner (client) or Admin can delete. |

### Authorization/Role-Based Checks
- **Create**: Task owner (Client) or Admin
- **Read**: All (implicit through task query param)
- **Update**: Task owner, assigned Runner, or Admin
- **Delete**: Task owner (Client) or Admin

### Business Logic Paths
1. **Completion Detection**: When item marked complete, checks if all items complete
2. **Task Completion Email**: Sent to client when all TaskItems marked completed
3. **Permission Validation**: Both Client and Runner can update (to mark done), only Client can create/delete

---

## 6. UsersController
**File Path:** [backend/Controllers/UsersController.cs](backend/Controllers/UsersController.cs)

### Overview
User management with authentication, role-based access, and statistics endpoints.

### Class-Level Authorization
- `[Authorize]` - Most endpoints require authentication (exceptions marked)
- Dependencies: `AppDbContext`, `IConfiguration`, `IEmailService`

### Methods & Actions

| Method | HTTP | Endpoint | Parameters | Return Type | Role/Auth | Key Logic |
|--------|------|----------|------------|-------------|-----------|-----------|
| `GetUsers()` | GET | `api/users` | None | `IEnumerable<User>` | All | Admin sees all; Clients/Runners see only themselves. |
| `GetUser(id)` | GET | `api/users/{id}` | `id: int` | `User` | All | Admin sees any; others see only themselves. |
| `CreateUser(dto)` | POST | `api/users` | `CreateUserDto` | `User` | Admin Only (Policy) | Creates user with hashed password; default role "Client". |
| `Register(dto)` | POST | `api/users/register` | `RegisterDto` | `User` | AllowAnonymous | Public registration, sets role "Client", sends confirmation email. |
| `Login(dto)` | POST | `api/users/login` | `LoginDto` | `AuthResponse` | AllowAnonymous | BCrypt password verification, JWT token generation (7-day expiry). |
| `DeleteUser(id)` | DELETE | `api/users/{id}` | `id: int` | `IActionResult` | Admin Only (Policy) | Soft check for dependencies; returns 409 Conflict if task dependencies exist. |
| `UpdateUser(id, dto)` | PATCH | `api/users/{id}` | `id: int, UpdateUserDto` | `IActionResult` | Self/Admin | User can update own profile; only Admin can change roles. |
| `GetRunnerStats()` | GET | `api/users/runners/stats` | None | `IEnumerable<RunnerStatsDto>` | All | Returns runners sorted by rating with: completed tasks, active tasks, money earned, acceptance rate. |
| `GetClientStats()` | GET | `api/users/clients/stats` | None | `IEnumerable<ClientStatsDto>` | All | Returns clients sorted by rating with: created tasks, completed tasks, spent, complaints filed, completion rate. |
| `GetAdminStats()` | GET | `api/users/admin/stats` | None | `AdminStatsDto` | Admin Only | Dashboard with system-wide metrics. |

### Authorization/Role-Based Checks
- **GetUsers**: Admin (all) vs Self (own only)
- **GetUser**: Admin (any) vs Self (own only)
- **Create**: Admin only (AdminOnly policy)
- **Delete**: Admin only (AdminOnly policy)
- **Update**: Self or Admin
- **Register**: Public/anonymous
- **Login**: Public/anonymous
- **Stats endpoints**: Get: All authenticated; Admin: Admin only

### Business Logic Paths
1. **Authentication**: BCrypt hashing for passwords, JWT token (7-day expiry) with NameIdentifier, Name, Role claims
2. **Password Validation**: Checks hash starts with "$2" (BCrypt format)
3. **User Stats Calculation**:
   - **Runners**: Completed tasks, active tasks, money earned, task acceptance rate
   - **Clients**: Tasks created/completed, active tasks, total spent, complaints filed, completion rate
   - **Admin**: Overall system statistics

---

## 7. PaymentsController
**File Path:** [backend/Controllers/PaymentsController.cs](backend/Controllers/PaymentsController.cs)

### Overview
Stripe payment integration for task fees with payment tracking.

### Class-Level Authorization
- `[Authorize]` - All endpoints require authentication
- Dependencies: `IPaymentService`, `AppDbContext`, `ILogger<PaymentsController>`

### Methods & Actions

| Method | HTTP | Endpoint | Parameters | Return Type | Role/Auth | Key Logic |
|--------|------|----------|------------|-------------|-----------|-----------|
| `CreatePaymentIntent(request)` | POST | `api/payments/create-intent` | `CreatePaymentDto` | `IActionResult` | All | Validates task ownership, checks payment not already made, delegates to `IPaymentService`. |
| `ConfirmPayment(request)` | POST | `api/payments/confirm` | `ConfirmPaymentDto` | `IActionResult` | All | Confirms payment via `IPaymentService` after Stripe processing. |
| `GetPaymentHistory(taskId)` | GET | `api/payments/task/{taskId}` | `taskId: int` | `IActionResult` | All | Retrieves payment history for task. |
| `HasPaid(taskId)` | GET | `api/payments/{taskId}/paid` | `taskId: int` | `IActionResult` | All | Checks if current user paid for task. |
| `GetMyPayments()` | GET | `api/payments/my-payments` | None | `IActionResult` | All | Returns all payments made by current user. |

### Authorization/Role-Based Checks
- **Create Intent**: User must own the task (Client)
- **All others**: Current user identification via NameIdentifier claim

### Business Logic Paths
1. **Payment Intent Creation**: Validates task exists, user owns task, task not already paid
2. **Double Payment Prevention**: Checks `HasPaidAsync` before allowing new payment
3. **User-Specific Queries**: GetMyPayments retrieves only current user's payments

---

## 8. ChatsController
**File Path:** [backend/Controllers/ChatsController.cs](backend/Controllers/ChatsController.cs)

### Overview
One-on-one messaging system with participant filtering based on active task relationships.

### Class-Level Authorization
- `[Authorize]` - All endpoints require authentication
- Dependencies: `AppDbContext`

### Methods & Actions

| Method | HTTP | Endpoint | Parameters | Return Type | Role/Auth | Key Logic |
|--------|------|----------|------------|-------------|-----------|-----------|
| `GetChats()` | GET | `api/chats` | None | `IEnumerable<ChatDto>` | All | Returns all chats for current user with last message, ordered newest updated. |
| `GetChat(chatId)` | GET | `api/chats/{chatId}` | `chatId: int` | `ChatDto` | All | Returns specific chat with all messages ordered by sent time. Permission: user must be participant. |
| `CreateOrGetChat(dto)` | POST | `api/chats` | `CreateChatDto` | `ChatDto` | All | Creates new 1-on-1 chat or returns existing; canonicalizes user IDs (lower=User1, higher=User2). |
| `SendMessage(chatId, dto)` | POST | `api/chats/{chatId}/messages` | `chatId: int, SendChatMessageDto` | `ChatMessageDto` | All | Sends message (validates non-empty content), updates chat's UpdatedAt. |
| `GetChatParticipants()` | GET | `api/chats/participants` | None | `IEnumerable<UserDto>` | All | Returns eligible chat partners: Clients see runners from active tasks + Admin; Runners see clients from active tasks + Admin. |
| `DeleteChat(chatId)` | DELETE | `api/chats/{chatId}` | `chatId: int` | `IActionResult` | All | Deletes chat & all messages; user must be participant. |

### Authorization/Role-Based Checks
- **Get/Send/Delete**: User must be participant in chat (User1 or User2)
- **Get Participants**: Role-based filtering (Client ↔ Runner on active tasks)
- **Self-chat Prevention**: Cannot create chat with self

### Business Logic Paths
1. **Participant Eligibility**: 
   - **Client**: Can chat with runners assigned to their incomplete tasks + Admin
   - **Runner**: Can chat with clients who have incomplete tasks assigned to them + Admin
   - Incomplete = no TaskItems or not all completed
2. **Chat Canonicalization**: Always stores lower ID as User1, higher as User2
3. **Message Ordering**: Listed newest-first in GetChats (last message), chronological in GetChat (all messages)

---

## Summary Table: Authorization Patterns

| Controller | Requires Auth | Public Endpoints | Admin-Only Endpoints | Role-Specific |
|-----------|---------------|------------------|---------------------|---------------|
| Tasks | Yes | None | Delete (hard delete) | Assign/Unassign (Runner) |
| Reviews | Yes | None | None | Implicit (task participant) |
| Complaints | Yes | None | GetAll, Delete, Resolve | Create (Client/Runner) |
| StatusLogs | Yes | None | None | Create/Update/Delete (Owner/Admin) |
| TaskItems | Yes | None | None | Create/Update (owner/runner/admin), Delete (owner/admin) |
| Users | Partial | Register, Login | Create, Delete, AdminStats | GetUser/UpdateUser (self/admin) |
| Payments | Yes | None | None | Owner verification for intent |
| Chats | Yes | None | None | Participant-based access |

## Key Dependencies & Services

| Service | Used In | Purpose |
|---------|---------|---------|
| `IEmailService` | Tasks, TaskItems, UsersController | Notifications: task creation, completion, assignment, registration |
| `IImageStorageService` | Tasks | Photo uploads for tasks (max 1MB) |
| `IPaymentService` | Payments | Stripe payment intent creation & confirmation |
| `ILogger<T>` | Tasks, Payments | Error/warning logging |

## Security Considerations

1. **Authentication**: JWT-based (7-day expiry)
2. **Authorization**: Claim-based roles (Admin, Client, Runner)
3. **Soft Deletes**: Clients/Runners soft-delete tasks; Admin hard-deletes
4. **Password Storage**: BCrypt hashing validated before storage
5. **Email Redaction**: Passwords excluded from API responses
6. **Ownership Verification**: All modifications require current user ID match or Admin
