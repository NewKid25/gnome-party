import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createWebHistory, createRouter } from 'vue-router'

import App from './App.vue'
import ParticipantView from './participant-view/ParticipantView.vue'
import HostView from './host-view/HostView.vue'

const routes = [
    { path: '/', component: ParticipantView },
    { path: '/host', component: HostView},
]

export const router = createRouter({
    history: createWebHistory(),
    routes,
})

const app = createApp(App)

// app.use(createPinia())
app.use(router)
app.mount('#app')
