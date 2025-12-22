<template>
    <div class="actions-wrap">
        <template v-if="paths.length == 0">
            <el-button type="primary" plain size="small" @click="handleCreate"><el-icon><Plus /></el-icon>新增应用</el-button>
        </template>
        <template v-if="paths.length >= 1">
            <el-button type="primary" plain size="small" @click="handleBuild" :loading="projects.building"><el-icon><Coin /></el-icon>打包FPK</el-button>
            <el-button plain size="small" @click="handleUpload" :loading="projects.building"><el-icon><Upload /></el-icon>上传文件</el-button>
            <el-button plain size="small" @click="handleCreateFile(true)" :loading="projects.building"><el-icon><DocumentAdd /></el-icon>新建文件</el-button>
            <el-button plain size="small" @click="handleCreateFile(false)" :loading="projects.building"><el-icon><FolderAdd /></el-icon>新建文件夹</el-button>
        </template>
        <Create v-model="projects.showCreate" v-if="projects.showCreate"></Create>
        <UploadFile v-model="projects.showUpload" v-if="projects.showUpload"></UploadFile>
    </div>
</template>

<script>
import { Coin, Upload,DocumentAdd,FolderAdd,Plus } from '@element-plus/icons-vue'
import { computed, reactive } from 'vue';
import { useProjects } from './list';
import Create from './Create.vue';
import { useLogger } from '../logger';
import { ElMessageBox } from 'element-plus';
import UploadFile from './Upload.vue';
import { fetchApi } from '@/api/api';
export default {
    components: {
        Upload,Coin,DocumentAdd,FolderAdd,Plus,Create,UploadFile
    },
    setup () {
        const logger = useLogger();
        const projects = useProjects();
        const paths = computed(()=>projects.value.page.path.split('/').filter(item=>item && item!='.'));
        const handleCreate = ()=>{
            projects.value.showCreate = true;
        }
        const handleUpload = ()=>{
            projects.value.showUpload = true;
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
            projects.value.building = true;
            logger.value.debug('开始打包...');
            fetchApi(`/files/build`,{
                params:{name:projects.value.page.path.split('/').filter(item=>item && item!='.')[0]},
                method:'POST',
                headers:{'Content-Type':'application/json'},
            }).then(res=>res.text()).then((res)=>{
                if(res.indexOf('Packing successfully')){
                    logger.value.success(res);
                    projects.value.load(); 
                }else{
                    logger.value.error(res);
                }
            }).catch((e)=>{
                logger.value.error(`${e}`);
            }).finally(()=>{
                projects.value.building = false;
            });
        }

        return {paths,projects,handleCreate,handleUpload,handleCreateFile,handleBuild}
    }
}
</script>

<style lang="stylus" scoped>
.actions-wrap{
    border-bottom:1px solid #ddd;
    padding:.6rem 1rem;
}
</style>