<template>
    <el-dialog v-model="state.show" :title="`编辑器[${state.remark}]`" :width="`${state.component?`${state.component.width}px`:'70%'}`" top="1vh" height="90%" style="max-width: 80rem;" 
    :close-on-click-modal="false" :close-on-press-escape="false"  draggable>
        <template v-if="state.component">
            <component :is="state.component"></component>
        </template>
    </el-dialog>
</template>

<script>
import { reactive, watch } from 'vue';
import Source from './Source.vue';
import { useProjects } from '../list';
import { useLogger } from '../../logger';
import { fetchApi } from '@/api/api';
import Manifest from './Manifest.vue';
export default {
    components:{Source,Manifest},
    setup () {

        const components = [
            Manifest
            ,Source
        ];

        const logger = useLogger();
        const projects = useProjects();
        const state = reactive({
            show:false,
            component:undefined,
            remark:''
        });
        watch(()=>projects.value.current.path,()=>{
            state.show = !!projects.value.current.path;
            state.remark = projects.value.current.remark;
            getContent();
        });
        watch(() => state.show, (val) => {
            if (!val) {
                setTimeout(() => {
                    projects.value.current.path = '';
                    projects.value.current.content =  '';
                }, 300);
            }
        });

        const getContent = ()=>{
            fetchApi(`/files/read`,{
                params:{path:projects.value.current.path},
                method:'GET',
                headers:{'Content-Type':'application/json'},
            }).then(res => res.text()).then(res => {
                projects.value.current.content = res;
                const paths = `${projects.value.current.path}`.split('/').filter(c=>c);
                const path = paths.filter((item,index)=>index>1).join('/');
                state.component = components.filter(c=>c.match.test(path))[0];
            }).catch((e)=>{
                logger.value.error(`${e}`);
            })
        }

        return {state}
    }
}
</script>

<style lang="stylus" scoped>

</style>