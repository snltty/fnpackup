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
        </el-form>
    </div>
</template>

<script>
import { reactive, ref } from 'vue';
export default {
    match:/privilege$/,
    width:400,
    height:'auto',
    props:['path','content'],
    setup (props) {
        const json = Object.assign({username:'',groupname:''},JSON.parse(props.content));
        const state = reactive({
            ruleForm: json,
            rules: {},
            loading: false
        });

        const ruleFormRef = ref(null);
        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                resolve({
                    path:props.path,
                    content:JSON.stringify(state.ruleForm,null,2)
                });
            }); 
        }

        return {
            state,ruleFormRef,getContent
        }
    }
}
</script>

<style lang="stylus" scoped>
.privilege-wrap{
    padding:2rem;
    border:1px solid var(--main-border-color);
    border-radius:5px;
}
</style>