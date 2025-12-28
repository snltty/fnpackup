<template>
    <div class="actions-wrap">
        <template v-if="paths.length == 0">
            <el-button type="primary" plain size="small" @click="handleCreate"><el-icon><Plus /></el-icon>创建应用</el-button>
            <el-button plain size="small" @click="handleUpload('.fpk')" :loading="projects.building"><el-icon><Upload /></el-icon>导入fpk</el-button>
        </template>
        <template v-if="paths.length >= 1">
            <el-button type="primary" plain size="small" @click="handleBuild" :loading="projects.building"><el-icon><Coin /></el-icon>打包fpk</el-button>
            <el-button type="success" plain size="small" @click="handleGuide" :loading="projects.building"><el-icon><Files /></el-icon>快速编辑</el-button>
            
            <el-button plain size="small" @click="handleUpload('*/*')" :loading="projects.building"><el-icon><Upload /></el-icon>上传文件</el-button>
            <el-button plain size="small" @click="handleCreateFile(true)" :loading="projects.building"><el-icon><DocumentAdd /></el-icon>新建文件</el-button>
            <el-button plain size="small" @click="handleCreateFile(false)" :loading="projects.building"><el-icon><FolderAdd /></el-icon>新建文件夹</el-button>
        </template>
        <Create v-model="projects.showCreate" v-if="projects.showCreate"></Create>
        <UploadFile v-model="projects.showUpload" v-if="projects.showUpload"></UploadFile>
    </div>
</template>

<script>
import { Coin, Upload,DocumentAdd,FolderAdd,Plus,Files } from '@element-plus/icons-vue'
import { computed } from 'vue';
import { useProjects } from '../list';
import Create from './Create.vue';
import { useLogger } from '../../logger';
import { ElMessageBox } from 'element-plus';
import UploadFile from './Upload.vue';
import { fetchApi } from '@/api/api';
export default {
    components: {
        Upload,Coin,DocumentAdd,FolderAdd,Plus,Files,Create,UploadFile
    },
    setup () {
        const logger = useLogger();
        const projects = useProjects();
        const paths = computed(()=>projects.value.page.path.split('/').filter(item=>item && item!='.'));
        const handleCreate = ()=>{
            projects.value.showCreate = true;
        }
        const handleUpload = (mime)=>{
            projects.value.showUpload = true;
            projects.value.uploadMime = mime;
        }
        const handleCreateFile = (isFile)=>{
            ElMessageBox.prompt('输入名称', `新建${isFile?'文件':'文件夹'}`, {
                confirmButtonText: '确认',
                cancelButtonText: '取消',
                draggable:true,
                customStyle: {
                    'vertical-align':'unset'
                },
            }).then(({ value }) => {
                if(!value) {
                    return;
                }
                fetchApi(`/files/createfile`,{
                    params:{path:`${projects.value.page.path}/${value}`,f:isFile},
                    method:'POST',
                    headers:{'Content-Type':'application/json'},
                }).then(res=>res.text()).then((res)=>{
                    if(res){
                        logger.value.error(res);
                    }else{
                        logger.value.success(`[${value}]创建成功`);
                        projects.value.load(); 
                    }
                });
            }).catch((e) => {
                logger.value.error(`${e}`);
            })
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

        return {paths,projects,handleCreate,handleUpload,handleCreateFile,handleBuild,handleGuide}
    }
}
</script>

<style lang="stylus" scoped>
.actions-wrap{
    border-bottom:1px solid #ddd;
    padding:.6rem 1rem;
}
</style>