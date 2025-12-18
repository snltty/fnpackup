<template>
    <div class="body-wrap h-100 absolute">
        <el-splitter>
            <el-splitter-panel :max="100" :min="100" size="100">
                <Menu></Menu>
            </el-splitter-panel>
            <el-splitter-panel>
                 <router-view></router-view>
            </el-splitter-panel>
        </el-splitter>
    </div>
</template>

<script>
import Menu from './Menu.vue';

export default {
    components: { Menu},
    setup () {
    
        return {}
    }
}
</script>

<style lang="stylus" scoped>
.body-wrap{
    background-color: #fff;
}
</style>