<template>
    <el-dialog v-model="projects.current.show" :title="`编辑器[${projects.current.remark}]`" :width="`${state.component?`${state.component.width}px`:'70%'}`" top="1vh" height="90%" style="max-width: 80rem;" 
    :close-on-click-modal="false" :close-on-press-escape="false"  draggable>
        <template v-if="state.component && projects.current.show">
            <component :is="state.component"></component>
        </template>
    </el-dialog>
</template>

<script>
import { markRaw, reactive, watch } from 'vue';
import Source from './Source.vue';
import { useProjects } from '../list';
import { useLogger } from '../../logger';
import { fetchApi } from '@/api/api';
import Manifest from './Manifest.vue';
import Icon from './Icon.vue';
import Privilege from './Privilege.vue';
import UiConfig from './UIConfig.vue';
import WizardWrap from './WizardWrap.vue';
export default {
    components:{Source,Manifest,Icon,Privilege,UiConfig,WizardWrap},
    setup () {

        const components = [
            Manifest,
            Icon,
            Privilege,
            UiConfig,
            WizardWrap,
            Source
        ];

        const logger = useLogger();
        const projects = useProjects();
        const state = reactive({
            component:undefined
        });
        watch(() => projects.value.current.show, (val) => {
            if (!val) {
                setTimeout(() => {
                    projects.value.current.path = '';
                    projects.value.current.content =  '';
                    projects.value.current.remark =  '';
                    projects.value.current.load =  true;
                }, 300);
            }else{
                if(projects.value.current.load){
                    state.component = null;
                    getContent().then(()=>{
                        resetComponent();
                    });
                }else{
                    resetComponent();
                }
            }
        });

        const resetComponent = ()=>{
            const path = `${projects.value.current.path}`.split('/').filter(c=>c).join('/');
            state.component = markRaw(components.filter(c=>c.match.test(path))[0]);
        }

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

        return {state,projects}
    }
}
</script>

<style lang="stylus" scoped>

</style>