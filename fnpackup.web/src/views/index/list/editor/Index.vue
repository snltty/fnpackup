<template>
    <el-dialog v-model="state.show" :title="`编辑器[${projects.editor.remark}]`" 
    :width="`${projects.editor.width}px`" :style="`height:${projects.editor.height || '90%'};`"
    :close-on-click-modal="false" :close-on-press-escape="false"  top="1vh"  draggable class="editor-dialog">
        <template v-if="state.show">
            <Editor :path="projects.editor.path"></Editor>
        </template>
    </el-dialog>
</template>

<script>
import {reactive,watch } from 'vue';
import { useProjects } from '../list';
import Editor from './Editor.vue';
export default {
    components:{Editor},
    props:['modelValue'],
    setup (props,{emit}) {

        const projects = useProjects();
        const state = reactive({
            show:true,
        });
        watch(() => state.show, (val) => {
            if (!val) {
                setTimeout(() => {
                    emit('update:modelValue', val);
                }, 300);
            }
        });

        return {projects,state}
    }
}
</script>

<style lang="stylus">
.el-overlay-dialog{
    overflow: hidden !important;
}
.editor-dialog{
    max-width: 80%;
    .el-dialog__body{
        height:calc(100% - 4rem);
    }
}
</style>