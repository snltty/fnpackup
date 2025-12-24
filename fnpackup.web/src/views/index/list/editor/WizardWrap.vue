<template>
    <div class="wizard-wrap">
        <el-tabs v-model="current" type="border-card" class="wizard-tab" @tab-change="handleChange">
            <el-tab-pane label="安装向导" name="install" v-loading="projects.current.loading" class="h-100">
                <Wizard v-if="projects.current.content && current=='install'" :type="current" @save="saveContent"></Wizard>
            </el-tab-pane>
            <el-tab-pane label="卸载向导" name="uninstall" v-loading="projects.current.loading" class="h-100">
                <Wizard v-if="projects.current.content && current=='uninstall'" :type="current" @save="saveContent"></Wizard>
            </el-tab-pane>
            <el-tab-pane label="更新向导" name="upgrade" v-loading="projects.current.loading" class="h-100">
                <Wizard v-if="projects.current.content && current=='upgrade'" :type="current" @save="saveContent"></Wizard>
            </el-tab-pane>
            <el-tab-pane label="配置向导" name="config" v-loading="projects.current.loading" class="h-100">
                <Wizard v-if="projects.current.content && current=='config'" :type="current" @save="saveContent"></Wizard>
            </el-tab-pane>
        </el-tabs>
    </div>
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
        const current = ref(paths[paths.length-1]);
        const handleChange = (type) => {
            current.value = type;
            projects.value.current.remark = {
                'install':'用户安装向导',
                'uninstall':'用户卸载向导',
                'upgrade':'用户更新向导',
                'config':'用户配置向导'
            }[type];
            paths[paths.length-1] = type;
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
                        logger.value.success(`未配置字段，[${paths.join('/')}]删除成功`);
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
            getContent();
        });

        return {projects,current,handleChange,saveContent}
    }
}
</script>

<style lang="stylus" scoped>
.wizard-wrap{
    height:80vh;

    .wizard-tab{
        height:100%;
    }
}
</style>