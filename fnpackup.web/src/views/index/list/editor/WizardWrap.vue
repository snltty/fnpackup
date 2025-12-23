<template>
    <div class="wizard-wrap">
        <el-tabs v-model="state.current" type="border-card" class="wizard-tab">
            <el-tab-pane label="安装向导" name="install" v-loading="projects.current.loading" class="h-100">
                <Wizard v-if="projects.current.content && state.current=='install'" :type="state.current"></Wizard>
            </el-tab-pane>
            <el-tab-pane label="卸载向导" name="uninstall" v-loading="projects.current.loading" class="h-100">
                <Wizard v-if="projects.current.content && state.current=='uninstall'" :type="state.current"></Wizard>
            </el-tab-pane>
            <el-tab-pane label="更新向导" name="upgrade" v-loading="projects.current.loading" class="h-100">
                <Wizard v-if="projects.current.content && state.current=='upgrade'" :type="state.current"></Wizard>
            </el-tab-pane>
            <el-tab-pane label="配置向导" name="config" v-loading="projects.current.loading" class="h-100">
                <Wizard v-if="projects.current.content && state.current=='config'" :type="state.current"></Wizard>
            </el-tab-pane>
        </el-tabs>
    </div>
</template>

<script>
import { onMounted, reactive } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import Wizard from './Wizard.vue';
import { fetchApi } from '@/api/api';

export default {
    match:/\/wizard/,
    width:600,
    components:{Wizard},
    setup () {
        const logger = useLogger();
        const projects = useProjects();

        const arr = projects.value.current.path.split('/');
        const state = reactive({
            current:arr[arr.length-1] == 'wizard' ? 'install' : arr[arr.length-1]
        });
        const handleChange = (type) => {
            state.current = type;
            projects.value.current.remark = {
                'install':'用户安装向导',
                'uninstall':'用户卸载向导',
                'upgrade':'用户更新向导',
                'config':'用户配置向导'
            }[type];
            getContent();
        };

        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 

                projects.value.current.loading = true;
                projects.value.current.content = '';
                fetchApi(`/files/read`,{
                    params:{path:`${projects.value.current.path.replace('/wizard','')}/wizard/${state.current}`},
                    method:'GET',
                    headers:{'Content-Type':'application/json'},
                }).then(res => res.text()).then(res => {
                    projects.value.current.loading = false;
                    projects.value.current.content = res || '{}';
                    resolve();
                }).catch((e)=>{
                    projects.value.current.loading = false;
                    logger.value.error(`${e}`);
                    reject();
                });
            });
        }
        onMounted(()=>{ 
            getContent();
        });

        return {state,projects,handleChange}
    }
}
</script>

<style lang="stylus" scoped>
.wizard-wrap{
    height:70vh;

    .wizard-tab{
        height:100%;
    }
}
</style>