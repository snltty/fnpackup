<template>
    <div class="table-wrap h-100">
        <div class="inner h-100 flex flex-column flex-nowrap">
            <div class="tips">
                <ul>
                    <li>自动托管以下目录静态资源，使用http://{appname}.domain.com:{{state.port}} 或 http://ip:{{state.port}}/{appname} 访问</li>
                    <li>1. fpk中的manifest配置的fnpackup={目录}，托管app/{目录}</li>
                    <li>2. 文件管理/应用文件/fnpackup-docker/statics/{目录}，托管{目录}，{目录}就是{appname}</li>
                </ul>
            </div>
            <div class="head">
                <el-button type="primary" size="small" :loading="state.loading" @click="handleSearch"><el-icon><Refresh></Refresh></el-icon>重载托管和刷新列表</el-button>
            </div>
            <div class="flex-1">
                <el-table :data="state.list" stripe size="small" height="98%" v-loading="state.loading" style="--el-table-header-bg-color: #f1f4f9">
                    <el-table-column prop="name" label="名称"></el-table-column>
                    <el-table-column prop="root" label="根目录"></el-table-column>
                </el-table>
            </div>
        </div>
    </div>
</template>

<script>
import { fetchApi } from '@/api/api';
import { Refresh } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { onMounted, reactive} from 'vue';
export default {
    components:{Refresh},
    setup () {

        const state = reactive({
            list:[],
            loading:false,
            port:window.location.port
        });

        const getList = () => { 
            return new Promise((resolve,reject)=>{ 
                state.loading = true;
                fetchApi('/static/list',{method:'get'}).then(c=>c.json())
                .then(res=>{
                    state.list = res;
                }).finally(()=>{
                    state.loading = false;
                    resolve();
                });
            });
        }
        const search = () => { 
            return new Promise((resolve,reject)=>{ 
                state.loading = true;
                fetchApi('/static/search',{method:'post'}).then(res=>{

                }).finally(()=>{
                    state.loading = false;
                    resolve();
                })
            });
        }
        const handleSearch = () => { 
            search().then(()=>{
                getList().then(()=>{
                    ElMessage.success('已重载');
                });
            });
        }
        
        onMounted(()=>{
            getList();
        });

        return {state,handleSearch}
    }
}
</script>
<style lang="stylus" scoped>

.table-wrap{
    overflow hidden;
    padding:1rem;
    box-sizing: border-box;
    .inner{
        border:1px solid #e2e8f0e6;
        border-radius:5px;
        box-sizing: border-box;

        .tips{
            padding:.6rem 1rem;
            border-bottom:1px solid #e2e8f0e6;
            font-size:1.2rem;
        }
        .head{
            border-bottom:1px solid #e2e8f0e6;
            padding:.6rem 1rem;
        }
    }
}
</style>