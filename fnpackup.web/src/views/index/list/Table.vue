<template>
    <div class="table-wrap flex-1 flex flex-column flex-nowrap" @contextmenu="handleContextMenu1">
        <div class="flex-1 scrollbar">
            <el-table :data="projects.page.list" stripe size="small" height="100%" v-loading="projects.loading"
            @cell-dblclick="handleOpen" @row-contextmenu="handleContextMenu" ref="table">
                <el-table-column prop="if" width="35">
                    <template #default="scope">
                        <div class="type">
                            <template v-if="scope.row.docker">
                                <img src="docker.svg" alt="docker" width="20">
                            </template>
                            <template v-else>
                                <img src="binary.svg" alt="binary" width="20">
                            </template>
                        </div>
                    </template>
                </el-table-column>
                <el-table-column prop="name" label="名称">
                    <template #default="scope">
                        <div class="name">
                            <template v-if="scope.row.if">
                                <el-icon size="16"><Document /></el-icon>
                            </template>
                            <template v-else>
                                <el-icon size="16"><Folder /></el-icon>
                            </template>
                            <span class="mgl-1">{{ scope.row.name }}</span>
                        </div>
                    </template>
                </el-table-column>
                <el-table-column prop="remark" label="描述">
                    <template #default="scope">
                        <template v-if="scope.row.doc">
                            <a :href="scope.row.doc" target="_blank" class="a-doc">{{ scope.row.remark }}</a>
                        </template>
                        <template v-else>
                            <span>{{ scope.row.remark }}</span>
                        </template>
                    </template>
                </el-table-column>
                <el-table-column prop="lwt" label="修改时间" width="140" />
                <el-table-column prop="ct" label="创建时间" width="140" />
            </el-table>
        </div>
        <div class="pages">
            <div>
                <el-pagination small background layout="prev, pager, next" 
                :total="projects.page.count"
                :current-page="1" 
                :page-size="projects.page.ps" @current-change="handlePageChange"/>
            </div>
        </div>
        <ContextMenu v-if="projects.contextMenu.show"></ContextMenu>
    </div>
</template>

<script>
import { onMounted, ref} from 'vue';
import {EditPen,Document,Folder} from '@element-plus/icons-vue'
import { useProjects } from './list';
import ContextMenu from './ContextMenu.vue';
import { useLogger } from '../logger';
export default {
    components:{EditPen,Document,Folder,ContextMenu},
    setup () {
        
        const logger = useLogger();
        const projects = useProjects();
        const handlePageChange = (p)=>{
            projects.value.page.p = p;
            projects.value.load();
        }

        const excludeEditor = [
            /\.DS_Store$/,
            /\.fpk$/,
            ///\/app\/server\//,
            ///\/app\/www\//,
        ]
        const handleOpen = (row,cell)=>{
            if(excludeEditor.some(e=>e.test(`${projects.value.page.path}/${row.name}`))){
                logger.value.error(`${`${projects.value.page.path}/${row.name}`} 不可编辑`)
                return;
            }
            if(row.if==false){
                projects.value.page.path = `${projects.value.page.path}/${row.name}`;
            }else{
                projects.value.current.path =`${projects.value.page.path}/${row.name}`;
                projects.value.current.remark = row.remark;
                projects.value.current.show = true;
            }
        }
        const handleContextMenu = (row,cell,event)=>{
            event.preventDefault()
            event.stopPropagation();
            projects.value.contextMenu.row = row;
            projects.value.contextMenu.cell = cell;
            projects.value.contextMenu.x = event.clientX;
            projects.value.contextMenu.y = event.clientY;
            projects.value.contextMenu.show = true;
        }
        const handleContextMenu1 = (event)=>{
            event.preventDefault()
            event.stopPropagation();
            projects.value.contextMenu.row = null;
            projects.value.contextMenu.cell= null;
            projects.value.contextMenu.x = event.clientX;
            projects.value.contextMenu.y = event.clientY;
            projects.value.contextMenu.show = true;
        }
        onMounted(()=>{
            projects.value.load();
        });

        return {projects,handlePageChange,handleOpen,handleContextMenu1,handleContextMenu}
    }
}
</script>
<style lang="stylus">
</style>
<style lang="stylus" scoped>
html.dark{
    a.a-doc{
        color:#257fff;
    }
}
.table-wrap{
    overflow hidden;
}
.pages{
    text-align: center;
    padding:1rem 0;

    &>div{
        display: inline-block;
    }

    
}
.type{
    display: flex;
    align-items: center;
}
.name{
    display: flex;
    align-items: center;
}

a.a-doc{
    color:blue;
    text-decoration: underline;
}
</style>