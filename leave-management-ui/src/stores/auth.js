import { defineStore } from 'pinia'
import api from '../services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null,
    token: localStorage.getItem('token') || null,
    refreshToken: localStorage.getItem('refreshToken') || null
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.user?.role === 'Admin',
    isManager: (state) => state.user?.role === 'Manager',
    isEmployee: (state) => state.user?.role === 'Employee',
    isManagerOrAdmin: (state) => state.user?.role === 'Admin' || state.user?.role === 'Manager',
    fullName: (state) => state.user ? `${state.user.firstName} ${state.user.lastName}` : '',
    userRole: (state) => state.user?.role || ''
  },

  actions: {
    async login(email, password) {
      try {
        const response = await api.post('/api/auth/login', { email, password })
        const { accessToken, refreshToken, user } = response.data

        this.setAuthData(accessToken, refreshToken, user)
        return { success: true }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Login failed'
        }
      }
    },

    async register(userData) {
      try {
        const response = await api.post('/api/auth/register', userData)
        const { accessToken, refreshToken, user } = response.data

        this.setAuthData(accessToken, refreshToken, user)
        return { success: true }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Registration failed',
          errors: error.response?.data?.errors
        }
      }
    },

    async refreshAccessToken() {
      try {
        const response = await api.post('/api/auth/refresh', {
          refreshToken: this.refreshToken
        })
        const { accessToken, refreshToken } = response.data

        this.token = accessToken
        this.refreshToken = refreshToken
        localStorage.setItem('token', accessToken)
        localStorage.setItem('refreshToken', refreshToken)

        return true
      } catch (error) {
        this.logout()
        return false
      }
    },

    setAuthData(accessToken, refreshToken, user) {
      this.token = accessToken
      this.refreshToken = refreshToken
      this.user = user

      localStorage.setItem('token', accessToken)
      localStorage.setItem('refreshToken', refreshToken)
      localStorage.setItem('user', JSON.stringify(user))
    },

    logout() {
      this.token = null
      this.refreshToken = null
      this.user = null

      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')
    },

    async updateProfile(userData) {
      try {
        const response = await api.put(`/api/employees/${this.user.id}`, userData)
        this.user = { ...this.user, ...response.data }
        localStorage.setItem('user', JSON.stringify(this.user))
        return { success: true }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Profile update failed' 
        }
      }
    }
  }
})