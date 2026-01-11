<template>
    <el-dialog v-model="state.show" :title="`源码编辑器[${projects.editor.remark}]`" width="50%"
     top="1vh" 
    :close-on-click-modal="false" :close-on-press-escape="false"  draggable class="source-dialog">
        <template v-if="state.show">
            <template v-if="state.content !== undefined">
                <Source :path="state.path" :content="state.content" ref="source"></Source>
                <div class="t-c mgt-1">
                    <el-button type="primary" @click="handleSave" :loading="state.loading">确定保存</el-button>
                </div>
            </template>
        </template>
    </el-dialog>
</template>

<script>
import {onMounted, reactive, ref, watch } from 'vue';
import { useProjects } from '../list';
import Source from './Source.vue';
import { fetchRead, fetchWrite } from '@/api/api';
import { useLogger } from '../../logger';
import { ElMessage } from 'element-plus';
export default {
    components:{Source},
    props:['modelValue'],
    setup (props,{emit}) {
        const logger = useLogger();
        const projects = useProjects();

        const state = reactive({
            show: true,
            path: projects.value.editor.path,
            content: undefined,
            loading: false
        });
        watch(()=>state.show,(val)=>{
            if(!val){
                setTimeout(()=>{
                    emit('update:modelValue',val);
                },300);
            }
        })

        const source = ref(null);
        const handleSave = ()=>{
            source.value.getContent().then((res)=>{
                state.loading = true;
                fetchWrite(res.path,res.content)
                .then(res => res.text()).then(res => {
                    state.loading = false;
                    if(res){
                        logger.value.error(res);
                    }else{
                        state.show = false;
                        ElMessage.success('保存成功');
                        logger.value.success(`保存成功`);
                        projects.value.load();
                    }
                }).catch((e)=>{
                    state.loading = false;
                    logger.value.error(`${e}`);
                })
            });
        }

        const loadContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                state.loading = true;
                fetchRead(state.path)
                .then(res => res.text()).then(res => {
                    state.loading = false;
                    state.content = res;
                    resolve();
                }).catch((e)=>{
                    state.loading = false;
                    logger.value.error(`${e}`);
                    reject();
                });
            });
        }

        onMounted(()=>{ 
            loadContent();
        })

        return {projects,state,source,handleSave}
    }
}
</script>

<style lang="stylus">
.el-overlay-dialog{
    overflow: hidden !important;
}
.source-dialog{
    max-width: 80%;
    height:90%;
    .el-dialog__body {
        height: calc(100% - 8rem);
    }
}
</style>