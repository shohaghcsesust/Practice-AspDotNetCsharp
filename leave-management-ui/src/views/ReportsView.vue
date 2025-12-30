<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <!-- Header -->
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Reports & Analytics</h1>
        <div class="flex space-x-2">
          <select
            v-model="selectedYear"
            @change="refreshData"
            class="border border-gray-300 rounded-md px-3 py-2 text-sm focus:ring-indigo-500 focus:border-indigo-500"
          >
            <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
          </select>
          <button
            @click="exportToCsv"
            class="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 transition flex items-center"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            Export CSV
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>

      <template v-else>
        <!-- Dashboard Stats -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div class="bg-white overflow-hidden shadow rounded-lg hover:shadow-md transition">
            <div class="p-5">
              <div class="flex items-center">
                <div class="flex-shrink-0 bg-blue-500 rounded-md p-3">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                  </svg>
                </div>
                <div class="ml-5 w-0 flex-1">
                  <dl>
                    <dt class="text-sm font-medium text-gray-500 truncate">Total Requests</dt>
                    <dd class="text-2xl font-bold text-gray-900">{{ stats.totalRequests }}</dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div class="bg-white overflow-hidden shadow rounded-lg hover:shadow-md transition">
            <div class="p-5">
              <div class="flex items-center">
                <div class="flex-shrink-0 bg-yellow-500 rounded-md p-3">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div class="ml-5 w-0 flex-1">
                  <dl>
                    <dt class="text-sm font-medium text-gray-500 truncate">Pending Requests</dt>
                    <dd class="text-2xl font-bold text-gray-900">{{ stats.pendingRequests }}</dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div class="bg-white overflow-hidden shadow rounded-lg hover:shadow-md transition">
            <div class="p-5">
              <div class="flex items-center">
                <div class="flex-shrink-0 bg-green-500 rounded-md p-3">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div class="ml-5 w-0 flex-1">
                  <dl>
                    <dt class="text-sm font-medium text-gray-500 truncate">Approved Requests</dt>
                    <dd class="text-2xl font-bold text-gray-900">{{ stats.approvedRequests }}</dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div class="bg-white overflow-hidden shadow rounded-lg hover:shadow-md transition">
            <div class="p-5">
              <div class="flex items-center">
                <div class="flex-shrink-0 bg-purple-500 rounded-md p-3">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                </div>
                <div class="ml-5 w-0 flex-1">
                  <dl>
                    <dt class="text-sm font-medium text-gray-500 truncate">Total Days Used</dt>
                    <dd class="text-2xl font-bold text-gray-900">{{ stats.totalDaysUsed }}</dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Monthly Leave Stats Chart -->
        <div class="bg-white shadow rounded-lg p-6 mb-8">
          <h2 class="text-lg font-medium text-gray-900 mb-6">Monthly Leave Statistics ({{ selectedYear }})</h2>
          <div class="grid grid-cols-12 gap-2 h-64">
            <div
              v-for="(month, index) in monthlyStats"
              :key="index"
              class="flex flex-col items-center justify-end"
            >
              <div class="relative w-full flex flex-col items-center justify-end h-48">
                <div 
                  class="w-8 bg-indigo-500 rounded-t transition-all duration-500 hover:bg-indigo-600"
                  :style="{ height: `${getBarHeight(month.count)}%`, minHeight: month.count > 0 ? '8px' : '0' }"
                  :title="`${month.name}: ${month.count} requests`"
                ></div>
                <span class="absolute -top-6 text-xs font-semibold text-gray-700" v-if="month.count > 0">{{ month.count }}</span>
              </div>
              <div class="text-xs mt-2 text-gray-600 font-medium">{{ month.name }}</div>
            </div>
          </div>
        </div>

        <!-- Status Distribution & Leave Type Stats -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
          <!-- Status Distribution -->
          <div class="bg-white shadow rounded-lg p-6">
            <h2 class="text-lg font-medium text-gray-900 mb-4">Request Status Distribution</h2>
            <div class="space-y-4">
              <div class="flex items-center">
                <div class="w-24 text-sm text-gray-600">Approved</div>
                <div class="flex-1 mx-4">
                  <div class="w-full bg-gray-200 rounded-full h-4">
                    <div
                      class="bg-green-500 h-4 rounded-full transition-all duration-500"
                      :style="{ width: `${getStatusPercentage(stats.approvedRequests)}%` }"
                    ></div>
                  </div>
                </div>
                <div class="w-20 text-right text-sm font-medium">{{ stats.approvedRequests }} ({{ getStatusPercentage(stats.approvedRequests).toFixed(0) }}%)</div>
              </div>
              <div class="flex items-center">
                <div class="w-24 text-sm text-gray-600">Pending</div>
                <div class="flex-1 mx-4">
                  <div class="w-full bg-gray-200 rounded-full h-4">
                    <div
                      class="bg-yellow-500 h-4 rounded-full transition-all duration-500"
                      :style="{ width: `${getStatusPercentage(stats.pendingRequests)}%` }"
                    ></div>
                  </div>
                </div>
                <div class="w-20 text-right text-sm font-medium">{{ stats.pendingRequests }} ({{ getStatusPercentage(stats.pendingRequests).toFixed(0) }}%)</div>
              </div>
              <div class="flex items-center">
                <div class="w-24 text-sm text-gray-600">Rejected</div>
                <div class="flex-1 mx-4">
                  <div class="w-full bg-gray-200 rounded-full h-4">
                    <div
                      class="bg-red-500 h-4 rounded-full transition-all duration-500"
                      :style="{ width: `${getStatusPercentage(stats.rejectedRequests)}%` }"
                    ></div>
                  </div>
                </div>
                <div class="w-20 text-right text-sm font-medium">{{ stats.rejectedRequests }} ({{ getStatusPercentage(stats.rejectedRequests).toFixed(0) }}%)</div>
              </div>
            </div>
            <!-- Pie Chart Representation -->
            <div class="mt-6 flex justify-center">
              <div class="relative w-32 h-32">
                <svg viewBox="0 0 36 36" class="w-32 h-32">
                  <path
                    :d="getDonutPath(0, getStatusPercentage(stats.approvedRequests))"
                    fill="none"
                    stroke="#22c55e"
                    stroke-width="4"
                  />
                  <path
                    :d="getDonutPath(getStatusPercentage(stats.approvedRequests), getStatusPercentage(stats.pendingRequests))"
                    fill="none"
                    stroke="#eab308"
                    stroke-width="4"
                  />
                  <path
                    :d="getDonutPath(getStatusPercentage(stats.approvedRequests) + getStatusPercentage(stats.pendingRequests), getStatusPercentage(stats.rejectedRequests))"
                    fill="none"
                    stroke="#ef4444"
                    stroke-width="4"
                  />
                </svg>
                <div class="absolute inset-0 flex items-center justify-center">
                  <span class="text-lg font-bold text-gray-700">{{ stats.totalRequests }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Leave Type Distribution -->
          <div class="bg-white shadow rounded-lg p-6">
            <h2 class="text-lg font-medium text-gray-900 mb-4">Leave by Type</h2>
            <div v-if="leaveTypeStats.length > 0" class="space-y-4">
              <div v-for="type in leaveTypeStats" :key="type.leaveType" class="flex items-center">
                <div class="w-28 text-sm text-gray-600 truncate" :title="type.leaveType">{{ type.leaveType }}</div>
                <div class="flex-1 mx-4">
                  <div class="w-full bg-gray-200 rounded-full h-4">
                    <div
                      class="h-4 rounded-full transition-all duration-500"
                      :class="getTypeColor(type.leaveType)"
                      :style="{ width: `${getTypePercentage(type.totalDays)}%` }"
                    ></div>
                  </div>
                </div>
                <div class="w-20 text-right text-sm font-medium">{{ type.totalDays }} days</div>
              </div>
            </div>
            <div v-else class="text-center text-gray-500 py-8">
              No leave data available for this year
            </div>
          </div>
        </div>

        <!-- Recent Leave Requests Table -->
        <div class="bg-white shadow rounded-lg p-6 mb-8">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-lg font-medium text-gray-900">Recent Leave Requests</h2>
            <router-link to="/leave-requests" class="text-indigo-600 hover:text-indigo-800 text-sm font-medium">
              View All →
            </router-link>
          </div>
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Employee</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Leave Type</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Duration</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Days</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="request in recentRequests" :key="request.id" class="hover:bg-gray-50">
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm font-medium text-gray-900">{{ request.employeeName || 'N/A' }}</div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span class="px-2 py-1 text-xs rounded-full" :class="getTypeColorBg(request.leaveTypeName)">
                      {{ request.leaveTypeName }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ formatDate(request.startDate) }} - {{ formatDate(request.endDate) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ calculateDays(request.startDate, request.endDate) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span
                      class="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full"
                      :class="getStatusColor(request.status)"
                    >
                      {{ request.status }}
                    </span>
                  </td>
                </tr>
                <tr v-if="recentRequests.length === 0">
                  <td colspan="5" class="px-6 py-8 text-center text-gray-500">
                    No leave requests found
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Leave Balances Summary -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-medium text-gray-900 mb-4">My Leave Balances</h2>
          <div v-if="leaveBalances.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div v-for="balance in leaveBalances" :key="balance.id" class="border rounded-lg p-4 hover:shadow-md transition">
              <div class="flex justify-between items-start mb-2">
                <h3 class="font-medium text-gray-900">{{ balance.leaveTypeName }}</h3>
                <span class="text-xs px-2 py-1 rounded-full" :class="getTypeColorBg(balance.leaveTypeName)">
                  {{ balance.year }}
                </span>
              </div>
              <div class="mt-3">
                <div class="flex justify-between text-sm mb-1">
                  <span class="text-gray-500">Used / Total</span>
                  <span class="font-medium">{{ balance.usedDays }} / {{ balance.totalDays }} days</span>
                </div>
                <div class="w-full bg-gray-200 rounded-full h-2">
                  <div
                    class="h-2 rounded-full transition-all duration-500"
                    :class="utilizationColor(balance.usedDays, balance.totalDays)"
                    :style="{ width: `${Math.min((balance.usedDays / balance.totalDays) * 100, 100)}%` }"
                  ></div>
                </div>
                <div class="flex justify-between text-xs mt-1">
                  <span class="text-gray-400">{{ Math.round((balance.usedDays / balance.totalDays) * 100) }}% used</span>
                  <span class="text-green-600 font-medium">{{ balance.remainingDays }} remaining</span>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="text-center text-gray-500 py-8">
            No leave balance data available
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()

const stats = ref({
  totalRequests: 0,
  pendingRequests: 0,
  approvedRequests: 0,
  rejectedRequests: 0,
  totalDaysUsed: 0
})

const monthlyStats = ref([])
const leaveTypeStats = ref([])
const leaveBalances = ref([])
const recentRequests = ref([])
const allLeaveRequests = ref([])
const selectedYear = ref(new Date().getFullYear())
const loading = ref(false)

const years = computed(() => {
  const currentYear = new Date().getFullYear()
  return [currentYear - 2, currentYear - 1, currentYear, currentYear + 1]
})

const maxMonthlyCount = computed(() => {
  return Math.max(...monthlyStats.value.map(m => m.count), 1)
})

const maxTypeDays = computed(() => {
  return Math.max(...leaveTypeStats.value.map(t => t.totalDays), 1)
})

const fetchAllData = async () => {
  loading.value = true
  try {
    await Promise.all([
      fetchLeaveRequests(),
      fetchLeaveBalances()
    ])
    computeStats()
    computeMonthlyStats()
    computeLeaveTypeStats()
  } catch (error) {
    console.error('Failed to fetch data:', error)
  } finally {
    loading.value = false
  }
}

const fetchLeaveRequests = async () => {
  try {
    // Try to fetch all requests (for admin/manager) or user's requests
    let response
    if (authStore.isManagerOrAdmin) {
      response = await api.get('/api/leaverequests')
    } else {
      response = await api.get(`/api/leaverequests/employee/${authStore.user?.id}`)
    }
    allLeaveRequests.value = response.data || []
    recentRequests.value = allLeaveRequests.value.slice(0, 10)
  } catch (error) {
    console.error('Failed to fetch leave requests:', error)
    allLeaveRequests.value = []
    recentRequests.value = []
  }
}

const fetchLeaveBalances = async () => {
  try {
    const response = await api.get('/api/leavebalance/my-balances')
    leaveBalances.value = response.data || []
  } catch (error) {
    console.error('Failed to fetch leave balances:', error)
    leaveBalances.value = []
  }
}

const computeStats = () => {
  const yearRequests = allLeaveRequests.value.filter(r => {
    const startDate = new Date(r.startDate)
    return startDate.getFullYear() === selectedYear.value
  })

  const pending = yearRequests.filter(r => r.status === 'Pending')
  const approved = yearRequests.filter(r => r.status === 'Approved')
  const rejected = yearRequests.filter(r => r.status === 'Rejected')

  let totalDays = 0
  approved.forEach(r => {
    totalDays += calculateDays(r.startDate, r.endDate)
  })

  stats.value = {
    totalRequests: yearRequests.length,
    pendingRequests: pending.length,
    approvedRequests: approved.length,
    rejectedRequests: rejected.length,
    totalDaysUsed: totalDays
  }
}

const computeMonthlyStats = () => {
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
  const monthlyCounts = new Array(12).fill(0)

  allLeaveRequests.value.forEach(r => {
    const startDate = new Date(r.startDate)
    if (startDate.getFullYear() === selectedYear.value) {
      const month = startDate.getMonth()
      monthlyCounts[month]++
    }
  })

  monthlyStats.value = months.map((name, index) => ({
    name,
    count: monthlyCounts[index]
  }))
}

const computeLeaveTypeStats = () => {
  const typeMap = {}

  allLeaveRequests.value.forEach(r => {
    const startDate = new Date(r.startDate)
    if (startDate.getFullYear() === selectedYear.value && r.status === 'Approved') {
      const typeName = r.leaveTypeName || 'Unknown'
      const days = calculateDays(r.startDate, r.endDate)
      
      if (!typeMap[typeName]) {
        typeMap[typeName] = 0
      }
      typeMap[typeName] += days
    }
  })

  leaveTypeStats.value = Object.entries(typeMap).map(([leaveType, totalDays]) => ({
    leaveType,
    totalDays
  })).sort((a, b) => b.totalDays - a.totalDays)
}

const refreshData = () => {
  computeStats()
  computeMonthlyStats()
  computeLeaveTypeStats()
}

const exportToCsv = () => {
  try {
    const yearRequests = allLeaveRequests.value.filter(r => {
      const startDate = new Date(r.startDate)
      return startDate.getFullYear() === selectedYear.value
    })

    if (yearRequests.length === 0) {
      alert('No data to export for the selected year')
      return
    }

    const headers = ['Employee', 'Leave Type', 'Start Date', 'End Date', 'Days', 'Status', 'Reason']
    const rows = yearRequests.map(r => [
      r.employeeName || 'N/A',
      r.leaveTypeName || 'N/A',
      formatDate(r.startDate),
      formatDate(r.endDate),
      calculateDays(r.startDate, r.endDate),
      r.status,
      r.reason || ''
    ])

    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
    ].join('\n')

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `leave-report-${selectedYear.value}.csv`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Failed to export:', error)
    alert('Failed to export report')
  }
}

const calculateDays = (startDate, endDate) => {
  const start = new Date(startDate)
  const end = new Date(endDate)
  const diffTime = Math.abs(end - start)
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1
  return diffDays
}

const formatDate = (dateStr) => {
  if (!dateStr) return 'N/A'
  const date = new Date(dateStr)
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

const getBarHeight = (count) => {
  if (count === 0) return 0
  return Math.max((count / maxMonthlyCount.value) * 100, 5)
}

const getStatusPercentage = (count) => {
  if (stats.value.totalRequests === 0) return 0
  return (count / stats.value.totalRequests) * 100
}

const getTypePercentage = (days) => {
  return (days / maxTypeDays.value) * 100
}

const getDonutPath = (startPercent, percent) => {
  if (percent === 0) return ''
  const start = startPercent / 100
  const end = (startPercent + percent) / 100
  const startAngle = start * 360 - 90
  const endAngle = end * 360 - 90
  const radius = 16
  const cx = 18
  const cy = 18
  
  const startRad = (startAngle * Math.PI) / 180
  const endRad = (endAngle * Math.PI) / 180
  
  const x1 = cx + radius * Math.cos(startRad)
  const y1 = cy + radius * Math.sin(startRad)
  const x2 = cx + radius * Math.cos(endRad)
  const y2 = cy + radius * Math.sin(endRad)
  
  const largeArc = percent > 50 ? 1 : 0
  
  return `M ${x1} ${y1} A ${radius} ${radius} 0 ${largeArc} 1 ${x2} ${y2}`
}

const getTypeColor = (type) => {
  const colors = {
    'Annual Leave': 'bg-blue-500',
    'Annual': 'bg-blue-500',
    'Sick Leave': 'bg-red-500',
    'Sick': 'bg-red-500',
    'Personal Leave': 'bg-green-500',
    'Personal': 'bg-green-500',
    'Maternity Leave': 'bg-pink-500',
    'Maternity': 'bg-pink-500',
    'Paternity Leave': 'bg-purple-500',
    'Paternity': 'bg-purple-500',
    'Unpaid Leave': 'bg-gray-500',
    'Unpaid': 'bg-gray-500'
  }
  return colors[type] || 'bg-indigo-500'
}

const getTypeColorBg = (type) => {
  const colors = {
    'Annual Leave': 'bg-blue-100 text-blue-800',
    'Annual': 'bg-blue-100 text-blue-800',
    'Sick Leave': 'bg-red-100 text-red-800',
    'Sick': 'bg-red-100 text-red-800',
    'Personal Leave': 'bg-green-100 text-green-800',
    'Personal': 'bg-green-100 text-green-800',
    'Maternity Leave': 'bg-pink-100 text-pink-800',
    'Maternity': 'bg-pink-100 text-pink-800',
    'Paternity Leave': 'bg-purple-100 text-purple-800',
    'Paternity': 'bg-purple-100 text-purple-800',
    'Unpaid Leave': 'bg-gray-100 text-gray-800',
    'Unpaid': 'bg-gray-100 text-gray-800'
  }
  return colors[type] || 'bg-indigo-100 text-indigo-800'
}

const getStatusColor = (status) => {
  const colors = {
    'Pending': 'bg-yellow-100 text-yellow-800',
    'Approved': 'bg-green-100 text-green-800',
    'Rejected': 'bg-red-100 text-red-800'
  }
  return colors[status] || 'bg-gray-100 text-gray-800'
}

const utilizationColor = (used, total) => {
  if (total === 0) return 'bg-gray-400'
  const percentage = (used / total) * 100
  if (percentage >= 90) return 'bg-red-500'
  if (percentage >= 70) return 'bg-yellow-500'
  return 'bg-green-500'
}

onMounted(() => {
  fetchAllData()
})
</script>