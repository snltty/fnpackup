<template>
    <div class="actions-wrap">
        <template v-if="paths.length == 0">
            <el-button type="primary" plain size="small" @click="handleCreate"><el-icon><Plus /></el-icon>创建应用</el-button>
            <el-button plain size="small" @click="handleUpload('.fpk')" :loading="projects.building"><el-icon><Upload /></el-icon>导入fpk</el-button>
        </template>
        <template v-if="paths.length >= 1">
            <el-button type="primary" plain size="small" @click="handleBuild" :loading="projects.building"><el-icon><Pointer /></el-icon>打包fpk</el-button>
            <el-button type="success" plain size="small" @click="handleGuide" :loading="projects.building"><el-icon><Files /></el-icon>快速编辑</el-button>     
            <el-button type="warning" plain size="small" @click="handleIcon" :loading="projects.building"><el-icon><Picture /></el-icon>图标设计</el-button>     
        </template>
        <Create v-model="projects.showCreate" v-if="projects.showCreate"></Create>
        <UploadFile v-model="projects.showUpload" v-if="projects.showUpload"></UploadFile>
    </div>
</template>

<script>
import { Upload,Plus,Files, Pointer, Picture } from '@element-plus/icons-vue'
import { computed } from 'vue';
import { useProjects } from '../list';
import Create from './Create.vue';
import { useLogger } from '../../logger';
import UploadFile from './Upload.vue';
export default {
    components: {
        Upload,Pointer,Plus,Files,Create,UploadFile,Picture
    },
    setup () {
        const logger = useLogger();
        const projects = useProjects();
        const paths = computed(()=>projects.value.page.path.split('/').filter(item=>item && item!='.'));
        const handleCreate = ()=>{
            projects.value.showCreate = true;
        }
        const handleBuild = ()=>{
            const name = projects.value.page.path.split('/').filter(item=>item && item!='.')[0];
            projects.value.current.path =`./${name}/fnpack`;
            projects.value.current.remark = '打包下载';
            projects.value.current.show = true;
        }
        const handleGuide = ()=>{
            projects.value.current.guide = true;
        }
        const handleUpload = (mime)=>{
            projects.value.showUpload = true;
            projects.value.uploadMime = mime;
        }
        const handleIcon = ()=>{
            projects.value.showPaint = true;
        }

        return {paths,projects,handleCreate,handleBuild,handleGuide,handleUpload,handleIcon}
    }
}
</script>

<style lang="stylus" scoped>
html.dark .actions-wrap{
    border-color:#39434c;
}
.actions-wrap{
    border-bottom:1px solid #e2e8f0e6;
    padding:.6rem 1rem;
}
</style>