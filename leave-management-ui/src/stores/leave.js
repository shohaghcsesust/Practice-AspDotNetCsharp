import { defineStore } from 'pinia'
import api from '../services/api'

export const useLeaveStore = defineStore('leave', {
  state: () => ({
    leaveRequests: [],
    pendingRequests: [],
    leaveTypes: [],
    leaveBalances: [],
    loading: false,
    error: null
  }),

  actions: {
    async fetchMyLeaveRequests(employeeId) {
      this.loading = true
      try {
        const response = await api.get(`/leaverequests/employee/${employeeId}`)
        this.leaveRequests = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch leave requests'
      } finally {
        this.loading = false
      }
    },

    async fetchPendingRequests() {
      this.loading = true
      try {
        const response = await api.get('/leaverequests/pending')
        this.pendingRequests = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch pending requests'
      } finally {
        this.loading = false
      }
    },

    async fetchLeaveTypes() {
      try {
        const response = await api.get('/leavetypes/active')
        this.leaveTypes = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch leave types'
      }
    },

    async fetchLeaveBalances() {
      try {
        const response = await api.get('/leavebalance/my-balances')
        this.leaveBalances = response.data
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to fetch leave balances'
      }
    },

    async createLeaveRequest(data) {
      try {
        const response = await api.post('/leaverequests', data)
        return { success: response.data.success, data: response.data.data, message: response.data.message }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Failed to create leave request'
        }
      }
    },

    async approveRequest(id, approvedById, comments) {
      try {
        const response = await api.post(`/leaverequests/${id}/approve`, {
          approvedById,
          comments
        })
        return { success: response.data.success, message: response.data.message }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Failed to approve request'
        }
      }
    },

    async rejectRequest(id, approvedById, comments) {
      try {
        const response = await api.post(`/leaverequests/${id}/reject`, {
          approvedById,
          comments
        })
        return { success: response.data.success, message: response.data.message }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Failed to reject request'
        }
      }
    },

    async cancelRequest(id) {
      try {
        const response = await api.post(`/leaverequests/${id}/cancel`)
        return { success: response.data.success, message: response.data.message }
      } catch (error) {
        return {
          success: false,
          message: error.response?.data?.message || 'Failed to cancel request'
        }
      }
    }
  }
})
