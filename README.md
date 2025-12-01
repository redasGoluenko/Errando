# Errando - Task Management System

## 📋 Project Overview

**Errando** is a full-stack task management system designed for clients to create tasks and runners to execute them. The system provides real-time status tracking, task assignment, and progress monitoring.

### 🎯 Purpose

Laboratory work #3 - Web Development course demonstrating:
- REST API integration
- Responsive UI design
- Role-based authentication
- Real-time task tracking

---

## 🚀 Features

### Core Functionality

- **User Management**
  - Registration & Login (JWT authentication)
  - Three user roles: Admin, Client, Runner
  - Role-based access control

- **Task Management**
  - Create, edit, delete tasks
  - Schedule tasks with date/time
  - Add multiple task items per task
  - Track task progress

- **Runner Assignment**
  - Runners can self-assign tasks
  - View assigned vs unassigned tasks
  - Filter tasks by assignment status

- **Status Tracking**
  - Update task item status (Pending → In Progress → Completed)
  - Add comments with status changes
  - View complete status history timeline
  - See who changed status and when

### UI/UX Features

- ✅ Responsive design (mobile, tablet, desktop)
- ✅ Google Fonts (Inter)
- ✅ SVG vector icons
- ✅ Modal dialogs
- ✅ Smooth transitions and hover effects
- ✅ Mobile hamburger menu
- ✅ Consistent color scheme
- ✅ Loading states and error handling

---

## 🛠️ Technology Stack

### Frontend
- **Vue 3** (Composition API, `<script setup>`)
- **TypeScript** (type safety)
- **Vue Router** (navigation)
- **Tailwind CSS** (styling)
- **Axios** (HTTP client)
- **Vite** (build tool)

### Backend
- **ASP.NET Core 8** (REST API)
- **Entity Framework Core** (ORM)
- **PostgreSQL** (database)
- **JWT** (authentication)
- **Swagger** (API documentation)

---

## 📦 Installation & Setup

### Prerequisites

- Node.js 18+ & npm
- .NET 8 SDK
- PostgreSQL 16

### 1. Clone Repository

```bash
git clone <repository-url>
cd Errando
```

### 2. Backend Setup

```bash
cd backend

# Restore packages
dotnet restore

# Update appsettings.json with your PostgreSQL connection string
# ConnectionString: "Host=localhost;Database=errando;Username=postgres;Password=YOUR_PASSWORD"

# Run backend
dotnet run
```

Backend will start at `http://localhost:5064`

### 3. Database Setup

```powershell
# Create database
$env:PGPASSWORD="YOUR_PASSWORD"
psql -U postgres -c "CREATE DATABASE errando;"

# Seed users
Invoke-WebRequest -Uri "http://localhost:5064/api/users/seed" -Method POST
```

**Seeded users:**
- Admin: `admin` / `password123`
- Client: `client1` / `password123`
- Runner: `runner1` / `password123`

### 4. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Run dev server
npm run dev
```

Frontend will start at `http://localhost:5173`

---

## 📱 User Workflows

### Client Workflow

1. **Register/Login** as Client
2. **Create Task**
   - Add title & description
   - Schedule date/time
   - Add task items (subtasks)
3. **View Tasks**
   - See assigned runner
   - Track progress
   - View status history
4. **Manage Tasks**
   - Edit task details
   - Add/remove task items
   - Delete completed tasks

### Runner Workflow

1. **Login** as Runner
2. **Runner Dashboard**
   - View all available tasks
   - Filter: Unassigned | My Tasks
3. **Assign Task**
   - Click "Assign to Me" on task
4. **Execute Task**
   - Open task details
   - Update task item status
   - Add comments about progress
   - Mark items as completed
5. **Track Work**
   - View status history
   - See completion progress

### Admin Workflow

1. **Login** as Admin
2. **View All Tasks**
   - See tasks from all clients
   - Monitor system-wide progress
3. **Manage System**
   - Full CRUD access
   - View all users and tasks

---

## 📐 Project Structure

```
Errando/
├── backend/
│   ├── Controllers/        # API endpoints
│   ├── Models/            # Data models
│   ├── Data/              # Database context
│   └── Program.cs         # App configuration
│
├── frontend/
│   ├── src/
│   │   ├── components/    # Reusable components
│   │   ├── views/         # Page components
│   │   ├── services/      # API service layer
│   │   ├── router/        # Route configuration
│   │   └── App.vue        # Root component
│   ├── index.html
│   └── package.json
│
└── README.md
```

---

## 🎨 UI Design Features

### Responsive Breakpoints
- Mobile: < 768px (hamburger menu)
- Tablet: 768px - 1024px
- Desktop: > 1024px

### Color Palette
- Primary: Blue (#2563EB)
- Success: Green (#10B981)
- Danger: Red (#EF4444)
- Gray scale: #F9FAFB → #111827

### Typography
- Font Family: Inter (Google Fonts)
- Weights: 300, 400, 500, 600, 700

### Components
- **Navbar:** Sticky header with logo, navigation, user menu
- **Footer:** Company info, links, contact
- **Cards:** Task items, status logs
- **Modals:** Create, edit, delete confirmations
- **Forms:** Input validation, error messages
- **Buttons:** Consistent styling with hover effects

---

## 🔒 API Endpoints

### Authentication
```
POST /api/Users/register    - Register new user
POST /api/Users/login       - Login user
POST /api/Users/seed        - Seed test users (dev only)
```

### Tasks
```
GET    /api/Tasks           - Get tasks (filtered by role)
GET    /api/Tasks/{id}      - Get single task
POST   /api/Tasks           - Create task
PUT    /api/Tasks/{id}      - Update task
DELETE /api/Tasks/{id}      - Delete task
POST   /api/Tasks/{id}/assign - Assign task to runner
```

### Task Items
```
GET    /api/TaskItems?taskId={id}  - Get task items
POST   /api/TaskItems              - Create task item
PUT    /api/TaskItems/{id}         - Update task item
DELETE /api/TaskItems/{id}         - Delete task item
PATCH  /api/TaskItems/{id}/toggle  - Toggle completion
```

### Status Logs
```
GET  /api/StatusLogs?taskItemId={id}  - Get status logs
POST /api/StatusLogs                  - Create status log (update status)
```

---

## ✅ Lab Requirements Checklist

- ✅ REST API integration
- ✅ Responsive layout (3 breakpoints)
- ✅ Header, Content, Footer with distinct styles
- ✅ Multiple input types (text, textarea, select, datetime, checkbox)
- ✅ Transitions and animations
- ✅ Responsive hamburger menu
- ✅ Vector icons (SVG)
- ✅ Custom font (Google Fonts: Inter)
- ✅ Modal windows
- ✅ Harmonious color palette
- ✅ Grid-based layout
- ✅ Clear accessibility
- ✅ Consistent forms
- ✅ Unified design
- ✅ Git repository with documentation

---

## 🐛 Known Issues & Future Improvements

### Potential Enhancements
- Email notifications for status updates
- Photo uploads for proof of work
- Admin dashboard with statistics
- Advanced filtering and search
- Task templates
- Real-time updates (WebSockets)

---

## 👨‍💻 Author

**Student:** [Your Name]  
**Course:** Web Development  
**Lab:** #3 - User Interface Design  
**Year:** 2025

---

## 📄 License

This project is created for educational purposes.