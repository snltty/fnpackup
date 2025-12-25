<template>
    <el-dialog v-model="projects.current.show" :title="`编辑器[${projects.current.remark}]`" 
    :width="`${projects.current.width}px`" top="1vh" 
    :close-on-click-modal="false" :close-on-press-escape="false"  draggable class="editor-dialog">
        <template v-if="projects.current.show">
            <div style="height:calc(100% - 4rem)">
                <Editor></Editor>
            </div>
        </template>
    </el-dialog>
</template>

<script>
import {watch } from 'vue';
import { useProjects } from '../list';
import Editor from './Editor.vue';
export default {
    components:{Editor},
    setup () {

        const projects = useProjects();
        watch(() => projects.value.current.show, (val) => {
            if (!val) {
                setTimeout(() => {
                    projects.value.current.path = '';
                    projects.value.current.content =  '';
                    projects.value.current.remark =  '';
                }, 300);
            }
        });

        return {projects}
    }
}
</script>

<style lang="stylus">
.editor-dialog{
    max-width: 80%;height:90%
    .el-dialog__body{
        height:100%;
    }
}
</style>