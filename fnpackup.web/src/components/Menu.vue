<template>
    <div class="h-100 flex flex-column flex-nowrap menu-wrap">
        <ul class="flex-1">
            <template v-for="item in options">
                <li>
                    <router-link :to="item.path">
                        <el-icon size="16"><Guide /></el-icon>
                        <span>{{item.meta.title}}</span>
                    </router-link>
                </li>
            </template>
        </ul>
        <div class="foot-wrap">
            <a href="https://github.com/snltty/fnpackup" target="_blank">
                <img src="../assets/github.svg" height="16" style="opacity: 0.5;">
            </a>
            <a href="javascript:;">Snltty ©2025</a>
            <a href="https://linker.snltty.com" target="_blank">Linker</a>
        </div>
    </div>
</template>

<script>
import { computed} from 'vue';
import { Guide} from '@element-plus/icons-vue'
import { useRouter } from 'vue-router';
export default {
    components: {Guide},
    setup () {
        const router = useRouter();
        const options = computed(()=>router.options.routes);
        return {options}
    }
}
</script>

<style lang="stylus" scoped>
.menu-wrap{
    background-color:#f1f4f9;
    width: 140px;
}
li{
    padding:.5rem;
    a{
        display: block;
        font-size:0;
        padding:.8rem .5rem;
        border-radius:4px;

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
.foot-wrap{
    padding:1rem .6rem;
    font-size:1.2rem;
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