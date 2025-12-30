<template>
    <div class="path-wrap">
        <el-breadcrumb>
            <el-breadcrumb-item>
                <a href="javascript:;" @click="handleBack"><el-icon><ArrowLeftBold /></el-icon></a>
            </el-breadcrumb-item>
            <el-breadcrumb-item>
                <a href="javascript:;" @click="handleRefresh"><el-icon><Refresh /></el-icon></a>
            </el-breadcrumb-item>
            <el-breadcrumb-item>
                <a href="javascript:;" @click="handleFolder(0)"><el-icon><Pointer /></el-icon>应用列表</a>
            </el-breadcrumb-item>
            <template v-for="(value,index) in paths">
                <el-breadcrumb-item @click="handleFolder(index+1)">
                    <a href="javascript:;">
                        <el-icon v-if="index===0"><Star /></el-icon>
                        <span>{{ value }}</span>
                    </a>
                </el-breadcrumb-item>
            </template>
        </el-breadcrumb>
    </div>
</template>

<script>
import { computed } from 'vue';
import { ArrowLeftBold,Refresh, Star, Pointer } from '@element-plus/icons-vue';
import { useProjects } from './list';
export default {
    components:{ArrowLeftBold,Refresh,Pointer,Star},
    setup () {
        
        const projects = useProjects();
        const paths = computed(()=>projects.value.page.path.split('/').filter(item=>item && item!='.'));
        const handleFolder = (index)=>{
            const paths = projects.value.page.path.split('/').filter(c=>c);
            projects.value.page.path = paths.filter((item,_index)=>_index<=index).join('/');
        }
        const handleBack = ()=>{
            const paths = projects.value.page.path.split('/').filter(c=>c);
            projects.value.page.path = paths.filter((item,_index)=>_index<paths.length-1).join('/');
        }
        const handleRefresh = ()=>{
            projects.value.load();
        }

        return {projects,paths,handleFolder,handleBack,handleRefresh}
    }
}
</script>

<style lang="stylus" scoped>
.path-wrap{
    padding:1rem;
    border-bottom: 1px solid #e2e8f0e6;
    .el-breadcrumb{
        font-size: 1.3rem;

        .el-icon{
            vertical-align: bottom;
        }
    }
}
</style>