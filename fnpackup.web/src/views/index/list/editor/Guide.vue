<template>
    <el-dialog v-model="projects.current.guide" :title="`快速编辑`" :width="`${projects.current.width+200}px`" top="1vh" class="guide-dialog"
    :close-on-click-modal="false" :close-on-press-escape="false"  draggable style="max-width: 80%;height:90%">
        <el-tabs type="border-card" tabPosition="left" v-model="state.key" @tab-change="handleChange" class="h-100">
            <template v-for="(item,index) in state.tabs.filter(c=>!c.exists_key || c.exists)">
                <el-tab-pane :label="item.label" :name="item.key" v-loading="projects.current.loading" class="h-100">
                    <Editor v-if="projects.current.path && state.key == item.key"></Editor>
                </el-tab-pane>
            </template>
        </el-tabs>
    </el-dialog>
</template>

<script>
import {onMounted, reactive, watch } from 'vue';
import { useProjects } from '../list';
import Editor from './Editor.vue';
import { fetchApi } from '@/api/api';
export default {
    components:{Editor},
    setup () {

        const projects = useProjects();

        const root = projects.value.page.path.split('/').filter((c,i)=>i<=1);

        watch(() => projects.value.current.guide, (val) => {
            if (!val) {
                setTimeout(() => {
                    projects.value.current.path = '';
                    projects.value.current.content =  '';
                    projects.value.current.remark =  '';
                }, 300);
            }
        });
        const state = reactive({
            key:'manifest',
            tabs:[
                {label:'应用清单',key:'manifest'},
                {label:'应用图标',key:'ICON_256.PNG'},
                {label:'应用权限',key:'config/privilege'},
                {label:'应用资源',key:'config/resource'},
                {label:'应用入口',key:'app/ui/config','exists_key':'ui','exists':false},
                {label:'入口图标',key:'app/ui/images/icon_256.png','exists_key':'ui','exists':false},
                {label:'用户向导',key:'wizard/install'},
                {label:'启停脚本',key:'cmd/main'},
                {label:'Docker',key:'app/docker/docker-compose.yaml','exists_key':'docker','exists':false},
                {label:'打包下载',key:'fnpack'}
            ]
        });
        const getExists = (path) => {
            fetchApi(`/files/exists`,{
                params:{name:root[1]},
                method:'GET',
                headers:{'Content-Type':'application/json'},
            }).then(res => res.json()).then(res => {
                for(let j in res){
                    state.tabs.filter(c=>c.exists_key == j).forEach(c=>{
                        c.exists = res[j];
                    });
                }
            });
        }
        const handleChange = () => {
            root[2] = state.key;
            projects.value.current.path = root.join('/');
        }
        onMounted(()=>{
            handleChange();
            getExists();
        });

        return {projects,state,handleChange}
    }
}
</script>

<style lang="stylus">
.el-overlay-dialog{
    overflow: hidden !important;
}
.guide-dialog{
    .el-dialog__body{
        height: calc(100% - 40px);
    }
}
</style>