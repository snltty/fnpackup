<template>
    <div class="actions-wrap">
        <template v-if="paths.length == 0">
            <el-button type="primary" plain size="small" @click="handleCreate"><el-icon><Plus /></el-icon>新增项目</el-button>
        </template>
        <template v-if="paths.length >= 1">
            <el-button type="success" plain size="small"><el-icon><Coin /></el-icon>打包FPK</el-button>
            <el-button plain size="small" @click="handleUpload"><el-icon><Upload /></el-icon>上传文件</el-button>
            <el-button plain size="small" @click="handleCreateFile(true)"><el-icon><DocumentAdd /></el-icon>新建文件</el-button>
            <el-button plain size="small" @click="handleCreateFile(false)"><el-icon><FolderAdd /></el-icon>新建文件夹</el-button>
        </template>
        <Create v-model="state.showCreate" v-if="state.showCreate"></Create>
        <UploadFile v-model="state.showUpload" v-if="state.showUpload"></UploadFile>
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
export default {
    components: {
        Upload,Coin,DocumentAdd,FolderAdd,Plus,Create,UploadFile
    },
    setup () {
        const logger = useLogger();
        const projects = useProjects();
        const paths = computed(()=>projects.value.page.path.split('/').filter(item=>item && item!='.'));
        const state = reactive({
            showCreate:false,
            showUpload:false
        });
        const handleCreate = ()=>{
            state.showCreate = true;
        }
        const handleUpload = ()=>{
            state.showUpload = true;
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
                fetch(`http://localhost:5083/files/createfile?path=${projects.value.page.path}/${value}&f=${isFile}`,{
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
            }).catch(() => {
            })
        }

        return {paths,state,handleCreate,handleUpload,handleCreateFile}
    }
}
</script>

<style lang="stylus" scoped>
.actions-wrap{
    border-bottom:1px solid #ddd;
    padding:.6rem 1rem;
}
</style>