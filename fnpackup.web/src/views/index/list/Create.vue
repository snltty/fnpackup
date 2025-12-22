<template>
    <el-dialog v-model="state.show" title="创建应用" width="280" >
        <div>
            <el-form :model="state.createForm" :rules="state.createRules" ref="ruleFormRef"  label-width="50">
                <el-form-item label="名称" prop="name">
                    <el-input v-model="state.createForm.name"></el-input>
                </el-form-item>
                <el-form-item label="">
                    <el-radio-group v-model="state.createForm.docker">
                        <el-radio :value="true" class="mgr-1">Docker应用</el-radio>
                        <el-radio :value="false">原生应用</el-radio>
                    </el-radio-group>
                </el-form-item>
                <el-form-item label="">
                    <el-radio-group v-model="state.createForm.ui">
                        <el-radio :value="true" class="mgr-1">有ui入口</el-radio>
                        <el-radio :value="false">无入口</el-radio>
                    </el-radio-group>
                </el-form-item>
                <el-form-item>
                    <el-button @click="state.show = false">取消</el-button>
                    <el-button type="primary" @click="handleSubmit" :loading="state.loading">确认创建</el-button>
                </el-form-item>
            </el-form>
        </div>
    </el-dialog>
</template>

<script>
import { reactive, ref, watch } from 'vue';
import {Plus,Refresh} from '@element-plus/icons-vue'
import { useLogger } from '../logger';
import { useProjects } from './list';
import { fetchApi } from '@/api/api';
export default {
    props: ['modelValue'],
    emits: ['update:modelValue'],
    components: {Plus,Refresh},
    setup (props,{emit}) {
        
        const logger = useLogger();
        const projects = useProjects();
        const state = reactive({
            show:true,
            loading:false,
            createForm:{
                name:'',
                docker:true,
                ui:true
            },
            createRules:{
                name:[
                    {required: true, message: '请输入应用名', trigger: 'blur'}
                ]
            }
        });
        watch(() => state.show, (val) => {
            if (!val) {
                setTimeout(() => {
                    emit('update:modelValue', val);
                }, 300);
            }
        });

        const ruleFormRef = ref(null);
        const handleSubmit = ()=>{
            ruleFormRef.value.validate(valid => {
                if (valid) {
                    state.loading = true;
                    fetchApi(`/files/create`,{
                        method:'POST',
                        headers:{'Content-Type':'application/json'},
                        body:JSON.stringify(state.createForm)
                    }).then(res => res.text()).then(res => {
                        state.loading = false;
                        if(res && res.indexOf('Success! Created')<0){
                            logger.value.error(res);
                        }else{
                            state.show = false;
                            logger.value.success(res);
                            projects.value.load();
                        }
                    }).catch((e)=>{
                        state.loading = false;
                        logger.value.error(`${e}`);
                    })
                }
            })
        }
        return {state,ruleFormRef,handleSubmit}
    }
}
</script>

<style lang="stylus" scoped>
</style>