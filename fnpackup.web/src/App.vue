<template>
    <div class="flex flex-column flex-nowrap h-100 absolute">
        <div class="head">
            <Head></Head>
        </div>
        <div class="body flex-1 relative">
            <Body></Body>
        </div>
        <div class="foot">
            <Foot></Foot>
        </div>
    </div>
</template>

<script>
import Head from './components/Head.vue';
import Body from './components/Body.vue';
import Foot from './components/Foot.vue';
export default {
    name: 'App',
    components: {Head,Body,Foot},
    setup() {

        return {}
    },
}
</script>

<style lang="stylus">
</style>
