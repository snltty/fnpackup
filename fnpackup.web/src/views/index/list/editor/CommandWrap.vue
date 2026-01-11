<template>
    <div class="command-wrap">
        <el-tabs v-model="state.type" type="border-card" class="command-tab" @tab-change="handleChange">
            <template v-for="item in state.types">
                <el-tab-pane :label="item.label" :name="item.key" class="h-100">
                    <template #label>
                        <template v-if="state.changeds[item.key]">
                            <div class="red">{{ item.label }} <strong >*</strong> </div>
                        </template>
                        <template v-else>
                            <span>{{ item.label }}</span>
                        </template>
                    </template>
                    <Source v-if="state.contents[item.key]" :content="state.contents[item.key]" :path="`${state.root}/${item.key}`" :ref="`source-${item.key}`"></Source>
                </el-tab-pane>
            </template>
        </el-tabs>
    </div>
</template>

<script>
import { getCurrentInstance, onMounted,  onUnmounted,  reactive } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import { fetchFileRead } from '@/api/api';
import Source from './Source.vue';
export default {
    match:/\/cmd/,
    width:900,
    components:{Source},
    props:['path','content'],
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
                {label:'主脚本',key:'main'},
                {label:'安装',key:'install_init'},
                {label:'安装回调',key:'install_callback'},
                {label:'卸载',key:'uninstall_init'},
                {label:'卸载回调',key:'uninstall_callback'},
                {label:'更新',key:'upgrade_init'},
                {label:'更新回调',key:'upgrade_callback'},
                {label:'配置',key:'config_init'},
                {label:'配置回调',key:'config_callback'},
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
            if(!state.contents[type]){
                loadContent(type);
            }
            doLayout();
        };

        const loadContent = (type)=>{
            return new Promise((resolve,reject)=>{ 
                state.loading = true;
                fetchFileRead(`${state.root}/${type}`)
                .then(res => {
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
                const _ref = $this.refs[`source-${key}`];
                if(_ref){
                    _ref[0].getContent().then((res)=>{
                        resolve({
                            path:res.path,
                            content:res.content,
                            changed_key:key,
                            changed:Object.values(state.changeds).some(c=>c)
                        });
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
                if($this.refs[`source-${key}`]){
                    const getContent = $this.refs[`source-${key}`][0].getContent;
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

        return {projects,state,handleChange,getContent,setChangedContent}
    }
}
</script>

<style lang="stylus" scoped>
.command-wrap{
    height:100%;

    .command-tab{
        height:100%;
    }
}
</style>