<template>
    <el-dialog v-model="state.show" :title="`快速编辑`" 
    :width="`${projects.editor.width+240}px`"
    :close-on-click-modal="false" :close-on-press-escape="false"  top="1vh" draggable  class="guide-dialog">
        <el-tabs type="border-card" tabPosition="left" v-model="state.key" @tab-change="handleChange" class="tabs">
            <template v-for="(item,index) in state.tabs.filter(c=>!c.exists_key || c.exists)">
                <el-tab-pane :label="item.label" :name="item.key" class="h-100">
                    <template #label>
                        <template v-if="state.changeds[item.key]">
                            <div class="red">{{ item.label }} <strong >*</strong> </div>
                        </template>
                        <template v-else>
                            <span>{{ item.label }}</span>
                        </template>
                    </template>
                    <Editor v-if="state.currents[item.key]" :path="`${state.root}/${item.key}`" :ref="`editor-${item.key}`"></Editor>
                </el-tab-pane>
            </template>
        </el-tabs>
        <div class="t-c mgt-1">
            <el-button @click="handleCancel" :loading="state.loading">取消</el-button>
            <template v-if="state.showSave">
                <el-button type="primary" @click="handleSave" :loading="state.loading">保存当前修改</el-button>
            </template>
        </div>
    </el-dialog>
</template>

<script>
import {getCurrentInstance, nextTick, onMounted, onUnmounted, reactive, watch } from 'vue';
import { useProjects } from '../list';
import Editor from './Editor.vue';
import { fetchApi, fetchDelete, fetchWrite } from '@/api/api';
import { ElMessage, ElMessageBox } from 'element-plus';
import { useLogger } from '../../logger';
export default {
    components:{Editor},
    props:['modelValue'],
    setup (props,{emit}) {

        const logger = useLogger();
        const projects = useProjects();
        const root = projects.value.page.root.slice();
        const $this = getCurrentInstance();
       
        const state = reactive({
            show:true,
            key:'manifest',
            root:root.join('/'),
            tabs:[
                {label:'1、应用清单',key:'manifest'},
                {label:'2、应用图标',key:'ICON_256.PNG'},
                {label:'3、应用权限',key:'config/privilege'},
                {label:'4、应用资源',key:'config/resource'},
                {label:'5、应用入口',key:'app/ui/config','exists_key':'ui','exists':false},
                {label:'6、入口图标',key:'app/ui/images/icon_256.png','exists_key':'ui','exists':false},
                {label:'7、用户向导',key:'wizard/install'},
                {label:'8、启停脚本',key:'cmd/main'},
                {label:'9、Docker',key:'app/docker/docker-compose.yaml','exists_key':'docker','exists':false},
                {label:'10、打包下载',key:'fnpack'},
                {label:'环境变量',key:'env'},
            ],
            currents:{},
            contents:{},
            changeds:{},
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

        const getExists = () => {
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
            state.currents[state.key] = '1';
            nextTick(()=>{
                if($this.refs[`editor-${state.key}`]){
                    $this.refs[`editor-${state.key}`][0].doLayout();
                }
            });
        }

        const handleCancel = () => {
            if(state.changeds[state.key]){
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
                const _ref = $this.refs[`editor-${state.key}`][0];
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
                            state.contents[state.key] = res.content;
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
            })
        }

        const saveBtnTimer = ()=>{
            clearTimeout(state.showSaveTimer);
            state.showSaveTimer = setTimeout(()=>{
                const key = state.key;
                if($this.refs[`editor-${key}`]){
                    const getContent = $this.refs[`editor-${key}`][0].getContent;
                    getContent().then((res)=>{
                        state.showSave = !!res;
                        if(state.showSave){
                            if(!state.contents[key]){
                                state.contents[key] = res.content;
                            }else{
                                if(res.changed !== undefined){
                                    state.changeds[key] = res.changed;
                                }else{
                                    state.changeds[key] = res.content != state.contents[key];
                                }
                            }
                        }
                    });
                }
                saveBtnTimer();
            },500);
        }

        onMounted(()=>{
            handleChange();
            getExists();
            saveBtnTimer();
        });
        onUnmounted(()=>{
            clearTimeout(state.showSaveTimer);
        });

        return {projects,state,handleChange,handleCancel,handleSave}
    }
}
</script>

<style lang="stylus">
.el-overlay-dialog{
    overflow: hidden !important;
}
.guide-dialog{
    max-width: 80%;
    height:90%;
    .el-dialog__body{
        height:calc(100% - 4rem);
    }
}
</style>
<style lang="stylus" scoped>
    .tabs{
        height:calc(100% - 4rem );
    }
</style>