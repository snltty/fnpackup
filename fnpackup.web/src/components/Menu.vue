<template>
    <div class="h-100 flex flex-column flex-nowrap menu-wrap">
        <ul class="flex-1">
            <template v-for="item in options">
                <li v-if="item.meta">
                    <router-link :to="item.path">
                        <el-icon size="16">
                            <component :is="item.meta.icon"></component>
                        </el-icon>
                        <span>{{item.meta.title}}</span>
                    </router-link>
                </li>
            </template>
        </ul>
        <div class="foot-wrap">
            <a href="javascript:;" @click="changeMode('light')" v-if="state.mode == 'dark'"><el-icon size="20"><Moon /></el-icon></a>
            <a href="javascript:;" @click="changeMode('dark')" v-else><el-icon size="20"><Sunny /></el-icon></a>
            <a href="https://github.com/snltty/fnpackup" target="_blank">
                <img v-if="state.mode == 'light'" src="../assets/github-light.svg" height="24" style="vertical-align: text-bottom;">
                <img v-else-if="state.mode == 'dark'" src="../assets/github-dark.svg" height="24" style="vertical-align: text-bottom;">
                <span>{{ state.version }}</span>
            </a>
            <a href="javascript:;">Snltty ©2025</a>
            <a href="https://linker.snltty.com" target="_blank">Linker</a>
        </div>
    </div>
</template>

<script>
import { fetchApi } from '@/api/api';
import { Moon, Sunny } from '@element-plus/icons-vue';
import { computed, onMounted, reactive} from 'vue';
import { useRouter } from 'vue-router';
export default {
    components:{Moon,Sunny},
    setup () {
        const router = useRouter();
        const options = computed(()=>router.options.routes);

        const isSystemDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const isSystemLightMode = window.matchMedia('(prefers-color-scheme: light)').matches;
        const cacheMode = localStorage.getItem('theme-mode') || (isSystemDarkMode?'dark':'light');
        const state = reactive({
            mode: cacheMode,
            version: 'v0.0.0'
        });
        const changeMode = (mode)=>{
            state.mode = mode;
            localStorage.setItem('theme-mode', mode);
            setMode();
        }
        const setMode = ()=>{
            document.querySelector('html').setAttribute('class', state.mode);
        }

        onMounted(()=>{
            setMode();
            fetchApi('/files/version').then(c=>c.text()).then(res=>{
                state.version = res;
            })
        })

        return {options,state,changeMode}
    }
}
</script>

<style lang="stylus" scoped>
html.dark{
    .menu-wrap{
        a{
            color:#cbd5e0;
            &.router-link-active,&:hover{
                background-color:#1a1e23;
            }
        }
    }
}
.menu-wrap{
    width: 140px;
}
ul{
    padding:.8rem;
    li{
        a{
            display: block;
            font-size:0;
            padding:.8rem .5rem;
            border-radius:6px;

            &.router-link-active,&:hover{
                background-color:#fff;
                color:#2173df;
                font-weight:500;
            }
            .el-icon,span{
                vertical-align: middle;
                font-size:1.4rem;
            }
            .el-icon{
                margin-right:.6rem;
            }

        }
    }
}

.foot-wrap{
    padding:1rem .6rem;
    font-size:1.4rem;
    a{
        display:block;padding:.6rem;
        border-radius:4px;
        &:hover{
            background-color:#fff;
            color:#2568ed
        }
    }
}
</style>