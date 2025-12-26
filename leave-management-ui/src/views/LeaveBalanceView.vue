<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <h1 class="text-3xl font-bold text-gray-900 mb-8">Leave Balance</h1>
      
      <div v-if="leaveStore.loading" class="text-center py-8">Loading...</div>
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div 
          v-for="balance in leaveStore.leaveBalances" 
          :key="balance.id"
          class="bg-white shadow rounded-lg overflow-hidden"
        >
          <div class="p-6">
            <h3 class="text-lg font-medium text-gray-900 mb-4">{{ balance.leaveTypeName }}</h3>
            
            <!-- Progress Bar -->
            <div class="mb-4">
              <div class="flex justify-between text-sm text-gray-600 mb-1">
                <span>Used</span>
                <span>{{ balance.usedDays }} / {{ balance.totalDays }} days</span>
              </div>
              <div class="w-full bg-gray-200 rounded-full h-2.5">
                <div 
                  class="h-2.5 rounded-full"
                  :class="progressColor(balance.usedDays, balance.totalDays)"
                  :style="{ width: `${Math.min((balance.usedDays / balance.totalDays) * 100, 100)}%` }"
                ></div>
              </div>
            </div>
            
            <div class="space-y-2">
              <div class="flex justify-between">
                <span class="text-gray-500">Total Allocation:</span>
                <span class="font-medium">{{ balance.totalDays }} days</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-500">Days Used:</span>
                <span class="font-medium text-red-600">{{ balance.usedDays }} days</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-500">Days Remaining:</span>
                <span class="font-medium text-green-600">{{ balance.remainingDays }} days</span>
              </div>
              <div v-if="balance.carryForwardDays > 0" class="flex justify-between">
                <span class="text-gray-500">Carry Forward:</span>
                <span class="font-medium text-blue-600">{{ balance.carryForwardDays }} days</span>
              </div>
            </div>
          </div>
          
          <div class="bg-gray-50 px-6 py-3">
            <router-link 
              :to="{ path: '/leave-requests/new', query: { type: balance.leaveTypeId } }"
              class="text-indigo-600 hover:text-indigo-900 text-sm font-medium"
            >
              Request Leave →
            </router-link>
          </div>
        </div>
      </div>
      
      <div v-if="!leaveStore.loading && leaveStore.leaveBalances.length === 0" 
        class="text-center py-8 text-gray-500 bg-white shadow rounded-lg"
      >
        No leave balances found. Please contact HR.
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useLeaveStore } from '../stores/leave'

const leaveStore = useLeaveStore()

onMounted(() => {
  leaveStore.fetchLeaveBalances()
})

const progressColor = (used, total) => {
  const percentage = (used / total) * 100
  if (percentage >= 90) return 'bg-red-500'
  if (percentage >= 70) return 'bg-yellow-500'
  return 'bg-green-500'
}
</script>
