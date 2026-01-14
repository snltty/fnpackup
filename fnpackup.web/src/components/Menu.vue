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
            <a href="javascript:;" @click="changeTheme('light')" v-if="state.theme == 'dark'"><el-icon size="20"><Moon /></el-icon></a>
            <a href="javascript:;" @click="changeTheme('dark')" v-else><el-icon size="20"><Sunny /></el-icon></a>
            <a href="https://github.com/snltty/fnpackup" target="_blank">
                <img v-if="state.theme == 'light'" src="../assets/github-light.svg" height="24" style="vertical-align: text-bottom;">
                <img v-else-if="state.theme == 'dark'" src="../assets/github-dark.svg" height="24" style="vertical-align: text-bottom;">
                <span>{{ state.version }}</span>
            </a>
            <a href="javascript:;">Snltty ©2026</a>
            <a href="https://linker.snltty.com" target="_blank">Linker</a>
        </div>
    </div>
</template>

<script>
import { fetchSystemVersion } from '@/api/api';
import { Moon, Sunny } from '@element-plus/icons-vue';
import { computed, onMounted, reactive} from 'vue';
import { useRouter } from 'vue-router';
export default {
    components:{Moon,Sunny},
    setup () {
        const router = useRouter();
        const options = computed(()=>router.options.routes);
  
        const state = reactive({
            theme: '',
            version: 'v0.0.0'
        });
        const changeTheme = (theme)=>{
            state.theme = theme;
            localStorage.setItem('fnos-theme', theme);
            setTheme();
        }
        const setTheme = ()=>{
            state.theme = localStorage.getItem('fnos-theme') || (isSystemDarkMode?'dark':'light')
            document.querySelector('html').setAttribute('class', state.theme);
        }

        onMounted(()=>{
            fetchSystemVersion().then(res=>{
                state.version = res;
            });
            setTheme();
        })

        return {options,state,changeTheme}
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
    font-size:1.3rem;
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