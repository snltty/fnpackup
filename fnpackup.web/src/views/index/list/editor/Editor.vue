<template>
    <template v-if="state.component">
        <component :is="state.component" :plusHeight="plusHeight"></component>
    </template>
</template>

<script>
import { markRaw, onMounted, reactive } from 'vue';
import Source from './Source.vue';
import { useProjects } from '../list';
import { useLogger } from '../../logger';
import { fetchApi } from '@/api/api';
import Manifest from './Manifest.vue';
import IconWrap from './IconWrap.vue';
import Privilege from './Privilege.vue';
import UiConfig from './UIConfig.vue';
import WizardWrap from './WizardWrap.vue';
import CommandWrap from './CommandWrap.vue';
import Fnpack from './Fnpack.vue';
export default {
    components:{Privilege,UiConfig},
    props:['plusHeight'],
    setup () {

        const components = [
            Manifest,
            IconWrap,
            Privilege,
            UiConfig,
            WizardWrap,
            CommandWrap,
            Fnpack,
            Source
        ];

        const logger = useLogger();
        const projects = useProjects();
        const state = reactive({
            component:undefined
        });
        const resetComponent = ()=>{
            const path = `${projects.value.current.path}`.split('/').filter(c=>c).join('/');
            state.component = markRaw(components.filter(c=>c.match.test(path))[0]);
            projects.value.current.width = state.component.width;
            if(state.component.height !== undefined){
                projects.value.current.height = `${state.component.height}px`;
            }else{
                projects.value.current.height = '';
            }
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

        onMounted(()=>{ 
            getContent().then(()=>{
                resetComponent();
            });
        })

        return {state,projects}
    }
}
</script>

<style lang="stylus" scoped>

</style>