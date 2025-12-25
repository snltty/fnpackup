<template>
    <div class="command-wrap">
        <el-tabs v-model="current" type="border-card" class="command-tab" @tab-change="handleChange">
            <template v-for="item in options">
                <el-tab-pane :label="item.label" :name="item.value" v-loading="projects.current.loading" class="h-100">
                    <Source v-if="projects.current.content && current==item.value"></Source>
                </el-tab-pane>
            </template>
        </el-tabs>
    </div>
</template>

<script>
import { onMounted,  ref } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import { fetchApi } from '@/api/api';
import Source from './Source.vue';

export default {
    match:/\/cmd/,
    width:900,
    components:{Source},
    setup () {
        const logger = useLogger();
        const projects = useProjects();

        const paths = (/\/cmd$/.test(projects.value.current.path) 
        ?`${projects.value.current.path}/install_init`
        :projects.value.current.path).split('/');

        const current = ref('');
        const options = [
            {label:'安装初始化',value:'install_init'},
            {label:'安装回调',value:'install_callback'},
            {label:'卸载初始化',value:'uninstall_init'},
            {label:'卸载回调',value:'uninstall_callback'},
            {label:'更新初始化',value:'upgrade_init'},
            {label:'更新回调',value:'upgrade_callback'},
            {label:'主脚本',value:'main'},
            {label:'配置初始化',value:'config_init'},
            {label:'配置回调',value:'config_callback'},
        ]
        const handleChange = (type) => {
            current.value = type;
            projects.value.current.remark = options.reduce((json,item)=>{ json[item.value]=item.label; return json;  },{})[type];
            paths[paths.length-1] = type;
            projects.value.current.path = paths.join('/');
            getContent();
        };

        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                projects.value.current.loading = true;
                projects.value.current.content = '';
                fetchApi(`/files/read`,{
                    params:{path:paths.join('/')},
                    method:'GET',
                    headers:{'Content-Type':'application/json'},
                }).then(res => res.text()).then(res => {
                    projects.value.current.loading = false;
                    projects.value.current.content = res || '[]';
                    resolve();
                }).catch((e)=>{
                    projects.value.current.loading = false;
                    logger.value.error(`${e}`);
                    reject();
                });
            });
        }
        onMounted(()=>{ 
            handleChange(paths[paths.length-1]);
        });

        return {projects,current,options,handleChange}
    }
}
</script>

<style lang="stylus" scoped>
.command-wrap{
    height:80vh;

    .command-tab{
        height:100%;
    }
}
</style>