<template>
    <div class="uiconfig-wrap">
        <div class="head flex mgb-2">
            <el-select v-model="state.key" style="width: 20rem;" @change="handkeKeyChange">
                <template v-for="item in state.keys">
                    <el-option :label="item" :value="item"></el-option>
                </template>
            </el-select>
            <span style="width: 1rem;"></span>
            <el-button type="success" @click="handleAdd"><el-icon><Plus></Plus></el-icon></el-button>
            <el-button @click="handleEdit"><el-icon><EditPen></EditPen></el-icon></el-button>
            <span class="flex-1"></span>
            <el-button type="danger" v-if="state.keys.length > 1" @click="handleDelete"><el-icon><Delete></Delete></el-icon></el-button>
        </div>
        <div class="item">
            <el-form :model="state.ruleForm" :rules="state.rules" label-width="80">
                <el-form-item label="显示标题" prop="title">
                    <el-input v-model="state.ruleForm.title" />
                </el-form-item>
                <el-form-item label="显示图标" prop="icon">
                    <el-input v-model="state.ruleForm.icon" />
                </el-form-item>
                <el-form-item label="" label-width="0" class="mgb-0">
                    <el-row>
                        <el-col :span="9">
                            <el-form-item label="打开协议" prop="protocol">
                                <el-select v-model="state.ruleForm.protocol">
                                    <el-option key="http" label="http" value="http"></el-option>
                                    <el-option key="https" label="https" value="https"></el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>
                        <el-col :span="8">
                            <el-form-item label="端口" prop="port" label-width="40">
                                <el-input v-model="state.ruleForm.port" />
                            </el-form-item>
                        </el-col>
                        <el-col :span="7">
                            <el-form-item label="路径" prop="url" label-width="40">
                                <el-input v-model="state.ruleForm.url" />
                            </el-form-item>
                        </el-col>
                    </el-row>
                </el-form-item>
                <el-form-item label="">
                    <div class="flex w-100">
                        <span>{{ state.ruleForm.protocol }}://fn_ip:{{ state.ruleForm.port }}{{ state.ruleForm.url }}</span>
                        <span class="flex-1"></span>
                        <span>
                            <el-radio-group v-model="state.ruleForm.type">
                                <el-radio value="url" class="mgr-1">新标签打开</el-radio>
                                <el-radio value="iframe">内嵌打开</el-radio>
                            </el-radio-group>
                        </span>
                    </div>
                </el-form-item>
                <el-form-item label="访问权限" prop="allUsers">
                    <el-radio-group v-model="state.ruleForm.allUsers">
                        <el-radio :value="true" class="mgr-1">所有用户可访问</el-radio>
                        <el-radio :value="false">仅管理员可访问</el-radio>
                    </el-radio-group>
                </el-form-item>
                
                <template v-if="state.ruleForm.allUsers==false">
                    <el-form-item label="" label-width="0">
                        <el-row class="w-100">
                            <el-col :span="10">
                                <el-form-item label="访问权限" prop="accessPerm">
                                    <el-select v-model="state.ruleForm.control.accessPerm">
                                        <el-option key="editable" label="可编辑" value="editable"></el-option>
                                        <el-option key="readonly" label="只读" value="readonly"></el-option>
                                        <el-option key="hidden" label="隐藏" value="hidden"></el-option>
                                    </el-select>
                                </el-form-item>
                            </el-col>
                            <el-col :span="7">
                                <el-form-item label="端口" prop="portPerm" label-width="40">
                                    <el-select v-model="state.ruleForm.control.portPerm">
                                        <el-option key="editable" label="可编辑" value="editable"></el-option>
                                        <el-option key="readonly" label="只读" value="readonly"></el-option>
                                        <el-option key="hidden" label="隐藏" value="hidden"></el-option>
                                    </el-select>
                                </el-form-item>
                            </el-col>
                            <el-col :span="7">
                                <el-form-item label="路径" prop="pathPerm" label-width="40">
                                    <el-select v-model="state.ruleForm.control.pathPerm">
                                        <el-option key="editable" label="可编辑" value="editable"></el-option>
                                        <el-option key="readonly" label="只读" value="readonly"></el-option>
                                        <el-option key="hidden" label="隐藏" value="hidden"></el-option>
                                    </el-select>
                                </el-form-item>
                            </el-col>
                        </el-row>
                    </el-form-item>
                </template>
                
                <el-form-item label="文件右键" prop="noDisplay">
                    <el-radio-group v-model="state.ruleForm.noDisplay">
                        <el-radio :value="true" class="mgr-1">仅右键菜单显示</el-radio>
                        <el-radio :value="false">桌面和右键菜单显示</el-radio>
                    </el-radio-group>
                </el-form-item>
                <el-form-item label="关联类型" prop="fileTypes">
                    <el-select v-model="state.ruleForm.fileTypes" filterable clearable allow-create 
                    multiple collapse-tags collapse-tags-tooltip :max-collapse-tags="19">
                        <el-option v-for="item in state.fileTypes" :key="item" :label="item" :value="item"></el-option>
                    </el-select>
                </el-form-item>
            </el-form>
        </div>
        <el-form :model="state.ruleForm" :rules="state.rules" label-width="100" style="margin-top: 2rem;">
            <el-form-item>
                <el-button @click="handleCancel" :loading="state.loading">取消</el-button>
                <el-button type="primary" @click="handleSubmit" :loading="state.loading">确定保存</el-button>
            </el-form-item>
        </el-form>
    </div>
</template>

<script>
import { onMounted, reactive, ref } from 'vue';
import { useProjects } from '../list';
import { useLogger } from '../../logger';
import { fetchApi } from '@/api/api';
import {Plus,EditPen,Delete} from '@element-plus/icons-vue'
import { ElMessageBox } from 'element-plus';
export default {
    match:/\/ui\/config$/,
    width:550,
    components: {Plus,EditPen,Delete},
    setup () {
        const logger = useLogger();
        const projects = useProjects();

        const configJson = JSON.parse(projects.value.current.content);
        const urlJson = configJson['.url'];
        const keys = Object.keys(urlJson);
        const state = reactive({
            ruleForm: {},
            rules: {},
            loading: false,
            keys: keys,
            key:keys[0],
            keyIndex:0,
            fileTypes:[
                "txt", "md", "doc", "docx", "pdf", "rtf", "odt",
                "json", "xml", "csv", "yml", "yaml", "ini", "toml",
                "html", "htm", "css", "js", "py", "java", "cpp", "c", "h", "php",'cs',
                "jpg", "jpeg", "png", "gif", "svg", "bmp", "webp",
                "zip", "rar", "7z", "tar", "gz",
                "mp3", "mp4", "avi", "mov", "wav",
                "exe", "dmg", "iso", "log", "db", "sqlite", "ppt", "pptx", "xls", "xlsx"
            ],
        });

        const resetForm = ()=>{ 
            urlJson[state.key] = Object.assign(Object.assign({
                "noDisplay":true,
                "fileTypes":[],
                "control": {
                    "accessPerm": "readonly",
                    "portPerm": "readonly", 
                    "pathPerm": "readonly"
                }
            },urlJson[state.key]),{protocol:'http'});
            state.ruleForm = urlJson[state.key];
            state.keyIndex = keys.indexOf(state.key);
        }
        const handkeKeyChange = (item)=>{
            resetForm();
        }
        const handleCancel = ()=>{
            projects.value.current.show = false;
        }
        const handleSubmit = ()=>{
            const content = JSON.stringify(configJson,null,2);
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

        const handleAdd = ()=>{
            ElMessageBox.prompt(`输入名称,必须已{appname}.开头`, '添加入口', {
                confirmButtonText: '确认',
                cancelButtonText: '取消',
                draggable:true,
                inputValue: `${state.key.split('.')[0]}.`,
                customStyle: {
                    'vertical-align':'unset'
                },
            }).then(({ value }) => {
                if(!value) {
                    return;
                }
                urlJson[value] = JSON.parse(JSON.stringify(urlJson[state.keys[0]]));
                urlJson[value].title = value.split('.')[1];
                state.keys.push(value);
                state.key = value;
                resetForm();
            }).catch((e) => {
                logger.value.error(`${e}`);
            })
        }
        const handleEdit = ()=>{
            ElMessageBox.prompt(`输入名称,必须已{appname}.开头`, `修改[${state.key}]`, {
                confirmButtonText: '确认',
                cancelButtonText: '取消',
                draggable:true,
                inputValue: state.key,
                customStyle: {
                    'vertical-align':'unset'
                },
            }).then(({ value }) => {
                if(!value) {
                    return;
                }
                urlJson[value] = JSON.parse(JSON.stringify(urlJson[state.key]));
                delete urlJson[state.key];
                state.keys[state.keyIndex] = value;
                state.key = value;
                resetForm();
            }).catch((e) => {
                logger.value.error(`${e}`);
            })
        }
        const handleDelete = ()=>{
            ElMessageBox.confirm(`确定要删除[${state.key}]吗?`, '删除', {
                confirmButtonText: '确认',
                cancelButtonText: '取消',
                type: 'warning',
                draggable:true,
            }).then(() => {
                delete urlJson[state.key];
                state.keys = Object.keys(urlJson);
                state.key = state.keys[0];
                resetForm();
            }).catch((e) => {
                logger.value.error(`${e}`);
            })
        }

        onMounted(()=>{
            resetForm();
        })

        return {
            state,handkeKeyChange,handleCancel,handleSubmit,handleAdd,handleEdit,handleDelete
        }
    }
}
</script>

<style lang="stylus" scoped>
.uiconfig-wrap{
    padding:2rem;

    .item{
        padding:1rem;
        border:1px solid #eee;
    }
}
</style>