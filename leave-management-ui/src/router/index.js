import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/LoginView.vue'),
    meta: { requiresGuest: true }
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('../views/RegisterView.vue'),
    meta: { requiresGuest: true }
  },
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/DashboardView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/leave-requests',
    name: 'LeaveRequests',
    component: () => import('../views/LeaveRequestsView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/leave-requests/new',
    name: 'NewLeaveRequest',
    component: () => import('../views/NewLeaveRequestView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/pending-approvals',
    name: 'PendingApprovals',
    component: () => import('../views/PendingApprovalsView.vue'),
    meta: { requiresAuth: true, roles: ['Manager', 'Admin'] }
  },
  {
    path: '/employees',
    name: 'Employees',
    component: () => import('../views/EmployeesView.vue'),
    meta: { requiresAuth: true, roles: ['Admin'] }
  },
  {
    path: '/leave-types',
    name: 'LeaveTypes',
    component: () => import('../views/LeaveTypesView.vue'),
    meta: { requiresAuth: true, roles: ['Admin'] }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next('/dashboard')
  } else if (to.meta.roles && !to.meta.roles.includes(authStore.user?.role)) {
    next('/dashboard')
  } else {
    next()
  }
})

export default router
