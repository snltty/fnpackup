<template>
    <div class="context-menu-wrap" :style="{left:`${projects.contextMenu.x+4}px`,top:`${projects.contextMenu.y+4}px`}">
        <a href="javascript:;" @click="handleRefresh"><el-icon><Refresh/></el-icon> 刷新</a>
        <a href="javascript:;" @click="handleUpload" v-if="hasInProject"><el-icon><Upload/></el-icon>上传</a>
        <a href="javascript:;" @click="handleDownload" v-if="hasInProject"><el-icon><Download/></el-icon>下载</a>
        <a href="javascript:;" @click="handleWizard" v-if="canWizard"><el-icon><DocumentAdd/></el-icon>编辑用户向导</a>
        <a href="javascript:;" @click="handleCreateFile(true)" v-if="hasInProject"><el-icon><DocumentAdd/></el-icon>新建文件</a>
        <a href="javascript:;" @click="handleCreateFile(false)" v-if="hasInProject"><el-icon><FolderAdd/></el-icon>新建文件夹</a>
        <template v-if="projects.contextMenu.row">
            <a href="javascript:;" class="red" @click="handleDel"><el-icon><Delete/></el-icon>删除</a>
        </template>
    </div>
</template>

<script>
import {Refresh,Upload,Download,DocumentAdd,FolderAdd,Delete} from '@element-plus/icons-vue'
import { useProjects } from './list';
import { computed, onMounted } from 'vue';
import { ElMessageBox } from 'element-plus';
import { useLogger } from '../logger';
import { fetchApi } from '@/api/api';
export default {
    components: {Refresh,Upload,Download,DocumentAdd,FolderAdd,Delete},
    setup () {

        const logger = useLogger();
        const projects = useProjects();

        const hasInProject = computed(()=>projects.value.page.path != '.');
        const canWizard = computed(()=>{
            return /\/wizard/.test(projects.value.page.path)
            || (projects.value.contextMenu.row && projects.value.contextMenu.row.name == 'wizard');
        });
        const handleWizard = ()=>{
            if(projects.value.contextMenu.row){
                projects.value.current.path = `${projects.value.page.path}/${projects.value.contextMenu.row.name}`;
            }else{
                projects.value.current.path = projects.value.page.path;
            }
            projects.value.current.remark = '用户向导';
            projects.value.current.show = true;
        }
        const handleRefresh = ()=>{
            projects.value.load();
        }
        const handleUpload = ()=>{
            projects.value.showUpload = true;
        }
        const handleDownload = ()=>{
            let href = process.env.NODE_ENV === 'development' 
            ? `http://localhost:1069/files/download?path=${projects.value.page.path}`
            : `/files/download?path=${projects.value.page.path}`;
            if(projects.value.contextMenu.row){
                href = `${href}/${projects.value.contextMenu.row.name}`;
            }
            const a = document.createElement('a');
            a.href = href;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
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
            });
        }
        const handleDel = ()=>{
            ElMessageBox.confirm('确定要删除吗？', '提示', {
                confirmButtonText: '确认',
                cancelButtonText: '取消',
                type: 'warning',
                draggable:true,
                customStyle: {
                    'vertical-align':'unset'
                },
            }).then(() => {
                fetchApi(`/files/delfile`,{
                    params:{
                        path:`${projects.value.page.path}/${projects.value.contextMenu.row.name}`,
                        f:projects.value.contextMenu.row.if
                    },
                    method:'POST',
                    headers:{'Content-Type':'application/json'},
                })
                .then(res=>res.text()).then((res)=>{
                    if(res){
                        logger.value.error(res);
                    }else{
                        logger.value.success(`[${projects.value.contextMenu.row.name}]删除成功`);
                        projects.value.load(); 
                    }
                }).catch((e)=>{
                    logger.value.error(`${e}`);
                });
            })
        }

        onMounted(()=>{
            document.addEventListener('click',(e)=>{
                projects.value.contextMenu.show = false;
            });
        });

        return {projects,hasInProject,canWizard,handleWizard,handleRefresh,handleUpload,handleDownload,handleCreateFile,handleDel}
    }
}
</script>

<style lang="stylus" scoped>
.context-menu-wrap{
    position: fixed;
    left:0;
    top:0;
    background-color: rgba(255,255,255,1);
    border: 1px solid #ddd;
    box-shadow: 0 0 5px rgba(0,0,0,0.1);
    z-index 99999;
    border-radius: 5px;

    a{
        display: block;
        padding: 5px 10px;
        cursor: pointer;
        font-size:1.3rem;

        &:hover{
            background-color: #eee;
        }

    }
}
</style>