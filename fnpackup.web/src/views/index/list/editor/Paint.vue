<template>
    <el-dialog v-model="projects.current.paint" title="画图"  width="300" top="1vh"
    :close-on-click-modal="false" :close-on-press-escape="false" draggable>
    <div class="svg-wrap">
        <svg width="256" height="256" viewBox="0 0 256 256" xmlns="http://www.w3.org/2000/svg">
            <g  >
                <rect x="1" y="1" width="254" height="254" rx="10" fill="#4fbcbc" stroke="#000" stroke-width="2"></rect>
            </g>
        </svg>
    </div>
    </el-dialog>
</template>

<script>
import { useProjects } from '../list';
import "tldraw-vue/dist/tldraw-vue.css";
export default {
    setup () {
        const projects = useProjects();

        return {projects}
    }
}
</script>

<style>

</style>
<style lang="stylus">
.editor-dialog{
    max-width: 80%;
    max-height: 90%;
    .el-dialog__body{
        height:100%;
    }
}
</style>