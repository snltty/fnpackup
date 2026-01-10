<template>
    <div class="resource-wrap h-100 scrollbar">
        <el-form ref="ruleFormRef"  :model="state.ruleForm" :rules="state.rules" label-width="110">
            <ResourceDocker v-if="state.docker" :data="state.ruleForm['docker-project']['projects']"></ResourceDocker>
            <ResourceShare :data="state.ruleForm['data-share']['shares']"></ResourceShare>
            <ResourceLinker :data="state.ruleForm['usr-local-linker']"></ResourceLinker>
        </el-form>
    </div>
</template>

<script>
import { onMounted, reactive, ref } from 'vue';
import ResourceDocker from './ResourceDocker.vue';
import ResourceShare from './ResourceShare.vue';
import ResourceLinker from './ResourceLinker.vue';
import { useProjects } from '../list';
import { fetchApi } from '@/api/api';
export default {
    match:/resource$/,
    width:600,
    props:['path','content'],
    components:{ResourceDocker,ResourceShare,ResourceLinker},
    setup (props) {

        const projects = useProjects();
        const root = projects.value.page.root.slice();
        const json = Object.assign({
            "docker-project":{
                'projects':[]
            },
            'data-share':{
                'shares':[]
            },
            'usr-local-linker':{

            }
        },JSON.parse(props.content));
        const state = reactive({
            ruleForm: json,
            rules: {},
            loading: false,
            docker:false
        });

        const ruleFormRef = ref(null);
        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                const json  = JSON.parse(JSON.stringify(state.ruleForm));
                if(json['docker-project']['projects'].length==0){
                    delete json['docker-project'];
                }
                json['data-share']['shares'].forEach(element => {
                    if(element.permission.ro && element.permission.ro.length==0){
                        delete element.permission.ro;
                    }
                    if(element.permission.rw && element.permission.rw.length==0){
                        delete element.permission.rw;
                    }
                });
                if(json['data-share']['shares'].length==0){
                    delete json['data-share'];
                }
                if(json['usr-local-linker']['bin'] && json['usr-local-linker']['bin'].length==0){
                    delete json['usr-local-linker']['bin'];
                }
                if(json['usr-local-linker']['lib'] && json['usr-local-linker']['lib'].length==0){
                    delete json['usr-local-linker']['lib'];
                }
                if(json['usr-local-linker']['etc'] && json['usr-local-linker']['etc'].length==0){
                    delete json['usr-local-linker']['etc'];
                }
                if(Object.keys(json['usr-local-linker']).length==0){
                    delete json['usr-local-linker'];
                }
                resolve({
                    path:props.path,
                    content:JSON.stringify(json,null,2)
                });
            }); 
        }

        const getExists = () => {
            fetchApi(`/files/exists`,{
                params:{name:root[1]},
                method:'GET',
                headers:{'Content-Type':'application/json'},
            }).then(res => res.json()).then(res => {
                state.docker = res.docker;
            });
        }
        onMounted(()=>{
            getExists(); 
        });


        return {
            state,ruleFormRef,getContent
        }
    }
}
</script>

<style lang="stylus" scoped>
.resource-wrap{
    padding:2rem;
    border:1px solid var(--main-border-color);
    border-radius:5px;
    box-sizing:border-box;
}
</style>