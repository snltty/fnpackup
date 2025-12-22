<template>
    <div class="path-wrap">
        <ul class="flex">
            <li @click="handleBack">
                <span><el-icon size="14"><Back /></el-icon></span>
            </li>
            <li @click="handleRefresh">
                <span><el-icon size="14"><Refresh /></el-icon></span>
            </li>
            <li @click="handleFolder(0)">
                <span>应用列表</span>
            </li>
            <template v-for="(value,index) in paths">
                <li @click="handleFolder(index+1)">
                    <el-icon size="12"><ArrowRight /></el-icon>
                    <span :class="{project:index==0}">{{ value }}</span>
                </li>
            </template>
        </ul>
    </div>
</template>

<script>
import { computed } from 'vue';
import { ArrowRight,Back,Refresh } from '@element-plus/icons-vue';
import { useProjects } from './list';
export default {
    components:{ArrowRight,Back,Refresh},
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
    background-color: #fafafa;
    box-shadow: 0px 0px 10px rgba(0,0,0,0.05);
    li{
        padding:0.5rem 1rem;
        font-size:0;
        border-right:1px solid #ddd;
        color:#333;
        &:hover{
            background-color: #f6f8fa;
            cursor pointer;
            box-shadow: 0 0 5px rgba(0,0,0,0.05) inset;
        }
       
        span{
            font-size:1.2rem;
            display: inline-flex;
            align-items: center;
            vertical-align: bottom;
            line-height:1;
            &.project{
                font-weight: bold;
                color:var(--primary-color);
            }
        }
        .el-icon{
            display: inline-flex;
            align-items: center;
            poesition: relative;
            top:0.1rem;
        }
    }
}
</style>