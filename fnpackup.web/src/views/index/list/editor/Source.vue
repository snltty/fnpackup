<template>
    <Codemirror 
    :style="{ height: state.height,fontSize:'1.6rem' }"
    v-model="state.code" 
    :autofocus="state.options.autofocus"
    :indent-with-tab="state.options.indentWithTab"
    :tab-size="state.options.tabSize"
    :extensions="state.options.extensions"
    class="theme" ref="myEditor"></Codemirror>
</template>

<script>
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { Codemirror } from 'vue-codemirror'
import { javascript } from '@codemirror/lang-javascript'
import { oneDark } from '@codemirror/theme-one-dark'
import { useProjects } from '../list';
export default {

    components: { Codemirror },
    setup () {
        
        const projects = useProjects();
        const state = reactive({
            code:projects.value.current.content,
            height:`${window.innerHeight*0.7}px`,
            options: {
                tabSize: 4,
                autofocus: true,
                indentWithTab:true,
                extensions:[javascript(), oneDark]
            },
        });
        watch(()=>projects.value.current.content,()=>{
            state.code = projects.value.current.content;
        })
        const myEditor = ref(null);

        const resize = () => {
            state.height = `${window.innerHeight*0.7}px`;
        }
        onMounted(()=>{
            window.addEventListener('resize', resize);
        });
        onUnmounted(()=>{
            window.removeEventListener('resize', resize);
        });

        return {state,myEditor}
    }
}
</script>

<style lang="stylus" scoped>

</style>