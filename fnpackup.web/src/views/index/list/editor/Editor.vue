<template>
    <template v-if="state.component">
        <component :is="state.component" :path="state.path" :content="state.content" ref="dom"></component>
    </template>
</template>

<script>
import { markRaw, nextTick, onMounted, reactive, ref } from 'vue';
import Source from './Source.vue';
import { useProjects } from '../list';
import { useLogger } from '../../logger';
import { fetchRead } from '@/api/api';
import Manifest from './Manifest.vue';
import IconWrap from './IconWrap.vue';
import Privilege from './Privilege.vue';
import Resource from './Resource.vue';
import UiConfig from './UIConfig.vue';
import WizardWrap from './WizardWrap.vue';
import CommandWrap from './CommandWrap.vue';
import Fnpack from './Fnpack.vue';
import Env from './Env.vue';
export default {
    components:{Privilege,UiConfig},
    props:['path'],
    setup (props) {

        const components = [
            Manifest,
            IconWrap,
            Privilege,
            Resource,
            UiConfig,
            WizardWrap,
            CommandWrap,
            Fnpack,
            Env,
            Source,
        ];

        const logger = useLogger();
        const projects = useProjects();
        const state = reactive({
            component:undefined,
            path:props.path,
            content:'',
            loading:false
        });

       
        const resetComponent = ()=>{
            const path = `${state.path}`.split('/').filter(c=>c).join('/');
            state.component = markRaw(components.filter(c=>c.match.test(path))[0]);
            doLayout();
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

        const dom = ref(null);
        const doLayout = ()=>{ 
            if(!state.component){
                return;
            }
            projects.value.editor.width = state.component.width;
            if(state.component.height !== undefined){
                if(state.component.height == 'auto'){
                    projects.value.editor.height = 'auto';
                }
                else{
                    projects.value.editor.height = `${state.component.height}px`;
                }
            }else{
                projects.value.editor.height = '';
            }
            if(dom.value && dom.value.doLayout){
                dom.value.doLayout();
            }
        }
        const setChangedContent = (type,content)=>{
            if(dom.value){
                dom.value.setChangedContent(type,content);
            }
        }
        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                if(!dom.value || !dom.value.getContent) resolve();
                else dom.value.getContent().then(resolve);
            });
        }

        onMounted(()=>{ 
            loadContent().then(()=>{
                resetComponent();
            });
        });

        return {state,projects,dom,getContent,setChangedContent,doLayout}
    }
}
</script>

<style lang="stylus" scoped>

</style>