<template>
    <div class="h-100 flex flex-column flex-nowrap">
        <ul>
            <template v-for="item in options">
                <li>
                    <router-link :to="item.path">
                        <el-icon size="16"><HomeFilled /></el-icon>
                        <span>{{item.meta.title}}</span>
                    </router-link>
                </li>
            </template>
        </ul>
    </div>
</template>

<script>
import { computed} from 'vue';
import {HomeFilled} from '@element-plus/icons-vue'
import { useRouter } from 'vue-router';
export default {
    components: {HomeFilled},
    setup () {
        const router = useRouter();
        const options = computed(()=>router.options.routes);
        return {options}
    }
}
</script>

<style lang="stylus" scoped>
li{
    a{
        display: block;
        font-size:0;
        padding:0 1rem 0 1rem;
        line-height:3.7rem;
        height: 3.7rem;

        &.router-link-active,&:hover{
            background-color:#f6f8fa;
            box-shadow: 0 0 5px rgba(0,0,0,0.05) inset;
        }
        .el-icon,span{
            vertical-align: middle;
            font-size:1.4rem;
        }

    }
}
</style>