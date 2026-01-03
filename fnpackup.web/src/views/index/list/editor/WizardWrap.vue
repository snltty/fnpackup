<template>
    
        <el-tabs v-model="current" type="border-card" class="h-100 bs wizard-tab" @tab-change="handleChange">
            <template v-for="item in options">
                <el-tab-pane :label="item.label" :name="item.value" v-loading="projects.current.loading" class="h-100 bs">
                    <Wizard v-if="projects.current.content && current==item.value" :type="current" @save="saveContent"></Wizard>
                </el-tab-pane>
            </template>
        </el-tabs>
</template>

<script>
import { onMounted,  ref } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import Wizard from './Wizard.vue';
import { fetchApi } from '@/api/api';
import { ElMessage } from 'element-plus';

export default {
    match:/\/wizard/,
    width:600,
    components:{Wizard},
    setup () {
        const logger = useLogger();
        const projects = useProjects();

        const paths = (/\/wizard$/.test(projects.value.current.path) 
        ?`${projects.value.current.path}/install`
        :projects.value.current.path).split('/');
        const current = ref('');

        const options = [
            {label:'用户安装向导',value:'install'},
            {label:'用户卸载向导',value:'uninstall'},
            {label:'用户更新向导',value:'upgrade'},
            {label:'用户配置向导',value:'config'}
        ];
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
        const saveContent = (content)=>{
            projects.value.current.loading = true;
            if(content == '[]'){
                fetchApi(`/files/delfile`,{
                    params:{ path:paths.join('/'), f:true},
                    method:'POST',
                    headers:{'Content-Type':'application/json'},
                })
                .then(res=>res.text()).then((res)=>{
                    projects.value.current.loading = false;
                    if(res){
                        logger.value.error(res);
                    }else{
                        ElMessage.success('操作成功!');
                        logger.value.error(`未配置字段，[${paths.join('/')}]删除成功`);
                        projects.value.load(); 
                    }
                }).catch((e)=>{
                    projects.value.current.loading = false;
                    logger.value.error(`${e}`);
                });
                return;
            }
            fetchApi(`/files/write`,{
                method:'POST',
                headers:{'Content-Type':'application/json'},
                body:JSON.stringify({
                    path:paths.join('/'),
                    content:content
                })
            }).then(res => res.text()).then(res => {
                projects.value.current.loading = false;
                if(res){
                    logger.value.error(res);
                }else{
                    ElMessage.success('保存成功');
                    logger.value.success(`保存成功`);
                    projects.value.load();
                }
            }).catch((e)=>{
                projects.value.current.loading = false;
                logger.value.error(`${e}`);
            })
        }

        onMounted(()=>{ 
            handleChange(paths[paths.length-1])
        });

        return {projects,current,options,handleChange,saveContent}
    }
}
</script>

<style lang="stylus" scoped>
</style>