import { Pointer, Service } from '@element-plus/icons-vue'
import { createRouter, createWebHashHistory } from 'vue-router'
const routes = [
    {
        path: '/',
        name: 'Index',
        meta: { title: '打包fpk',icon:Pointer },
        component: () => import('@/views/index/Index.vue'),
    },
    {
        path: '/static',
        name: 'Static',
        meta: { title: '静态托管',icon:Service },
        component: () => import('@/views/static/Index.vue'),
    },
    {
        path: '/static-view',
        name: 'StaticView',
        component: () => import('@/views/static/View.vue'),
    }
]

const router = createRouter({
    history: createWebHashHistory(),
    routes
})

export default router
