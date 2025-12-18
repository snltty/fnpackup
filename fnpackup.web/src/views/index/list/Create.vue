<template>
    <el-dialog v-model="state.show" title="创建项目" width="340" >
        <div>
            <el-form :model="state.createForm" :rules="state.createRules" ref="ruleFormRef"  label-width="50">
                <el-form-item label="名称" prop="name">
                    <el-input v-model="state.createForm.name"></el-input>
                </el-form-item>
                <el-form-item label="">
                    <el-checkbox v-model="state.createForm.docker" label="Docker应用"/>
                    <el-checkbox v-model="state.createForm.ui" label="UI访问入口"/>
                </el-form-item>
                <el-form-item label="">
                    <el-checkbox v-model="state.createForm.wizardInstall" label="安装向导"/>
                    <el-checkbox v-model="state.createForm.wizardUninstall" label="卸载向导"/>
                    <el-checkbox v-model="state.createForm.wizardUpgrade" label="更新向导"/>
                    <el-checkbox v-model="state.createForm.wizardConfig" label="配置向导"/>
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
                ui:true,
                wizardInstall:false,
                wizardUninstall:false,
                wizardUpgrade:false,
                wizardConfig:false,
            },
            createRules:{
                name:[
                    {required: true, message: '请输入项目名', trigger: 'blur'}
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
                    fetch(`http://localhost:5083/files/create`,{
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
                    }).catch(()=>{
                        state.loading = false;
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