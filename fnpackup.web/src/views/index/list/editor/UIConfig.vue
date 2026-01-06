<template>
    <el-tabs v-model="state.key" type="border-card" class="h-100 ui-tab"  editable @edit="handleEdit">
        <el-tab-pane :label="value._key" :name="value._id" :key="index" v-for="(value,index) in state.values" class="h-100">
            <el-form ref="ruleFormRef" label-width="80" class="ui-form h-100 flex flex-column flex-nowrap">
                <div class="fields flex-1 scrollbar mgb-1">
                    <UIConfigItem :item="value"></UIConfigItem>
                </div>
                <el-form-item label-width="0">
                    <div class="t-c w-100">
                        <el-button plain type="primary" @click="handleSubmit" :loading="state.loading">保存修改</el-button>
                    </div>
                </el-form-item>
            </el-form>
        </el-tab-pane>
    </el-tabs>
</template>

<script>
import {reactive } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import {Edit,CircleCloseFilled,CirclePlusFilled} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus';
import UIConfigItem from './UIConfigItem.vue';
import { fetchApi } from '@/api/api';
export default {
    match:/\/ui\/config$/,
    width:550,
    height:660,
    components: { Edit,CircleCloseFilled,CirclePlusFilled ,UIConfigItem},
    setup (props,{emit}) {

        const logger = useLogger();
        const projects = useProjects();

        const resetForm = (item) => { 
            const json = JSON.parse(JSON.stringify(item));
            return Object.assign(Object.assign({
                "noDisplay":false,
                "allUsers":true,
                "fileTypes":[],
                "control": {
                    "accessPerm": "readonly"
                }
            },json),{protocol:'http'});
        }

        const configJson = JSON.parse(projects.value.current.content);
        const urlJson = configJson['.url'];
        for(let j in urlJson){
            urlJson[j]._key = j;
            urlJson[j] = resetForm(urlJson[j]);
        }
        const values = Object.values(urlJson);
        values.forEach((item,index)=>{
            item._id = index;
        });
        const state = reactive({
            key:values[0]._id,
            values:values,
            loading:false
        });

        const handleSubmit = ()=>{
            const json = JSON.parse(JSON.stringify(configJson));
            json['.url'] = Object.values(json['.url']).reduce((json,item)=>{
                json[item._key] = item;
                delete item._key;
                delete item._id;
                return json;
            },{});
            const content = JSON.stringify(json,null,2);
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
                    ElMessage.success('保存成功');
                    logger.value.success(`保存成功`);
                    projects.value.load();
                }
            }).catch((e)=>{
                state.loading = false;
                logger.value.error(`${e}`);
            })
        }
    
        const handleEdit = (_id,action) => {
            if(action == 'add'){
                ElMessageBox.prompt(`输入入口标识,必须以{appname}.开头`, '添加入口', {
                    confirmButtonText: '确认',
                    cancelButtonText: '取消',
                    draggable:true,
                    inputValue: `${state.values.filter(item=>item._id == state.key)[0]._key.split('.')[0]}.`,
                    customStyle: {
                        'vertical-align':'unset'
                    },
                }).then(({ value }) => {
                    if(!value) {
                        return;
                    }
                    urlJson[value] = JSON.parse(JSON.stringify(urlJson[state.values[0]._key]));
                    urlJson[value].title = value.split('.')[1];
                    urlJson[value]._key = value;
                    urlJson[value]._id = Date.now();
                    state.key = urlJson[value]._id;
                    urlJson[value] = resetForm(urlJson[value]);
                    state.values = Object.values(urlJson);
                }).catch((e) => {
                    logger.value.error(`${e}`);
                })
            }else if(action == 'remove'){
                if(state.values.length <= 1){
                    return;
                }
                ElMessageBox.confirm('确定要删除该入口吗？', '提示', {
                    confirmButtonText: '确定',
                    cancelButtonText: '取消',
                    type: 'warning',
                    draggable:true,
                    customStyle: {
                        'vertical-align':'unset'
                    },
                }).then(() => {
                    const item = state.values.filter(item=>item._id == _id)[0];
                    delete urlJson[item._key];
                    state.values = Object.values(urlJson);
                    if(state.key == _id && state.values.length > 0){
                        state.key = state.values[0]._id;
                    }
                }).catch((e) => {
                    logger.value.error(`${e}`);
                });
            }
        }

        return {state,handleEdit,handleSubmit}
    }
}
</script>

<style lang="stylus" scoped>
.ui-tab{
    box-sizing: border-box;
}
.fields{
    padding:1rem;
    border:1px solid var(--main-border-color);;
    border-radius: 5px;
}
</style>