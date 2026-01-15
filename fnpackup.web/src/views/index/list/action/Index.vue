<template>
    <div class="actions-wrap">
        <template v-if="paths.length == 0">
            <el-button type="primary" plain size="small" @click="handleCreate"><el-icon><Plus /></el-icon>创建应用</el-button>
            <el-button plain size="small" @click="handleUpload('.fpk')"><el-icon><Upload /></el-icon>导入fpk</el-button>
        </template>
        <template v-if="paths.length >= 1">
            <el-button type="primary" plain size="small" @click="handleBuild"><el-icon><Pointer /></el-icon>打包fpk</el-button>
            <el-button type="success" plain size="small" @click="handleGuide"><el-icon><Files /></el-icon>快速编辑</el-button>     
            <el-button type="warning" plain size="small" @click="handleIcon"><el-icon><Picture /></el-icon>图标设计</el-button>     
            <el-button type="info" plain size="small" @click="handleEnv"><el-icon><MessageBox /></el-icon>环境变量</el-button> 
        </template>
            
        <Create v-model="projects.editor.create" v-if="projects.editor.create"></Create>
        <UploadFile v-model="projects.editor.upload" v-if="projects.editor.upload"></UploadFile>
    </div>
</template>

<script>
import { Upload,Plus,Files, Pointer, Picture, MessageBox } from '@element-plus/icons-vue'
import { computed } from 'vue';
import { useProjects } from '../list';
import Create from './Create.vue';
import UploadFile from './Upload.vue';
export default {
    components: {
        Upload,Pointer,Plus,Files,Create,UploadFile,Picture,MessageBox
    },
    setup () {
        const projects = useProjects();
        const paths = computed(()=>projects.value.page.path.split('/').filter(item=>item && item!='.'));
        const handleCreate = ()=>{
            projects.value.editor.create = true;
        }
        const handleBuild = ()=>{
            const name = projects.value.page.path.split('/').filter(item=>item && item!='.')[0];
            projects.value.editor.path =`./${name}/fnpack`;
            projects.value.editor.remark = '打包下载';
            projects.value.editor.show = true;
        }
        const handleGuide = ()=>{
            projects.value.editor.guide = true;
        }
        const handleUpload = (mime)=>{
            projects.value.editor.upload = true;
            projects.value.editor.mime = mime;
        }
        const handleIcon = ()=>{
            projects.value.editor.paint = true;
        }
        const handleEnv = ()=>{
            const name = projects.value.page.path.split('/').filter(item=>item && item!='.')[0];
            projects.value.editor.path =`./${name}/env`;
            projects.value.editor.remark = '环境变量';
            projects.value.editor.show = true;
        }

        return {paths,projects,handleCreate,handleBuild,handleGuide,handleUpload,handleIcon,handleEnv}
    }
}
</script>

<style lang="stylus" scoped>
html.dark .actions-wrap{
    border-color:var(--main-border-color);
}
.actions-wrap{
    border-bottom:1px solid #e2e8f0e6;
    padding:.6rem 1rem;
}
</style>