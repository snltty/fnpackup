<template>
    <div class="source-wrap">
        <Codemirror 
        :style="{height:state.height, fontSize:'1.6rem' }"
        v-model="state.code" 
        :autofocus="state.options.autofocus"
        :indent-with-tab="state.options.indentWithTab"
        :tab-size="state.options.tabSize"
        :extensions="state.options.extensions"
        class="theme" ref="myEditor"></Codemirror>
        <div style="height:6px;"></div>
        <div class="btns t-c">
            <el-button type="primary" @click="handleSubmit" :loading="state.loading">确定保存</el-button>
        </div>
    </div>
</template>

<script>
import {  nextTick, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { Codemirror } from 'vue-codemirror'
import { json } from '@codemirror/lang-json'
import { yaml } from '@codemirror/lang-yaml'
import { javascript } from '@codemirror/lang-javascript'
import { oneDark } from '@codemirror/theme-one-dark'
import { useProjects } from '../list';
import { fetchApi } from '@/api/api';
import { useLogger } from '../../logger';
export default {
    match:/.*/,
    width:600,
    components: { Codemirror },
    setup () {
        

        const logger = useLogger();
        const projects = useProjects();

        const extensions = [oneDark];
        if(projects.value.current.path.endsWith('.yaml')){
            extensions.splice(0,0,yaml());
        }else if(projects.value.current.content.startsWith('#!/bin/bash')){
            extensions.splice(0,0,javascript());
        }else{
            extensions.splice(0,0,json());
        }
        const state = reactive({
            code:projects.value.current.content,
            height:`${window.innerHeight-300}px`,
            options: {
                tabSize: 2,
                autofocus: true,
                indentWithTab:true,
                extensions:extensions
            },
            loading:false
        });
        watch(()=>projects.value.current.content,()=>{
            state.code = projects.value.current.content;
        })
        const myEditor = ref(null);
        const handleCancel = ()=>{
            projects.value.current.show = false;
        }
        const handleSubmit = ()=>{
            state.loading = true;
            fetchApi(`/files/write`,{
                method:'POST',
                headers:{'Content-Type':'application/json'},
                body:JSON.stringify({
                    path:projects.value.current.path,
                    content:state.code
                })
            }).then(res => res.text()).then(res => {
                state.loading = false;
                if(res){
                    logger.value.error(res);
                }else{
                    state.show = false;
                    logger.value.success(`保存成功`);
                    projects.value.load();
                }
            }).catch((e)=>{
                state.loading = false;
                logger.value.error(`${e}`);
            })
        }

        const resize = () => {
            state.height = `${window.innerHeight-300}px`;
        }
        onMounted(()=>{
            window.addEventListener('resize', resize);
            nextTick(()=>{
                resize();
            })
        });
        onUnmounted(()=>{
            window.removeEventListener('resize', resize);
        });

        return {state,myEditor,handleCancel,handleSubmit}
    }
}
</script>
<style lang="stylus">
.v-codemirror {
  border: 1px solid #eee;
  height: auto;
}
</style>
<style lang="stylus" scoped>
.source-wrap{height:50%}
</style>