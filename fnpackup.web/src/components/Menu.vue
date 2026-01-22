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
            <a href="javascript:;" @click="changeTheme('light')" v-if="state.theme == 'dark'"><el-icon size="18"><Moon /></el-icon></a>
            <a href="javascript:;" @click="changeTheme('dark')" v-else><el-icon size="18"><Sunny /></el-icon></a>
            <a href="javascript:;" @click="handlePay">
                <img src="../assets/money.svg" class="img-big">
                <span>慷慨赞助</span>
            </a>
            <a href="https://linker.snltty.com" target="_blank">
                <img src="../assets/github.svg" class="img">
                <span>开源组网</span>
            </a>
            <a href="https://tun324.snltty.com/" target="_blank">
                <img src="../assets/github.svg" class="img">
                <span>tun转代理</span>
            </a>
            <a href="https://github.com/snltty/fnpackup" target="_blank">
                <img src="../assets/github.svg" class="img">
                <span>在线fpk {{ state.version }}</span>
            </a>
        </div>
    </div>
    <el-dialog v-model="state.showPay" title="慷慨赞助" width="350">
        <div class="pay">
            <p class="t-c mgb-1">
                程序本身完全开源免费
            </p>
            <p class="t-c mgb-1">
                但也可以作者帮忙打包，￥30/次，OR，纯慷慨赞助
            </p>
            <p>
                <img src="pay.png" alt="pay" width="100%"/>
            </p>
        </div>
    </el-dialog>
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
            version: 'v0.0.0',
            showPay:false
        });
        const changeTheme = (theme)=>{
            state.theme = theme;
            localStorage.setItem('fnos-theme', theme);
            setTheme();
        }
        const setTheme = ()=>{
             const isSystemDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;
            state.theme = localStorage.getItem('fnos-theme') || (isSystemDarkMode?'dark':'light')
            document.querySelector('html').setAttribute('class', state.theme);
        }
        const handlePay = ()=>{
            state.showPay = true;
        }

        onMounted(()=>{
            fetchSystemVersion().then(res=>{
                state.version = res;
            });
            setTheme();
        })

        return {options,state,changeTheme,handlePay}
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
            .img-big{
                &+span{color:#03ff03;}
            }
        }
    }
}
.menu-wrap{
    width: 15rem;
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
                font-size:1.3rem;
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
        display:flex;
        align-items: center;
        &:hover{
            background-color:#fff;
            color:#2568ed
        }
        .img{
            width:1.8rem;
            height:1.8rem;
        }
        span{margin-left:.5rem;}
        .img-big{
            width:2.6rem;
            height:2.6rem;

            &+span{font-size:1.5rem;color:green;}
        }
    }
}
</style>