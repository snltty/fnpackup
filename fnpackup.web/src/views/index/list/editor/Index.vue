<template>
    <el-dialog v-model="state.show" title="编辑器" width="70%" top="1vh" height="90%" style="max-width: 80rem;" 
    :close-on-click-modal="false" :close-on-press-escape="false"  draggable >
        <el-tabs type="border-card">
            <el-tab-pane label="源码编辑">
                <Source></Source>
            </el-tab-pane>
            <el-tab-pane label="可视化编辑">
            </el-tab-pane>
        </el-tabs>
    </el-dialog>
</template>

<script>
import { onMounted, reactive, watch } from 'vue';
import Source from './Source.vue';
import { useProjects } from '../list';
export default {
    components:{Source},
    setup () {
        
        const projects = useProjects();
        const state = reactive({
            show:false
        });
        watch(()=>projects.value.current.path,()=>{
            state.show = !!projects.value.current.path;
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
            fetch(`http://localhost:5083/files/read?path=${projects.value.current.path}`,{
                method:'GET',
                headers:{'Content-Type':'application/json'},
            }).then(res => res.text()).then(res => {
                projects.value.current.content = res;
            }).catch(()=>{
            })
        }

        return {state}
    }
}
</script>

<style lang="stylus" scoped>

</style>