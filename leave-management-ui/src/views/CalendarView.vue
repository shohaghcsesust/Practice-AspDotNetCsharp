<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <h1 class="text-3xl font-bold text-gray-900 mb-8">Leave Calendar</h1>
      
      <!-- Calendar Navigation -->
      <div class="bg-white shadow rounded-lg p-4 mb-6">
        <div class="flex items-center justify-between">
          <button 
            @click="previousMonth"
            class="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-md transition"
          >
            ← Previous
          </button>
          <h2 class="text-xl font-semibold">
            {{ monthNames[currentMonth] }} {{ currentYear }}
          </h2>
          <button 
            @click="nextMonth"
            class="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-md transition"
          >
            Next →
          </button>
        </div>
      </div>
      
      <!-- Legend -->
      <div class="bg-white shadow rounded-lg p-4 mb-6">
        <div class="flex flex-wrap gap-4 text-sm">
          <div class="flex items-center">
            <span class="w-4 h-4 bg-green-500 rounded mr-2"></span>
            <span>Approved Leave</span>
          </div>
          <div class="flex items-center">
            <span class="w-4 h-4 bg-yellow-500 rounded mr-2"></span>
            <span>Pending Leave</span>
          </div>
          <div class="flex items-center">
            <span class="w-4 h-4 bg-red-500 rounded mr-2"></span>
            <span>Public Holiday</span>
          </div>
        </div>
      </div>
      
      <!-- Calendar Grid -->
      <div class="bg-white shadow rounded-lg overflow-hidden">
        <div class="grid grid-cols-7 bg-gray-50">
          <div 
            v-for="day in weekDays" 
            :key="day"
            class="py-3 text-center text-sm font-semibold text-gray-700 border-b"
          >
            {{ day }}
          </div>
        </div>
        
        <div class="grid grid-cols-7">
          <div 
            v-for="(day, index) in calendarDays" 
            :key="index"
            class="min-h-24 p-2 border-b border-r"
            :class="{ 
              'bg-gray-50': !day.currentMonth,
              'bg-blue-50': day.isToday
            }"
          >
            <div class="text-sm font-medium" :class="{ 'text-gray-400': !day.currentMonth }">
              {{ day.date }}
            </div>
            
            <div v-if="day.events.length > 0" class="mt-1 space-y-1">
              <div 
                v-for="event in day.events.slice(0, 3)" 
                :key="event.id"
                @click="showEventDetails(event)"
                class="text-xs px-1 py-0.5 rounded truncate cursor-pointer"
                :class="eventClass(event)"
              >
                {{ event.title }}
              </div>
              <div 
                v-if="day.events.length > 3"
                class="text-xs text-gray-500"
              >
                +{{ day.events.length - 3 }} more
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Event Details Modal -->
    <div v-if="selectedEvent" class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full">
        <h3 class="text-lg font-medium text-gray-900 mb-4">{{ selectedEvent.title }}</h3>
        <div class="space-y-2 text-sm">
          <div v-if="selectedEvent.employeeName">
            <span class="text-gray-500">Employee:</span>
            <span class="ml-2">{{ selectedEvent.employeeName }}</span>
          </div>
          <div>
            <span class="text-gray-500">Type:</span>
            <span class="ml-2">{{ selectedEvent.type }}</span>
          </div>
          <div>
            <span class="text-gray-500">Start:</span>
            <span class="ml-2">{{ formatDate(selectedEvent.startDate) }}</span>
          </div>
          <div>
            <span class="text-gray-500">End:</span>
            <span class="ml-2">{{ formatDate(selectedEvent.endDate) }}</span>
          </div>
          <div v-if="selectedEvent.status">
            <span class="text-gray-500">Status:</span>
            <span 
              :class="statusClass(selectedEvent.status)"
              class="ml-2 px-2 py-0.5 rounded-full text-xs"
            >
              {{ selectedEvent.status }}
            </span>
          </div>
        </div>
        <div class="mt-6 flex justify-end">
          <button 
            @click="selectedEvent = null"
            class="bg-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-400"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import api from '../services/api'

const currentMonth = ref(new Date().getMonth())
const currentYear = ref(new Date().getFullYear())
const events = ref([])
const holidays = ref([])
const loading = ref(false)
const selectedEvent = ref(null)

const weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
const monthNames = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December'
]

const calendarDays = computed(() => {
  const days = []
  const firstDay = new Date(currentYear.value, currentMonth.value, 1)
  const lastDay = new Date(currentYear.value, currentMonth.value + 1, 0)
  const startPadding = firstDay.getDay()
  const today = new Date()
  
  // Previous month padding
  for (let i = startPadding - 1; i >= 0; i--) {
    const date = new Date(currentYear.value, currentMonth.value, -i)
    days.push({
      date: date.getDate(),
      fullDate: date,
      currentMonth: false,
      isToday: false,
      events: getEventsForDate(date)
    })
  }
  
  // Current month days
  for (let i = 1; i <= lastDay.getDate(); i++) {
    const date = new Date(currentYear.value, currentMonth.value, i)
    days.push({
      date: i,
      fullDate: date,
      currentMonth: true,
      isToday: date.toDateString() === today.toDateString(),
      events: getEventsForDate(date)
    })
  }
  
  // Next month padding
  const remaining = 42 - days.length
  for (let i = 1; i <= remaining; i++) {
    const date = new Date(currentYear.value, currentMonth.value + 1, i)
    days.push({
      date: i,
      fullDate: date,
      currentMonth: false,
      isToday: false,
      events: getEventsForDate(date)
    })
  }
  
  return days
})

const getEventsForDate = (date) => {
  const dateStr = date.toISOString().split('T')[0]
  const allEvents = []
  
  // Add leave events
  events.value.forEach(event => {
    const start = new Date(event.startDate)
    const end = new Date(event.endDate)
    if (date >= start && date <= end) {
      allEvents.push(event)
    }
  })
  
  // Add holidays
  holidays.value.forEach(holiday => {
    const holidayDate = new Date(holiday.date)
    if (holidayDate.toDateString() === date.toDateString()) {
      allEvents.push({
        id: `holiday-${holiday.id}`,
        title: holiday.name,
        type: 'Holiday',
        startDate: holiday.date,
        endDate: holiday.date,
        isHoliday: true
      })
    }
  })
  
  return allEvents
}

const fetchCalendarData = async () => {
  loading.value = true
  try {
    const startDate = new Date(currentYear.value, currentMonth.value, 1).toISOString()
    const endDate = new Date(currentYear.value, currentMonth.value + 1, 0).toISOString()
    
    const [eventsRes, holidaysRes] = await Promise.all([
      api.get(`/api/calendar?startDate=${startDate}&endDate=${endDate}`),
      api.get(`/api/publicholidays?year=${currentYear.value}`)
    ])
    
    events.value = eventsRes.data
    holidays.value = holidaysRes.data
  } catch (error) {
    console.error('Failed to fetch calendar data:', error)
  }
  loading.value = false
}

const previousMonth = () => {
  if (currentMonth.value === 0) {
    currentMonth.value = 11
    currentYear.value--
  } else {
    currentMonth.value--
  }
}

const nextMonth = () => {
  if (currentMonth.value === 11) {
    currentMonth.value = 0
    currentYear.value++
  } else {
    currentMonth.value++
  }
}

const eventClass = (event) => {
  if (event.isHoliday) return 'bg-red-100 text-red-800'
  if (event.status === 'Approved') return 'bg-green-100 text-green-800'
  if (event.status === 'Pending') return 'bg-yellow-100 text-yellow-800'
  return 'bg-gray-100 text-gray-800'
}

const statusClass = (status) => {
  switch (status) {
    case 'Approved': return 'bg-green-100 text-green-800'
    case 'Rejected': return 'bg-red-100 text-red-800'
    case 'Pending': return 'bg-yellow-100 text-yellow-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}

const showEventDetails = (event) => {
  selectedEvent.value = event
}

const formatDate = (dateStr) => {
  return new Date(dateStr).toLocaleDateString()
}

watch([currentMonth, currentYear], () => {
  fetchCalendarData()
})

onMounted(() => {
  fetchCalendarData()
})
</script>
