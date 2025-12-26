import { defineStore } from 'pinia'
import api from '../services/api'
import { useAuthStore } from './auth'

export const useLeaveStore = defineStore('leave', {
  state: () => ({
    leaveRequests: [],
    leaveTypes: [],
    leaveBalances: [],
    pendingRequests: [],
    loading: false,
    error: null
  }),

  getters: {
    getLeaveRequestById: (state) => (id) => {
      return state.leaveRequests.find(r => r.id === id)
    },
    pendingCount: (state) => state.pendingRequests.length,
    approvedRequests: (state) => state.leaveRequests.filter(r => r.status === 'Approved'),
    rejectedRequests: (state) => state.leaveRequests.filter(r => r.status === 'Rejected')
  },

  actions: {
    async fetchLeaveTypes() {
      try {
        this.loading = true
        const response = await api.get('/api/leavetypes')
        this.leaveTypes = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch leave types'
      } finally {
        this.loading = false
      }
    },

    async fetchMyLeaveRequests() {
      try {
        this.loading = true
        const authStore = useAuthStore()
        const response = await api.get(`/api/leaverequests/employee/${authStore.user.id}`)
        this.leaveRequests = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch leave requests'
      } finally {
        this.loading = false
      }
    },

    async fetchAllLeaveRequests() {
      try {
        this.loading = true
        const response = await api.get('/api/leaverequests')
        this.leaveRequests = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch leave requests'
      } finally {
        this.loading = false
      }
    },

    async fetchPendingRequests() {
      try {
        this.loading = true
        const response = await api.get('/api/leaverequests/pending')
        this.pendingRequests = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch pending requests'
      } finally {
        this.loading = false
      }
    },

    async fetchLeaveBalances() {
      try {
        this.loading = true
        const response = await api.get('/api/leavebalance/my-balances')
        this.leaveBalances = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch leave balances'
      } finally {
        this.loading = false
      }
    },

    async createLeaveRequest(requestData) {
      try {
        this.loading = true
        const authStore = useAuthStore()
        const payload = {
          ...requestData,
          employeeId: authStore.user.id
        }
        const response = await api.post('/api/leaverequests', payload)
        const data = response.data.data || response.data
        if (data) {
          this.leaveRequests.push(data)
        }
        return { success: true, data }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Failed to create leave request'
        }
      } finally {
        this.loading = false
      }
    },

    async approveLeaveRequest(id, comments = '') {
      try {
        this.loading = true
        const authStore = useAuthStore()
        await api.post(`/api/leaverequests/${id}/approve`, { 
          approvedById: authStore.user.id,
          comments 
        })
        await this.fetchPendingRequests()
        return { success: true }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Failed to approve leave request'
        }
      } finally {
        this.loading = false
      }
    },

    async rejectLeaveRequest(id, comments) {
      try {
        this.loading = true
        const authStore = useAuthStore()
        await api.post(`/api/leaverequests/${id}/reject`, { 
          approvedById: authStore.user.id,
          comments 
        })
        await this.fetchPendingRequests()
        return { success: true }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Failed to reject leave request'
        }
      } finally {
        this.loading = false
      }
    },

    async cancelLeaveRequest(id) {
      try {
        this.loading = true
        await api.post(`/api/leaverequests/${id}/cancel`)
        this.leaveRequests = this.leaveRequests.filter(r => r.id !== id)
        await this.fetchMyLeaveRequests()
        return { success: true }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Failed to cancel leave request'
        }
      } finally {
        this.loading = false
      }
    }
  }
})