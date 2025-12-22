<template>
    <div class="manifset-wrap">
        <el-form ref="ruleFormRef"  :model="state.ruleForm" :rules="state.rules" label-width="140">
            <div class="inner scrollbar">
                <template v-for="(item,index) in fieldsArray">
                    <el-form-item :label="item.type == 'checkbox'?'':item.label" :prop="item.name">
                        <template v-if="item.type == 'input'">
                            <el-input v-model="state.ruleForm[item.name]" @change="handleChange(item.name)" />
                        </template>
                        <template v-else-if="item.type == 'textarea'">
                            <el-input type="textarea" 
                            :autosize="{ minRows: 2, maxRows: 6}" resize="none" 
                            v-model="state.ruleForm[item.name]" @change="handleChange(item.name)"/>
                        </template>
                        <template v-else-if="item.type == 'checkbox'">
                            <el-checkbox v-model="state.ruleForm[item.name]" @change="handleChange(item.name)">{{ item.label }}</el-checkbox>
                        </template>
                        <template v-else-if="item.type == 'select'">
                            <template v-if="item.create">
                                <el-select v-model="state.ruleForm[item.name]" filterable clearable allow-create 
                                multiple collapse-tags collapse-tags-tooltip :max-collapse-tags="19" @change="handleChange(item.name)">
                                    <el-option v-for="(option,index) in item.options" :key="index" :label="option.label" :value="option.value"></el-option>
                                </el-select>
                            </template>
                            <template v-else> 
                                <el-select v-model="state.ruleForm[item.name]" @change="handleChange(item.name)">
                                    <el-option v-for="(option,index) in item.options" :key="index" :label="option.label" :value="option.value"></el-option>
                                </el-select>
                            </template>
                        </template>
                    </el-form-item>
                </template>
            </div>
            <el-form-item>
                <el-button @click="handleCancel" :loading="state.loading">取消</el-button>
                <el-button type="primary" @click="handleSubmit" :loading="state.loading">确定保存</el-button>
            </el-form-item>
        </el-form>
    </div>
</template>

<script>
import { computed, onMounted, reactive, ref } from 'vue';
import { useProjects } from '../list';
import { fetchApi } from '@/api/api';
import { useLogger } from '../../logger';

export default {
    match:/^manifest$/,
    width:500,
    setup () {

        const fieldsArray = [
            {name: 'appname', label: '应用的唯一标识符', type: 'input',default:'',rules:[{required: true, message: '请填写唯一标识符', trigger: 'blur'}]},
            {name: 'version', label: '应用版本号', type: 'input',default:'0.0.1',rules:[{required: true, message: '请填写版本号', trigger: 'blur'}]},
            {name: 'display_name', label: '应用显示名', type: 'input',default:'',rules:[{required: true, message: '请填写显示名', trigger: 'blur'}]},
            {name: 'desc', label: '详细介绍支持HTML', type: 'textarea',default:'',rules:[{required: true, message: '请填写描述', trigger: 'blur'}]},
            {name: 'arch', label: '架构类型', type: 'select', options: [{label: 'x86_64', value: 'x86_64'}],default:'x86_64'},
            {name: 'source', label: '应用来源', type: 'select', options: [{label: '第三方应用', value: 'thirdparty'}],default:'thirdparty'},
            {name: 'maintainer', label: '开发者名', type: 'input',default:'',rules:[{required: true, message: '请填写开发者名', trigger: 'blur'}]},
            {name: 'maintainer_url', label: '开发者网站/联系方式', type: 'input',default:''},
            {name: 'distributor', label: '发布者名', type: 'input',default:'',rules:[{required: true, message: '请填写发布者名', trigger: 'blur'}]},
            {name: 'distributor_url', label: '发布者网站/联系方式', type: 'input',default:''},
            {name: 'os_min_version', label: '支持最低系统版本', type: 'input',default:''},
            {name: 'os_max_version', label: '支持最高系统版本', type: 'input',default:''},
            {name: 'ctl_stop', label: '显示启动/停止功能', type: 'checkbox',default:true},
            {name: 'install_type', label: '安装类型', type: 'select',options:[{label: '应用用户', value: ' '},{label: 'root用户', value: 'root'}],default:' '},
            {name: 'install_dep_apps', label: '依赖应用列表', type: 'select',options:[],create:true,default:''},
            {name: 'desktop_uidir', label: 'UI组件目录路径', type: 'input',default:'ui',rules:[{required: true, message: '请填写UI组件目录路径', trigger: 'blur'}]},
            {name: 'desktop_applaunchname', label: '应用中心启动入口',  type: 'select',options:[],default:''},
            {name: 'service_port', label: '占用端口', type: 'input',default:''},
            {name: 'checkport', label: '检查端口占用', type: 'checkbox',default:true},
            {name: 'disable_authorization_path', label: '是否禁用授权目录功能', type: 'checkbox',default:false},
        ];
        
        const logger = useLogger();
        const projects = useProjects();

        const rules = fieldsArray.reduce((json,item)=>{
            if(item.rules)
                json[item.name] = item.rules;
            return json;
        },{});
        const defaultJosn = fieldsArray.reduce((json,item)=>{
            if(item.name == 'install_dep_apps'){
                json[item.name] = item.default.split(':').filter(c=>c);
            }else{
                json[item.name] = item.default;
            }
            return json;
        },{});
        const contentJson = projects.value.current.content.split('\n').reduce((json,item)=>{
            const index = item.indexOf('=');
            if(index>0){
                const key = item.substring(0,index).trim();
                const value = item.substring(index+1).trim();

                if(key == 'install_dep_apps'){
                    json[key] = value.split(':').filter(c=>c);
                }else{
                    json[key] = value;
                }
            }
            return json;
        },{});
        const json = Object.assign(defaultJosn,contentJson);
        json.desc = json.desc.replace(/^"""|"""$/g,'');
        const state = reactive({
            ruleForm: json,
            rules: rules,
            loading: false,
        });

        const handleChange = (name)=>{
            if(name == 'desktop_uidir'){
                readUiEndpoint();
            }
        }

        const readUiEndpoint = ()=>{
            fetchApi('/files/read',{
                params:{path:`${projects.value.page.path}/app/${state.ruleForm.desktop_uidir}/config`},
                method:'GET',
                headers:{'Content-Type':'application/json'},
            }).then(res => res.json()).then(res => { 
                fieldsArray.filter(c=>c.name == 'desktop_applaunchname')[0].options = Object.keys(res['.url']).map(c=>{
                    return {label:c,value:c}
                });
            }).catch((e)=>{
                fieldsArray.filter(c=>c.name == 'desktop_applaunchname')[0].options = [];
                logger.value.error(`${e}`);
            })
        }

        const ruleFormRef = ref(null);
        const handleCancel = ()=>{
            projects.value.current.show = false;
        }
        const handleSubmit = ()=>{
            ruleFormRef.value.validate(valid => {
                if (valid) {
                    
                    const json = JSON.parse(JSON.stringify(state.ruleForm));
                    json.install_dep_apps = json.install_dep_apps.join(':');

                    const keys = Object.keys(json);
                    const maxlength = keys.map(c=>c.length).sort((a,b)=>b-a)[0];
                    const content = keys.reduce((arr,item)=>{
                        let value = json[item].trim();
                        if(value){
                            if(item == 'desc'){
                                value  = `"""${value}"""`
                            }
                            arr.push(`${item.padEnd(maxlength,' ')} = ${value}`);
                        }
                        return arr;
                    },[]).join('\n');
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

        onMounted(()=>{
            readUiEndpoint();
        })
    
        return {state,fieldsArray,ruleFormRef,handleChange,handleCancel,handleSubmit}
    }
}
</script>

<style lang="stylus" scoped>
.manifset-wrap{
    .inner{
        max-height: 70vh;
        padding:2rem 1rem;
    }
}
</style>