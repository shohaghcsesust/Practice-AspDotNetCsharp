# Leave Management UI

A Vue.js frontend application for the Leave Management API.

## Features

- **Authentication**: Login, Register, JWT token management with auto-refresh
- **Dashboard**: View leave balances and recent requests
- **Leave Requests**: Create, view, and cancel leave requests
- **Approvals**: Managers can approve/reject pending leave requests
- **Employee Management**: Admins can view employees and change roles
- **Leave Types**: Admins can manage leave types

## Tech Stack

- **Vue 3** with Composition API
- **Vite** for fast development
- **Tailwind CSS** for styling
- **Pinia** for state management
- **Vue Router** for navigation
- **Axios** for API calls

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
├── main.js           # App entry point
├── App.vue           # Root component with navigation
├── style.css         # Global styles with Tailwind
├── router/
│   └── index.js      # Route definitions
├── services/
│   └── api.js        # Axios instance with interceptors
├── stores/
│   ├── auth.js       # Authentication state
│   └── leave.js      # Leave-related state
└── views/
    ├── LoginView.vue
    ├── RegisterView.vue
    ├── DashboardView.vue
    ├── LeaveRequestsView.vue
    ├── NewLeaveRequestView.vue
    ├── PendingApprovalsView.vue
    ├── EmployeesView.vue
    └── LeaveTypesView.vue
```

## API Configuration

The proxy is configured in `vite.config.js` to forward `/api` requests to your backend. Update the `target` if your API runs on a different port:

```javascript
proxy: {
  '/api': {
    target: 'http://localhost:5000',
    changeOrigin: true
  }
}
```

## Build for Production

```bash
npm run build
```

The output will be in the `dist/` folder.
