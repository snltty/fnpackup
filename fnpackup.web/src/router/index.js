import { createRouter, createWebHashHistory } from 'vue-router'
const routes = [
    {
        path: '/',
        name: 'Index',
        meta: { title: '编辑器' },
        component: () => import('@/views/index/Index.vue'),
       
    }
]

const router = createRouter({
    history: createWebHashHistory(),
    routes
})

export default router
