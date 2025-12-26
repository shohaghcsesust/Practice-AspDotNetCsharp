<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Reports & Analytics</h1>
        <div class="flex space-x-2">
          <button
            @click="exportToCsv"
            class="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 transition"
          >
            Export CSV
          </button>
        </div>
      </div>

      <!-- Dashboard Stats -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0 bg-blue-500 rounded-md p-3">
                <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Total Employees</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ stats.totalEmployees }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
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
                  <dd class="text-lg font-medium text-gray-900">{{ stats.pendingRequests }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0 bg-green-500 rounded-md p-3">
                <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Approved This Month</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ stats.approvedThisMonth }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
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
                  <dd class="text-lg font-medium text-gray-900">{{ stats.totalDaysUsed }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Monthly Leave Stats -->
      <div class="bg-white shadow rounded-lg p-6 mb-8">
        <h2 class="text-lg font-medium text-gray-900 mb-4">Monthly Leave Statistics</h2>
        <div class="flex items-center mb-4">
          <label class="mr-2 text-sm text-gray-600">Year:</label>
          <select
            v-model="selectedYear"
            @change="fetchMonthlyStats"
            class="border border-gray-300 rounded px-3 py-1"
          >
            <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
          </select>
        </div>
        <div class="grid grid-cols-12 gap-2">
          <div
            v-for="(month, index) in monthlyStats"
            :key="index"
            class="text-center"
          >
            <div
              class="h-24 rounded flex items-end justify-center"
              :style="{ backgroundColor: getBarColor(month.count) }"
            >
              <div
                class="bg-indigo-500 w-full rounded-t"
                :style="{ height: `${getBarHeight(month.count)}%` }"
              ></div>
            </div>
            <div class="text-xs mt-1">{{ month.name }}</div>
            <div class="text-xs font-medium">{{ month.count }}</div>
          </div>
        </div>
      </div>

      <!-- Department Stats -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-medium text-gray-900 mb-4">Leave by Department</h2>
          <div class="space-y-4">
            <div v-for="dept in departmentStats" :key="dept.department" class="flex items-center">
              <div class="w-24 text-sm text-gray-600">{{ dept.department }}</div>
              <div class="flex-1 mx-4">
                <div class="w-full bg-gray-200 rounded-full h-4">
                  <div
                    class="bg-indigo-600 h-4 rounded-full"
                    :style="{ width: `${getDeptPercentage(dept.totalDays)}%` }"
                  ></div>
                </div>
              </div>
              <div class="w-16 text-right text-sm font-medium">{{ dept.totalDays }} days</div>
            </div>
          </div>
        </div>

        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-medium text-gray-900 mb-4">Leave by Type</h2>
          <div class="space-y-4">
            <div v-for="type in leaveTypeStats" :key="type.leaveType" class="flex items-center">
              <div class="w-24 text-sm text-gray-600">{{ type.leaveType }}</div>
              <div class="flex-1 mx-4">
                <div class="w-full bg-gray-200 rounded-full h-4">
                  <div
                    class="h-4 rounded-full"
                    :class="getTypeColor(type.leaveType)"
                    :style="{ width: `${getTypePercentage(type.totalDays)}%` }"
                  ></div>
                </div>
              </div>
              <div class="w-16 text-right text-sm font-medium">{{ type.totalDays }} days</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Leave Balance Report -->
      <div class="bg-white shadow rounded-lg p-6">
        <h2 class="text-lg font-medium text-gray-900 mb-4">Employee Leave Balance Report</h2>
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Employee</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Department</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total Days</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Used</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Remaining</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Utilization</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="balance in balanceReport" :key="balance.employeeId">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ balance.employeeName }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ balance.department }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ balance.totalDays }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ balance.usedDays }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ balance.remainingDays }}</td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <div class="w-16 bg-gray-200 rounded-full h-2 mr-2">
                      <div
                        class="h-2 rounded-full"
                        :class="utilizationColor(balance.usedDays, balance.totalDays)"
                        :style="{ width: `${Math.min((balance.usedDays / balance.totalDays) * 100, 100)}%` }"
                      ></div>
                    </div>
                    <span class="text-xs">{{ Math.round((balance.usedDays / balance.totalDays) * 100) }}%</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '../services/api'

const stats = ref({
  totalEmployees: 0,
  pendingRequests: 0,
  approvedThisMonth: 0,
  totalDaysUsed: 0
})

const monthlyStats = ref([])
const departmentStats = ref([])
const leaveTypeStats = ref([])
const balanceReport = ref([])
const selectedYear = ref(new Date().getFullYear())
const loading = ref(false)

const years = computed(() => {
  const currentYear = new Date().getFullYear()
  return [currentYear - 2, currentYear - 1, currentYear, currentYear + 1]
})

const maxDeptDays = computed(() => {
  return Math.max(...departmentStats.value.map(d => d.totalDays), 1)
})

const maxTypeDays = computed(() => {
  return Math.max(...leaveTypeStats.value.map(t => t.totalDays), 1)
})

const maxMonthlyCount = computed(() => {
  return Math.max(...monthlyStats.value.map(m => m.count), 1)
})

const fetchStats = async () => {
  loading.value = true
  try {
    const response = await api.get('/api/reports/dashboard')
    stats.value = response.data
  } catch (error) {
    console.error('Failed to fetch stats:', error)
  }
  loading.value = false
}

const fetchMonthlyStats = async () => {
  try {
    const response = await api.get(`/api/reports/monthly?year=${selectedYear.value}`)
    monthlyStats.value = response.data
  } catch (error) {
    console.error('Failed to fetch monthly stats:', error)
    // Default empty data
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
    monthlyStats.value = months.map(name => ({ name, count: 0 }))
  }
}

const fetchDepartmentStats = async () => {
  try {
    const response = await api.get(`/api/reports/department?year=${selectedYear.value}`)
    departmentStats.value = response.data
  } catch (error) {
    console.error('Failed to fetch department stats:', error)
  }
}

const fetchLeaveTypeStats = async () => {
  try {
    const response = await api.get(`/api/reports/leave-types?year=${selectedYear.value}`)
    leaveTypeStats.value = response.data
  } catch (error) {
    console.error('Failed to fetch leave type stats:', error)
  }
}

const fetchBalanceReport = async () => {
  try {
    const response = await api.get(`/api/reports/balances?year=${selectedYear.value}`)
    balanceReport.value = response.data
  } catch (error) {
    console.error('Failed to fetch balance report:', error)
  }
}

const exportToCsv = async () => {
  try {
    const response = await api.post('/api/reports/export/leaves', {
      year: selectedYear.value
    }, {
      responseType: 'blob'
    })
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `leave-report-${selectedYear.value}.csv`)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch (error) {
    console.error('Failed to export:', error)
    alert('Failed to export report')
  }
}

const getBarHeight = (count) => {
  return (count / maxMonthlyCount.value) * 100
}

const getBarColor = () => {
  return '#e5e7eb'
}

const getDeptPercentage = (days) => {
  return (days / maxDeptDays.value) * 100
}

const getTypePercentage = (days) => {
  return (days / maxTypeDays.value) * 100
}

const getTypeColor = (type) => {
  const colors = {
    'Annual': 'bg-blue-500',
    'Sick': 'bg-red-500',
    'Personal': 'bg-green-500',
    'Maternity': 'bg-pink-500',
    'Paternity': 'bg-purple-500'
  }
  return colors[type] || 'bg-gray-500'
}

const utilizationColor = (used, total) => {
  const percentage = (used / total) * 100
  if (percentage >= 90) return 'bg-red-500'
  if (percentage >= 70) return 'bg-yellow-500'
  return 'bg-green-500'
}

onMounted(() => {
  fetchStats()
  fetchMonthlyStats()
  fetchDepartmentStats()
  fetchLeaveTypeStats()
  fetchBalanceReport()
})
</script>