<template>
    <div class="context-menu-wrap" :style="{left:`${projects.contextMenu.x+4}px`,top:`${projects.contextMenu.y+4}px`}">
        <a href="javascript:;" @click="handleRefresh"><el-icon><Refresh/></el-icon> 刷新</a>
        <a href="javascript:;" v-if="hasInProject">
            <div class="item">
                <span><el-icon><More/></el-icon>目录</span>
                <span class="flex-1"></span>
                <el-icon><ArrowRight/></el-icon>
            </div>
            <div class="sub">
                <a href="javascript:;" @click="handleBack"><el-icon><Back/></el-icon>回上一级</a>
                <a href="javascript:;" @click="handleHome"><el-icon><Back/></el-icon>回应用列表</a>
            </div>
        </a>
        <template v-if="hasInProject">
            <a href="javascript:;" @click="handleUpload('*/*')"><el-icon><Upload/></el-icon>上传</a>
        </template>
        <a href="javascript:;" @click="handleDownload"><el-icon><Download/></el-icon>下载</a>
        <a href="javascript:;" v-if="canSource" @click="handleSource"><el-icon><EditPen/></el-icon>源码</a>
        <a href="javascript:;">
            <div class="item">
                <span><el-icon><Plus/></el-icon>新建</span>
                <span class="flex-1"></span>
                <el-icon><ArrowRight/></el-icon>
            </div>
            <div class="sub">
                <template v-if="hasInProject">
                    <a href="javascript:;" @click="handleCreateFile(true)"><el-icon><DocumentAdd/></el-icon>新建文件</a>
                    <a href="javascript:;" @click="handleCreateFile(false)"><el-icon><FolderAdd/></el-icon>新建文件夹</a>
                </template>
                <template v-else>
                    <a href="javascript:;" @click="handleCreate()"><el-icon><Plus/></el-icon>创建应用</a>
                    <a href="javascript:;" @click="handleUpload('.fpk')"><el-icon><Upload/></el-icon>导入fpk</a>
                </template>
            </div>
        </a>
        
        <template v-if="projects.contextMenu.row">
            <a href="javascript:;" class="red" @click="handleDel"><el-icon><Delete/></el-icon>删除</a>
        </template>
    </div>
</template>

<script>
import {Refresh,Upload,Download,DocumentAdd,FolderAdd,Delete,EditPen, Plus, Back, ArrowRight, More} from '@element-plus/icons-vue'
import { useProjects } from './list';
import { computed, onMounted } from 'vue';
import { ElMessageBox } from 'element-plus';
import { useLogger } from '../logger';
import { fetchApi } from '@/api/api';
export default {
    components: {Refresh,Upload,Download,DocumentAdd,FolderAdd,Delete,EditPen,Plus,Back,ArrowRight,More},
    setup () {

        const logger = useLogger();
        const projects = useProjects();

        const canSource = computed(()=>projects.value.contextMenu.row && projects.value.contextMenu.row.if);
        const hasInProject = computed(()=>projects.value.page.path != '.');
        const handleRefresh = ()=>{
            projects.value.load();
        }
        const handleCreate = ()=>{
            projects.value.showCreate = true;
        }

        const handleBack = ()=>{
            projects.value.page.path = projects.value.page.path.replace(/\/[^\/]+$/,'');
            projects.value.load();
        }
        const handleHome = ()=>{
            projects.value.page.path = '.';
            projects.value.load();
        }
        const handleUpload = (mime)=>{
            projects.value.showUpload = true;
            projects.value.uploadMime = mime;
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

        const handleSource = ()=>{
            projects.value.current.path = `${projects.value.page.path}/${projects.value.contextMenu.row.name}`;
            projects.value.current.remark = projects.value.contextMenu.row.remark;
            projects.value.current.source = true;
        }

        onMounted(()=>{
            document.addEventListener('click',(e)=>{
                projects.value.contextMenu.show = false;
            });
        });

        return {projects,canSource,hasInProject,
            handleRefresh,handleCreate,handleBack,handleHome,handleUpload,handleDownload,handleCreateFile,handleDel,handleSource}
    }
}
</script>

<style lang="stylus" scoped>
html.dark .context-menu-wrap{
    background-color: #1a1e23;
    a{
        border-bottom-color: var(--main-border-color);
        color: var(--main-text-color);
        &:hover{
            background-color: #3f4955;
        }
    }
    .sub{
        background-color: #1a1e23;
    }
}
.context-menu-wrap{
    position: fixed;
    left:0;
    top:0;
    background-color: rgba(255,255,255,1);
    border: 1px solid var(--main-border-color);
    box-shadow: 0 0 5px rgba(0,0,0,0.1);
    z-index 99999;
    border-radius: 5px;
    min-width: 10rem;

    a{
        display: block;
        padding: 5px 10px;
        cursor: pointer;
        font-size:1.3rem;
        position: relative;
        white-space : nowrap;
        border-bottom: 1px solid #f5f5f5;
        &:last-child{
            border-bottom: none;
        }

        .item{
            display:flex
            justify-content: center;
            align-items: center;
        }

        .sub{
            position: absolute;
            left: 100%;
            top: 0;
            background-color: rgba(255,255,255,1);
            border: 1px solid var(--main-border-color);
            box-shadow: 0 0 5px rgba(0,0,0,0.1);
            z-index 99999;
            border-radius: 5px;
            display: none;
        }

        &:hover{
            background-color: #eee;
            .sub{
                display: block;
            }
        }
    }
}
</style>