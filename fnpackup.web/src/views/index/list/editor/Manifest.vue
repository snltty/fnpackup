<template>
    <div class="manifest-wrap h-100">
        <el-form ref="ruleFormRef"  :model="state.ruleForm" :rules="state.rules" label-width="140" class="h-100 flex flex-column flex-nowrap">
            <div class="flex-1 inner scrollbar">
                <template v-for="(item,index) in fieldsArray">
                    <el-form-item :label="item.type == 'checkbox'?'':item.label" :prop="item.name">
                        <template #label="{label}">
                            <div>
                                <span>{{ label }}</span>
                                <template v-if="item.help">
                                    <el-popover title="提示" placement="top" width="240">
                                        <template #reference>
                                            <el-icon size="12"><QuestionFilled></QuestionFilled></el-icon>
                                        </template>
                                        <div v-html="item.help"></div>
                                    </el-popover>
                                </template>
                            </div>
                        </template>
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
                            <template v-if="item.to">
                                <el-row class="w-100">
                                    <el-col :span="12">
                                        <template v-if="item.create">
                                            <el-select v-model="state.ruleForm[item.name]" :remote="item.remote" :filter-method="item.remoteFn || remoteFn"
                                            filterable clearable allow-create 
                                            multiple collapse-tags collapse-tags-tooltip :max-collapse-tags="19" @change="handleChange(item.name)">
                                                <el-option v-for="(option,index) in item.options" :label="option.label" :value="option.value"></el-option>
                                            </el-select>
                                        </template>
                                        <template v-else> 
                                            <el-select v-model="state.ruleForm[item.name]" @change="handleChange(item.name)">
                                                <el-option v-for="(option,index) in item.options" :key="index" :label="option.label" :value="option.value"></el-option>
                                            </el-select>
                                        </template>
                                    </el-col>
                                    <el-col :span="12" class="t-r"> 
                                        <el-button plain @click="handleTransform(index)">转为{{item.to.label}}</el-button>
                                    </el-col>
                                </el-row>
                            </template>
                            <template v-else>
                                <template v-if="item.create">
                                    <el-select v-model="state.ruleForm[item.name]" :remote="item.remote" :filter-method="item.remoteFn || remoteFn"
                                    filterable clearable allow-create 
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
                        </template>

                    </el-form-item>
                </template>
            </div>
        </el-form>
    </div>
</template>

<script>
import { onMounted, reactive, ref } from 'vue';
import { useProjects } from '../list';
import { fetchAppCenter, fetchFileRead, fetchProjectExists } from '@/api/api';
import { useLogger } from '../../logger';
import {  QuestionFilled } from '@element-plus/icons-vue';

export default {
    match:/manifest$/,
    width:500,
    components:{QuestionFilled},
    props:['path','content'],
    setup (props) {

        const logger = useLogger();
        const projects = useProjects();
        const contentJson = props.content.split('\n').reduce((json,item)=>{
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

        const archJson = {
            arch:{
                name: 'arch', label: '架构类型', type: 'select', options: [{label: 'x86_64', value: 'x86_64'}],default:'x86_64',
                to:{label:'platform',value:'platform'}
            },
            platform:{
                name: 'platform', label: '架构类型', type: 'select', 
                options: [
                    {label: '任意', value: 'all'},
                    {label: '仅x86', value: 'x86'},
                    {label: '仅arm', value: 'arm'},
                    {label: '仅loongarch', value: 'loongarch'},
                    {label: '仅risc-v', value: 'risc-v'},
                ],
                default:'x86', to:{label:'arch',value:'arch'}
            }
        }

        const staticHelp = [
            '可以填写app下的文件夹<br/>比如填写www则可通过<br/>',
            `{appname}.domain.com:${window.location.port}<br>`,
            `domain.com:${window.location.port}/{appname}<br>`,
            `访问app/www下的静态文件`
        ];

        const fieldsArray = ref([
            {name: 'appname', label: '应用的唯一标识符', type: 'input',default:'',rules:[{required: true, message: '请填写唯一标识符', trigger: 'blur'}]},
            {name: 'version', label: '应用版本号', type: 'input',default:'0.0.1',rules:[{required: true, message: '请填写版本号', trigger: 'blur'}]},
            {name: 'display_name', label: '应用显示名', type: 'input',default:'',rules:[{required: true, message: '请填写显示名', trigger: 'blur'}]},
            {name: 'desc', label: '详细介绍支持HTML', type: 'textarea',default:'',rules:[{required: true, message: '请填写描述', trigger: 'blur'}]},
            contentJson['arch'] !== undefined ? archJson['arch'] : archJson['platform'],
            {name: 'source', label: '应用来源', type: 'select', options: [{label: '第三方应用', value: 'thirdparty'}],default:'thirdparty'},
            {name: 'maintainer', label: '开发者名', type: 'input',default:'',rules:[{required: true, message: '请填写开发者名', trigger: 'blur'}]},
            {name: 'maintainer_url', label: '开发者网站/联系方式', type: 'input',default:''},
            {name: 'distributor', label: '发布者名', type: 'input',default:'',rules:[{required: true, message: '请填写发布者名', trigger: 'blur'}]},
            {name: 'distributor_url', label: '发布者网站/联系方式', type: 'input',default:''},
            {name: 'os_min_version', label: '支持最低系统版本', type: 'input',default:''},
            {name: 'os_max_version', label: '支持最高系统版本', type: 'input',default:''},
            {name: 'ctl_stop', label: '显示启动/停止功能', type: 'checkbox',default:true},
            {name: 'install_type', label: '安装类型', type: 'select',options:[{label: '应用用户', value: ' '},{label: 'root用户', value: 'root'}],default:' '},
            {name: 'install_dep_apps', label: '依赖应用列表', type: 'select',options:[],create:true,remote:true,remoteFn:(query)=>{
                fetchAppCenter(query)
                .then(res => {
                    if(res.code == 0){
                        fieldsArray.value[fieldsArray.value.findIndex(c=>c.name == 'install_dep_apps')].options = res.data.list.map(c=>{
                            return {label:c.name,value:c.appName};
                        });
                        return;
                    }
                    logger.value.error(res.msg);
                });
            },default:''},
            {name: 'desktop_uidir', label: 'UI组件目录路径', type: 'input',default:'ui'},
            {name: 'desktop_applaunchname', label: '应用中心启动入口',  type: 'select',options:[],default:''},
            {name: 'service_port', label: '占用端口', type: 'input',default:''},
            {name: 'checkport', label: '检查端口占用', type: 'checkbox',default:true},
            {name: 'disable_authorization_path', label: '是否禁用授权目录功能', type: 'checkbox',default:false},
            {name: 'fnpackup', label: '静态托管', type: 'input',default:'',help:staticHelp.join('')},
            {name: 'changelog', label: '应用更新日志', type: 'input',default:''},
            
        ]);
        const rules = fieldsArray.value.reduce((json,item)=>{
            if(item.rules)
                json[item.name] = item.rules;
            return json;
        },{});
        const defaultJosn = fieldsArray.value.reduce((json,item)=>{
            if(item.name == 'install_dep_apps'){
                json[item.name] = item.default.split(':').filter(c=>c);
            }else{
                json[item.name] = item.default;
            }
            return json;
        },{});

        const json = Object.assign(defaultJosn,contentJson);
        json.ctl_stop = json.ctl_stop == 'true';
        json.checkport = json.checkport == 'true';
        json.disable_authorization_path = json.disable_authorization_path == 'true';
        json.desc = json.desc.replace(/^"""|"""$/g,'');
        const state = reactive({
            ruleForm: json,
            rules: rules,
            loading: false,
            apps:[]
        });

        const handleChange = (name)=>{
            if(name == 'desktop_uidir'){
                readUiEndpoint();
            }
        }

        const readUiEndpoint = ()=>{
            fetchFileRead(`${projects.value.page.path.split('/')[1]}/app/${state.ruleForm.desktop_uidir}/config`)
            .then(res => {
                if(!res) return;
                res = JSON.parse(res); 
                fieldsArray.value.filter(c=>c.name == 'desktop_applaunchname')[0].options = Object.keys(res['.url']).map(c=>{
                    return {label:c,value:c}
                });
            }).catch((e)=>{
                fieldsArray.value.filter(c=>c.name == 'desktop_applaunchname')[0].options = [];
                logger.value.error(`${e}`);
            })
        }
        const remoteFn = ()=>{ 
        }

        const handleTransform = (index)=>{
            const from = fieldsArray.value[index];
            const to = archJson[from.to.value];

            fieldsArray.value[index] = to;
            delete state.ruleForm[from.name];
            state.ruleForm[to.name] = to.default;
        }

        const ruleFormRef = ref(null);
        const getContent = ()=>{
            return new Promise((resolve,reject)=>{ 
                ruleFormRef.value.validate(valid => {
                    if (!valid){
                        reject();
                    } else {
                        const json = JSON.parse(JSON.stringify(state.ruleForm));
                        json.install_dep_apps = json.install_dep_apps.join(':');

                        const keys = Object.keys(json);
                        const content = keys.reduce((arr,item)=>{
                            let value = json[item];
                            if(typeof value == 'string'){
                                value = value.trim();
                            }
                            if(value){
                                if(item == 'desc'){
                                    value  = `"""${value}"""`
                                }
                                arr.push(`${item}=${value}`);
                            }
                            return arr;
                        },[]).join('\n');
                        resolve({
                            content: content,
                            path: props.path
                        });
                    }
                })
            });
        }

        onMounted(()=>{
            readUiEndpoint();
            
            fetchAppCenter('',state.ruleForm.install_dep_apps.join(':'))
            .then(res => {
                if(res.code == 0){
                    fieldsArray.value[fieldsArray.value.findIndex(c=>c.name == 'install_dep_apps')].options = res.data.list.map(c=>{
                        return {label:c.name,value:c.appName};
                    });
                    return;
                }
                logger.value.error(res.msg);
            });
            
            fetchProjectExists().then(res=>{
                if(res.ui == false){
                    state.ruleForm.desktop_uidir = contentJson.desktop_uidir || '';
                    state.ruleForm.desktop_applaunchname = contentJson.desktop_applaunchname || '';
                }
            });
        })
    
        return {state,fieldsArray,ruleFormRef,handleChange,handleTransform,remoteFn,getContent}
    }
}
</script>

<style lang="stylus" scoped>
.manifest-wrap{
    .inner{
        padding:2rem 1rem;
        border:1px solid var(--main-border-color);
        border-radius:5px;
    }
}
</style>