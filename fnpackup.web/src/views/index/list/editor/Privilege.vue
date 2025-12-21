<template>
    <div class="privilege-wrap">
        <el-form ref="ruleFormRef"  :model="state.ruleForm" :rules="state.rules" label-width="110">
            <el-form-item label="权限" prop="run-as">
                <el-select v-model="state.ruleForm.defaults['run-as']">
                    <el-option key="package" label="应用用户" value="package"></el-option>
                    <el-option key="root" label="root" value="root"></el-option>
                </el-select>
            </el-form-item>
            <el-form-item label="应用用户名" prop="username">
                <el-input v-model="state.ruleForm.username" />
            </el-form-item>
            <el-form-item label="应用用户组名" prop="groupname">
                <el-input v-model="state.ruleForm.groupname" />
            </el-form-item>
            <el-form-item>
                <el-button @click="handleCancel" :loading="state.loading">取消</el-button>
                <el-button type="primary" @click="handleSubmit" :loading="state.loading">确定保存</el-button>
            </el-form-item>
        </el-form>
    </div>
</template>

<script>
import { reactive, ref } from 'vue';
import { useProjects } from '../list';
import { useLogger } from '../../logger';
import { fetchApi } from '@/api/api';

export default {
    match:/privilege$/,
    width:400,
    setup () {
        const logger = useLogger();
        const projects = useProjects();
        const json = Object.assign({username:'',groupname:''},JSON.parse(projects.value.current.content));
        const state = reactive({
            ruleForm: json,
            rules: {},
            loading: false
        });

        const ruleFormRef = ref(null);
        const handleCancel = ()=>{
            projects.value.current.show = false;
        }
        const handleSubmit = ()=>{
            ruleFormRef.value.validate(valid => {
                if (valid) {
                    
                    const content = JSON.stringify(state.ruleForm,null,2);
                    state.loading = true;
                    fetchApi(`/files/write`,{
                        method:'POST',
                        headers:{'Content-Type':'application/json'},
                        body:JSON.stringify({
                            path:projects.value.current.path,
                            content:content
                        })
                    }).then(res => res.text()).then(res => {
                        state.loading = false;
                        if(res){
                            logger.value.error(res);
                        }else{
                            state.show = false;
                            logger.value.success(`保存成功`);
                            projects.value.load();
                        }
                    }).catch((e)=>{
                        state.loading = false;
                        logger.value.error(`${e}`);
                    })
                }
            })
        }

        return {
            state,ruleFormRef,handleCancel,handleSubmit
        }
    }
}
</script>

<style lang="stylus" scoped>
.privilege-wrap{
    padding:2rem;
}
</style>