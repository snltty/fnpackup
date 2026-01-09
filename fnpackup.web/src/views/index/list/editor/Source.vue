<template>
    <div class="source-wrap h-100 relative">
        <div class="absolute scrollbar">
            <Codemirror 
            :style="{height:'100%', fontSize:'1.6rem' }"
            v-model="state.code" 
            :autofocus="state.options.autofocus"
            :indent-with-tab="state.options.indentWithTab"
            :tab-size="state.options.tabSize"
            :extensions="state.options.extensions"
            class="theme" ref="myEditor"></Codemirror>
        </div>
    </div>
</template>

<script>
import {  nextTick, onMounted, reactive, ref } from 'vue';
import { Codemirror } from 'vue-codemirror'
import { json } from '@codemirror/lang-json'
import { yaml } from '@codemirror/lang-yaml'
import { javascript } from '@codemirror/lang-javascript'
import { oneDark } from '@codemirror/theme-one-dark'
export default {
    match:/.*/,
    width:600,
    components: { Codemirror },
    props:['content','path'],
    setup (props) {
        const extensions = [oneDark];
        if(props.path.endsWith('.yaml')){
            extensions.splice(0,0,yaml());
        }else if(props.content.startsWith('#!/bin/bash')){
            extensions.splice(0,0,javascript());
        }else{
            extensions.splice(0,0,json());
        }
        const state = reactive({
            code:props.content,
            options: {
                tabSize: 2,
                autofocus: true,
                indentWithTab:true,
                extensions:extensions
            }
        });
        const myEditor = ref(null);  

        const getContent = () => {
            return new Promise((resolve,reject)=>{ 
                resolve({
                    path:props.path,
                    content:state.code
                });
            });
        }
        return {state,myEditor,getContent}
    }
}
</script>
<style lang="stylus">
.v-codemirror {
  border: 1px solid var(--main-border-color);;
  height: auto;
}
.cm-editor,.cm-scroller{
    border-radius:5px;
}
</style>
<style lang="stylus" scoped>
</style>