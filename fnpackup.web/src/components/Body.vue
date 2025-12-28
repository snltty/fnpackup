<template>
    <div class="body-wrap h-100 absolute flex flex-nowrap">
        <div>
            <Menu></Menu>
        </div>
        <div class="flex-1 inner">
            <router-view></router-view>
        </div>
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
    background-color: #f1f4f9;
    .inner{
        background-color: #fff;
        border-radius:1.2rem;
    }
}
</style>