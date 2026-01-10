<template>
    <el-tabs v-model="state.type" type="border-card" class="h-100 bs wizard-tab" @tab-change="handleChange">
        <template v-for="item in state.types">
            <el-tab-pane :label="item.label" :name="item.key" v-loading="state.loading" class="h-100 bs">
                <template #label>
                    <template v-if="state.changeds[item.key]">
                        <div class="red">{{ item.label }} <strong >*</strong> </div>
                    </template>
                    <template v-else>
                        <span>{{ item.label }}</span>
                    </template>
                </template>
                <Wizard v-if="state.contents[item.key]" :content="state.contents[item.key]" :path="`${state.root}/${item.key}`" :ref="`wizard-${item.key}`"></Wizard>
            </el-tab-pane>
        </template>
    </el-tabs>
</template>

<script>
import { getCurrentInstance, onMounted,  onUnmounted,  reactive } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import Wizard from './Wizard.vue';
import {fetchRead } from '@/api/api';

export default {
    match:/\/wizard/,
    width:600,
    components:{Wizard},
    props:['path','content'],
    setup (props) {
        const logger = useLogger();
        const projects = useProjects();
        const $this = getCurrentInstance();

        const paths =  (/\/wizard$/.test(props.path) ? `${props.path}/install` : props.path).split('/');
        const root = paths.filter((c,i)=>i<paths.length-1).join('/');

        const state = reactive({
            paths:paths,
            root: root,
            type:'',
            types:[
                {label:'安装向导',key:'install'},
                {label:'卸载向导',key:'uninstall'},
                {label:'更新向导',key:'upgrade'},
                {label:'配置向导',key:'config'}
            ],
            loading:false,
            contents:{},
            changeds:{},
            showChangeTimer:0
        });

        const doLayout = ()=>{ 
            projects.value.editor.remark = state.types.find(c=>c.key==state.type).label;
        }
        const handleChange = (type) => {
            state.type = type;
            loadContent(type);
            doLayout();
        };

       
        const loadContent = (type)=>{
            return new Promise((resolve,reject)=>{ 
                state.loading = true;
                state.paths[state.paths.length-1] = type;
                fetchRead(state.paths.join('/')).then(res => res.text()).then(res => {
                    state.loading = false;
                    state.contents[type] = res || '[]'; 
                    resolve();
                }).catch((e)=>{
                   state.loading = false;
                    logger.value.error(`${e}`);
                    reject();
                });
            });
        }
        const setChangedContent = (type,content)=>{
            state.contents[type] = content;
        }
        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                const key = state.type;
                const _ref = $this.refs[`wizard-${key}`];
                if(_ref){
                    _ref[0].getContent().then((res)=>{
                        resolve({
                            path:res.path,
                            content:res.content,
                            changed_key:key,
                            changed:Object.values(state.changeds).some(c=>c)
                        })
                    });
                }else{
                    resolve({
                        path:props.path,
                        content:state.contents[key] || '--',
                        changed:Object.values(state.changeds).some(c=>c)
                    });
                }
            });
        }

        const saveBtnTimer = ()=>{
            clearTimeout(state.showChangeTimer);
            state.showChangeTimer = setTimeout(()=>{
                const key = state.type;
                if($this.refs[`wizard-${key}`]){
                    const getContent = $this.refs[`wizard-${key}`][0].getContent;
                    getContent().then((res)=>{
                        state.changeds[key] = res.content != state.contents[key];
                    });
                }
                saveBtnTimer();
            },500);
        }

        onMounted(()=>{ 
            handleChange(state.paths[state.paths.length-1]);
            saveBtnTimer();
        });
        onUnmounted(()=>{
            clearTimeout(state.showChangeTimer);
        });

        return {projects,state,handleChange,getContent,setChangedContent,doLayout}
    }
}
</script>

<style lang="stylus" scoped>
</style>