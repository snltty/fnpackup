<template>
    <div class="flex flex-column flex-nowrap h-100 absolute" v-if="showApp">
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
import { computed, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
export default {
    name: 'App',
    components: {Head,Body},
    setup() {

        const router = useRouter();
        const route = useRoute();
        const showHead = computed(()=>window.self === window.top);
        const showApp = ref(false);
        router.isReady().then(()=>{
            for(let key in route.query){
                document.cookie = `${key}=${route.query[key]}; path=/;`;
                localStorage.setItem(key,route.query[key]);
            }
            if(route.query['fnos-theme']){
                window.location = '/';
            }
            showApp.value = true;
        });

        return {showHead,showApp}
    },
}
</script>

<style lang="stylus">
</style>
