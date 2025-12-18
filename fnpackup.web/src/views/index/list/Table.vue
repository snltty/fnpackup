<template>
    <div class="table-wrap flex-1 flex flex-column flex-nowrap">
         <el-table :data="projects.page.list" stripe border size="small" class="flex-1" v-loading="projects.loading" @cell-dblclick="handleOpen">
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
            <el-table-column prop="remark" label="描述"></el-table-column>
            <el-table-column prop="ct" label="创建时间" width="140" />
            <el-table-column prop="lwt" label="修改时间" width="140" />
        </el-table>
        <div class="pages">
            <div>
                <el-pagination small background layout="prev, pager, next" 
                :total="projects.page.count"
                :current-page="1" 
                :page-size="projects.page.ps" @current-change="handlePageChange"/>
            </div>
        </div>
    </div>
</template>

<script>
import { onMounted} from 'vue';
import {EditPen,Document,Folder} from '@element-plus/icons-vue'
import { useProjects } from './list';
export default {
    components:{EditPen,Document,Folder},
    setup () {
        
        const projects = useProjects();
        const handlePageChange = (p)=>{
            projects.value.page.p = p;
            projects.value.load();
        }
        const handleOpen = (row,cell)=>{
            if(row.if==false){
                projects.value.page.path = `${projects.value.page.path}/${row.name}`;
            }else{
                projects.value.current.path =`${projects.value.page.path}/${row.name}`;
            }
        }
        onMounted(()=>{
           projects.value.load();
        });

        return {projects,handlePageChange,handleOpen}
    }
}
</script>

<style lang="stylus" scoped>
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
</style>