import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes = [
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue'),
    meta: { requiresGuest: true }
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('../views/RegisterView.vue'),
    meta: { requiresGuest: true }
  },
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () => import('../views/DashboardView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/leave-requests',
    name: 'leave-requests',
    component: () => import('../views/LeaveRequestsView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/leave-requests/new',
    name: 'new-leave-request',
    component: () => import('../views/NewLeaveRequestView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/pending-approvals',
    redirect: '/leave-requests' // Pending approvals tab is in LeaveRequestsView
  },
  {
    path: '/leave-balance',
    name: 'leave-balance',
    component: () => import('../views/LeaveBalanceView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/employees',
    name: 'employees',
    component: () => import('../views/EmployeesView.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/leave-types',
    name: 'leave-types',
    component: () => import('../views/LeaveTypesView.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/calendar',
    name: 'calendar',
    component: () => import('../views/CalendarView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/reports',
    name: 'reports',
    component: () => import('../views/ReportsView.vue'),
    meta: { requiresAuth: true, requiresManagerOrAdmin: true }
  },
  {
    path: '/holidays',
    name: 'holidays',
    component: () => import('../views/HolidaysView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('../views/NotFoundView.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next({ name: 'dashboard' })
  } else if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next({ name: 'dashboard' })
  } else if (to.meta.requiresManagerOrAdmin && !authStore.isManagerOrAdmin) {
    next({ name: 'dashboard' })
  } else {
    next()
  }
})

export default router