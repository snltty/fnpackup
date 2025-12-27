<template>
    <el-dialog v-model="projects.current.source" :title="`源码编辑器[${projects.current.remark}]`" width="50%" top="1vh" 
    :close-on-click-modal="false" :close-on-press-escape="false"  draggable class="editor-dialog" style="max-width: 80%;height:90%">
        <template v-if="projects.current.source">
            <Source plusHeight="110"></Source>
        </template>
    </el-dialog>
</template>

<script>
import {onMounted, watch } from 'vue';
import { useProjects } from '../list';
import Source from './Source.vue';
import { fetchApi } from '@/api/api';
import { useLogger } from '../../logger';
export default {
    components:{Source},
    setup () {
        const logger = useLogger();
        const projects = useProjects();
        watch(() => projects.value.current.source, (val) => {
            if (!val) {
                setTimeout(() => {
                    projects.value.current.path = '';
                    projects.value.current.content =  '';
                    projects.value.current.remark =  '';
                }, 300);
            }
        });

        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                projects.value.current.loading = true;
                fetchApi(`/files/read`,{
                    params:{path:projects.value.current.path},
                    method:'GET',
                    headers:{'Content-Type':'application/json'},
                }).then(res => res.text()).then(res => {
                    projects.value.current.loading = false;
                    projects.value.current.content = res;
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
        })

        return {projects}
    }
}
</script>

<style lang="stylus">
.editor-dialog{
    
    .el-dialog__body{
        height:100%;
    }
}
</style>