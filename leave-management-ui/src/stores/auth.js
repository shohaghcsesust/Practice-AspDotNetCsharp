import { defineStore } from 'pinia'
import api from '../services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null,
    accessToken: localStorage.getItem('accessToken') || null,
    refreshTokenValue: localStorage.getItem('refreshToken') || null,
    expiresAt: localStorage.getItem('expiresAt') || null
  }),

  getters: {
    isAuthenticated: (state) => !!state.accessToken,
    isAdmin: (state) => state.user?.role === 'Admin',
    isManager: (state) => state.user?.role === 'Manager',
    isManagerOrAdmin: (state) => ['Manager', 'Admin'].includes(state.user?.role)
  },

  actions: {
    async login(email, password) {
      try {
        const response = await api.post('/auth/login', { email, password })
        this.setAuthData(response.data)
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
        const response = await api.post('/auth/register', userData)
        this.setAuthData(response.data)
        return { success: true }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Registration failed'
        }
      }
    },

    async refreshToken() {
      try {
        const response = await api.post('/auth/refresh', {
          refreshToken: this.refreshTokenValue
        })
        this.setAuthData(response.data)
        return true
      } catch (error) {
        this.clearAuthData()
        throw error
      }
    },

    async logout() {
      try {
        if (this.refreshTokenValue) {
          await api.post('/auth/logout', {
            refreshToken: this.refreshTokenValue
          })
        }
      } catch (error) {
        console.error('Logout error:', error)
      } finally {
        this.clearAuthData()
      }
    },

    setAuthData(data) {
      this.user = data.user
      this.accessToken = data.accessToken
      this.refreshTokenValue = data.refreshToken
      this.expiresAt = data.expiresAt

      localStorage.setItem('user', JSON.stringify(data.user))
      localStorage.setItem('accessToken', data.accessToken)
      localStorage.setItem('refreshToken', data.refreshToken)
      localStorage.setItem('expiresAt', data.expiresAt)
    },

    clearAuthData() {
      this.user = null
      this.accessToken = null
      this.refreshTokenValue = null
      this.expiresAt = null

      localStorage.removeItem('user')
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('expiresAt')
    }
  }
})
