<template>
    <el-dialog v-model="state.show" 
    :width="`${projects.editor.width}px`" :style="`height:${projects.editor.height || '90%'};`"
    :close-on-click-modal="false" :close-on-press-escape="false"  top="1vh"  draggable class="editor-dialog">
        <template #header>
            <div class="header">
                <template v-if="state.changed">
                    <span class="red">编辑器[{{ projects.editor.remark }}] <strong >*</strong> </span>
                </template>
                <template v-else>
                    <span>编辑器[{{ projects.editor.remark }}]</span>
                </template>
            </div>
        </template>
        <template v-if="state.show">
            <div class="tabs w-100">
                <Editor :path="projects.editor.path" ref="editor"></Editor>
            </div>
            <div class="t-c mgt-1">
                <el-button @click="handleCancel" :loading="state.loading">取消</el-button>
                <el-button  v-if="state.showSave" type="primary" @click="handleSave" :loading="state.loading">保存当前修改</el-button>
            </div>
        </template>
    </el-dialog>
</template>

<script>
import {getCurrentInstance, onMounted, onUnmounted, reactive,watch } from 'vue';
import { useProjects } from '../list';
import Editor from './Editor.vue';
import { fetchWrite } from '@/api/api';
import { ElMessage, ElMessageBox } from 'element-plus';
import { useLogger } from '../../logger';
export default {
    components:{Editor},
    props:['modelValue'],
    setup (props,{emit}) {

        const $this = getCurrentInstance();
        const logger = useLogger();
        const projects = useProjects();
        const state = reactive({
            show:true,
            loading:false,
            content:'',
            changed:false,
            showSave:false,
            showSaveTimer:0
        });
        watch(() => state.show, (val) => {
            if (!val) {
                setTimeout(() => {
                    emit('update:modelValue', val);
                }, 300);
            }
        });

        const handleCancel = () => {
            if(state.changed){
                ElMessageBox.confirm('存在尚未保存的修改，是否保存？', '提示', {
                    confirmButtonText: '保存关闭',
                    cancelButtonText: '直接关闭',
                    type: 'warning',
                    draggable:true,
                    customStyle: {
                        'vertical-align':'unset'
                    },
                })
                .then(()=>{
                    handleSave().then(()=>{
                        state.show = false;
                    });
                }).catch(()=>{
                    state.show = false;
                });
            }else{
                state.show = false;
            }
        }
        const handleSave = () => {
            return new Promise((resolve,reject)=>{
                state.loading = true;
                const _ref = $this.refs[`editor`];
                const getContent = _ref.getContent;
                const setChangedContent = _ref.setChangedContent;

                getContent().then((res)=>{   
                    fetchWrite(res.path,res.content)
                    .then(c=>c.text())
                    .then((msg)=>{
                        if(msg){
                            ElMessage.error('保存失败');
                            logger.value.error(msg);
                            reject();
                        }else{
                            state.content = res.content;
                            ElMessage.success('保存成功');
                            logger.value.success(`[${res.path}]保存成功`);
                            if(setChangedContent && res.changed_key){
                                setChangedContent(res.changed_key,res.content);
                            }
                            projects.value.load();
                            resolve();
                        }
                    }).catch(()=>{
                        reject();
                        ElMessage.error('操作失败');
                    }).finally(()=>{
                        state.loading = false;
                    });
                });
            });
        }

        const saveBtnTimer = ()=>{
            clearTimeout(state.showSaveTimer);
            state.showSaveTimer = setTimeout(()=>{
                const _ref = $this.refs[`editor`];
                if(_ref){
                    const getContent = _ref.getContent;
                    getContent().then((res)=>{
                        state.showSave = !!res;
                        if(state.showSave){
                            if(!state.content){
                                state.content = res.content;
                            }else{
                                if(res.changed !== undefined){
                                    state.changed = res.changed;
                                }else{
                                    state.changed = res.content != state.content;
                                }
                            }
                        }
                    });
                }
                saveBtnTimer();
            },500);
        }
        onMounted(()=>{
            saveBtnTimer();
        });
        onUnmounted(()=>{
            clearTimeout(state.showSaveTimer);
        });

        return {projects,state,handleCancel,handleSave}
    }
}
</script>

<style lang="stylus">
.el-overlay-dialog{
    overflow: hidden !important;
}
.editor-dialog{
    max-width: 80%;
    .el-dialog__body{
        height:calc(100% - 4rem);
    }
}
</style>
<style lang="stylus" scoped>
    .tabs{
        height:calc(100% - 4rem );
    }
    .header{
        font-size:1.6rem;
    }
</style>