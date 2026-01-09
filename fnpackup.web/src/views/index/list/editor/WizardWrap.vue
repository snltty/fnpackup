<template>
    <el-tabs v-model="state.type" type="border-card" class="h-100 bs wizard-tab" @tab-change="handleChange">
        <template v-for="item in state.types">
            <el-tab-pane :label="item.label" :name="item.value" v-loading="state.loading" class="h-100 bs">
                <Wizard v-if="state.contents[item.value]" :content="state.contents[item.value]" :path="`${state.root}/${item.value}`" :ref="`wizard-${item.value}`"></Wizard>
            </el-tab-pane>
        </template>
    </el-tabs>
</template>

<script>
import { getCurrentInstance, onMounted,  reactive } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import Wizard from './Wizard.vue';
import {fetchRead } from '@/api/api';

export default {
    match:/\/wizard/,
    width:600,
    components:{Wizard},
    props:['path'],
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
                {label:'安装向导',value:'install'},
                {label:'卸载向导',value:'uninstall'},
                {label:'更新向导',value:'upgrade'},
                {label:'配置向导',value:'config'}
            ],
            loading:false,
            contents:{}
        });
        const handleChange = (type) => {
            state.type = type;
            loadContent(type);
        };

        const loadContent = (type)=>{
            return new Promise((resolve,reject)=>{ 
                state.loading = true;
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
        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                const _ref = $this.refs[`wizard-${state.type}`];
                if(_ref){
                    return _ref[0].getContent().then(resolve);
                }else{
                    resolve();
                }
            });
        }

        onMounted(()=>{ 
            handleChange(state.paths[state.paths.length-1])
        });

        return {projects,state,handleChange,getContent}
    }
}
</script>

<style lang="stylus" scoped>
</style>