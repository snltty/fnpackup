<template>
    <Codemirror 
    :style="{ height: height,fontSize:'1.6rem' }"
    v-model="state.code" 
    :autofocus="state.options.autofocus"
    :indent-with-tab="state.options.indentWithTab"
    :tab-size="state.options.tabSize"
    :extensions="state.options.extensions"
    class="theme" ref="myEditor"></Codemirror>
</template>

<script>
import { computed, reactive, ref, watch } from 'vue';
import { Codemirror } from 'vue-codemirror'
import { javascript } from '@codemirror/lang-javascript'
import { oneDark } from '@codemirror/theme-one-dark'
import { useProjects } from '../list';
export default {

    components: { Codemirror },
    setup () {
        
        const projects = useProjects();
        const height = computed(() => {
            return `${window.innerHeight*0.7}px`
        });
        const state = reactive({
            code:projects.value.current.content,
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

        return {state,myEditor,height}
    }
}
</script>

<style lang="stylus" scoped>

</style>