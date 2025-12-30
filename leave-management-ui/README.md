# Leave Management UI

A Vue.js frontend application for the Leave Management API.

## Features

- **Authentication**: Login, Register, JWT token management with auto-refresh
- **Dashboard**: View leave balances, recent requests, and quick stats
- **Leave Requests**: Create, view, and cancel leave requests
- **Approvals**: Managers can approve/reject pending leave requests with comments
- **Leave Balance**: View detailed leave balance breakdown by leave type
- **Leave Calendar**: Visual calendar showing approved/pending leaves and public holidays
  - Monthly navigation
  - Weekend highlighting
  - Click on events for details
- **Public Holidays**: Admins can manage public holidays by year
- **Employee Management**: Admins can view employees and change roles
- **Leave Types**: Admins can manage leave types
- **Reports**: Managers and admins can view leave reports and analytics
- **Notifications**: 
  - Real-time notifications via SignalR
  - Browser push notifications
  - Notification bell with unread count
  - Full notifications page with mark as read functionality

## Tech Stack

- **Vue 3** with Composition API
- **Vite** for fast development
- **Tailwind CSS** for styling
- **Pinia** for state management
- **Vue Router** for navigation
- **Axios** for API calls
- **SignalR** for real-time notifications

## Setup

1. Install dependencies:
```bash
cd leave-management-ui
npm install
```

2. Start the development server:
```bash
npm run dev
```

3. Make sure your API is running on `http://localhost:5000` (or update `vite.config.js` to match your API URL)

4. Open your browser at `http://localhost:5173`

## Project Structure

```
src/
├── main.js              # App entry point
├── App.vue              # Root component with navigation
├── style.css            # Global styles with Tailwind
├── components/
│   └── NotificationBell.vue  # Real-time notification component
├── router/
│   └── index.js         # Route definitions
├── services/
│   └── api.js           # Axios instance with interceptors
├── stores/
│   ├── auth.js          # Authentication state
│   └── leave.js         # Leave-related state
└── views/
    ├── LoginView.vue
    ├── RegisterView.vue
    ├── DashboardView.vue
    ├── LeaveRequestsView.vue
    ├── NewLeaveRequestView.vue
    ├── LeaveBalanceView.vue
    ├── CalendarView.vue
    ├── HolidaysView.vue
    ├── EmployeesView.vue
    ├── LeaveTypesView.vue
    ├── ReportsView.vue
    ├── NotificationsView.vue
    └── NotFoundView.vue
```

## Routes

| Path | Component | Access |
|------|-----------|--------|
| `/login` | LoginView | Guest only |
| `/register` | RegisterView | Guest only |
| `/dashboard` | DashboardView | Authenticated |
| `/leave-requests` | LeaveRequestsView | Authenticated |
| `/leave-requests/new` | NewLeaveRequestView | Authenticated |
| `/leave-balance` | LeaveBalanceView | Authenticated |
| `/calendar` | CalendarView | Authenticated |
| `/holidays` | HolidaysView | Authenticated |
| `/employees` | EmployeesView | Admin only |
| `/leave-types` | LeaveTypesView | Admin only |
| `/reports` | ReportsView | Manager/Admin |
| `/notifications` | NotificationsView | Authenticated |

## API Configuration

The API base URL is configured in `src/services/api.js`. Update if your API runs on a different port:

```javascript
const api = axios.create({
  baseURL: 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json'
  }
})
```

## Environment Variables

You can configure the API URL using environment variables:

```env
VITE_API_URL=http://localhost:5000
```

## Build for Production

```bash
npm run build
```

The output will be in the `dist/` folder.

## Backend Requirements

For full functionality, the backend API should provide:

- **Authentication**: `/api/auth/login`, `/api/auth/register`, `/api/auth/refresh`
- **Leave Requests**: CRUD operations at `/api/leaverequests`
- **Leave Types**: `/api/leavetypes`
- **Leave Balance**: `/api/leavebalance`
- **Employees**: `/api/employees`
- **Public Holidays**: `/api/publicholidays`
- **Notifications**: `/api/notifications` with SignalR hub at `/hubs/notifications`
