<template>
    <div class="command-wrap">
        <el-tabs v-model="state.type" type="border-card" class="command-tab" @tab-change="handleChange">
            <template v-for="item in state.types">
                <el-tab-pane :label="item.label" :name="item.value" class="h-100">
                    <Source v-if="state.contents[item.value]" :content="state.contents[item.value]" :path="`${state.root}/${item.value}`" :ref="`source-${item.value}`"></Source>
                </el-tab-pane>
            </template>
        </el-tabs>
    </div>
</template>

<script>
import { getCurrentInstance, onMounted,  reactive } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import { fetchRead } from '@/api/api';
import Source from './Source.vue';
export default {
    match:/\/cmd/,
    width:900,
    components:{Source},
    props:['path'],
    setup (props) {
        const logger = useLogger();
        const projects = useProjects();
        const $this = getCurrentInstance();

        const paths =  (/\/cmd$/.test(props.path) ? `${props.path}/install_init` : props.path).split('/');
        const root = paths.filter((c,i)=>i<paths.length-1).join('/');
        const state = reactive({
            paths:paths,
            root: root,

            type:'',
            types:[
                {label:'主脚本',value:'main'},
                {label:'安装',value:'install_init'},
                {label:'安装回调',value:'install_callback'},
                {label:'卸载',value:'uninstall_init'},
                {label:'卸载回调',value:'uninstall_callback'},
                {label:'更新',value:'upgrade_init'},
                {label:'更新回调',value:'upgrade_callback'},
                {label:'配置',value:'config_init'},
                {label:'配置回调',value:'config_callback'},
            ],
            loading:false,
            contents:{}
        });
        const handleChange = (type) => {
            state.type = type;
            if(!state.contents[type]){
                loadContent(type);
            }
        };

        const loadContent = (type)=>{
            return new Promise((resolve,reject)=>{ 
                state.loading = true;
                fetchRead(`${state.root}/${type}`)
                .then(res => res.text()).then(res => {
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
                const _ref = $this.refs[`source-${state.type}`];
                if(_ref){
                    return _ref[0].getContent().then(resolve);
                }else{
                    resolve();
                }
            });
        }

        onMounted(()=>{ 
            handleChange(state.paths[state.paths.length-1]);
        });

        return {projects,state,handleChange,getContent}
    }
}
</script>

<style lang="stylus" scoped>
.command-wrap{
    height:80vh;

    .command-tab{
        height:100%;
    }
}
</style>