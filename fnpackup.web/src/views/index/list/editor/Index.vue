<template>
    <el-dialog v-model="projects.current.show" :title="`编辑器[${projects.current.remark}]`" :width="`${projects.current.width}px`" top="1vh" height="90%" style="max-width: 80rem;" 
    :close-on-click-modal="false" :close-on-press-escape="false"  draggable>
        <template v-if="projects.current.show">
            <Editor></Editor>
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

<style lang="stylus" scoped>

</style>