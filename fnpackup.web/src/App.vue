<template>
    <div class="flex flex-column flex-nowrap h-100 absolute">
        <div class="head" v-if="showHead">
            <Head></Head>
        </div>
        <div class="body flex-1 relative">
            <Body></Body>
        </div>
    </div>
</template>

<script>
import Head from './components/Head.vue';
import Body from './components/Body.vue';
import { computed } from 'vue';
export default {
    name: 'App',
    components: {Head,Body},
    setup() {

        const showHead = computed(()=>window.self === window.top);

        return {showHead}
    },
}
</script>

<style lang="stylus">
</style>
