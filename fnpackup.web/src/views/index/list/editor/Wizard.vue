<template>
    <el-tabs v-model="state.step" type="border-card" class="wizard-tab"  editable @edit="handleStepEdit">
        <el-tab-pane :label="step.stepTitle" :name="step.id" :key="index" v-for="(step,index) in state.steps" class="h-100">
            <el-form ref="ruleFormRef" :model="step.items" label-width="80" class="wizard-form h-100 flex flex-column flex-nowrap">
                <el-form-item label="步骤标题" class="mgb-0">
                    <el-input v-model="step.stepTitle" ></el-input>
                </el-form-item>
                <div class="fields flex-1 scrollbar-4">
                    <template v-if="step.items.length > 0">
                        <el-form-item v-for="(item,index) in step.items" label-width="0" class="field-item mgb-0">
                            <a href="javasceipt:;" class="action del" @click="handleDelField(step,index)"><el-icon><CircleCloseFilled></CircleCloseFilled></el-icon></a>
                            <a href="javasceipt:;" class="action add" @click="handleAddField(step,index)"><el-icon><CirclePlusFilled></CirclePlusFilled></el-icon></a>
                            <component :is="choiceComponent(item.type)" :item="item" :types="state.types"></component>
                        </el-form-item>
                    </template>
                    <template v-else>
                        <el-form-item label-width="0" >
                            <div class="t-c w-100 mgt-1">
                                <el-button @click="handleAddField(step,0)">添加字段</el-button>
                            </div>
                        </el-form-item>
                    </template>
                </div>
                <el-form-item label-width="0" v-if="step.items.length > 0">
                    <div class="t-c w-100">
                        <el-button type="primary" @click="handleSubmit" :loading="state.loading">保存修改</el-button>
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
import WizardText from './WizardText.vue';
import WizardOptions from './WizardOptions.vue';
import WizardSwitch from './WizardSwitch.vue';
import WizardTips from './WizardTips.vue';
export default {
    props: ['type'],
    components: { Edit,CircleCloseFilled,CirclePlusFilled },
    setup (props,{emit}) {

        const logger = useLogger();
        const projects = useProjects();

        const types = [
            {label:'文本框',value:'text',default:''},
            {label:'密码框',value:'password',default:''},
            {label:'单选按钮',value:'radio',default:''},
            {label:'多选框',value:'checkbox',default:[]},
            {label:'下拉框',value:'select',default:''},
            {label:'开关',value:'switch',default:true},
            {label:'提示文本',value:'tips',default:''},
        ];
        const defaultItem = Object.assign({
            type:'text',
            field:'wizard_default',
            label:'示例项',
            initValue:'',
            rules:[{required:true,message:'请输入内容'}],
            options:[]
        }, types.reduce((json,item)=>{
            json[`_${item.value}`] = item.default;
            return json;
        },{}));

        const _default = JSON.parse(projects.value.current.content);
        _default.forEach((item,index)=>{
            item.id = index;
        });
        const state = reactive({
            step:_default.length > 0 ? _default[0].id : '',
            steps:_default,
            types:types,
            loading:false
        });

        const choiceComponent = (type) =>{
            return [WizardText,WizardOptions,WizardSwitch,WizardTips].filter(c=>c.allowTypes.indexOf(type) >= 0)[0];
        };

        const handleStepEdit = (id,action) => {
            if(action == 'add'){
                ElMessageBox.prompt('请输入步骤标题','添加步骤',{
                    confirmButtonText: '确定',
                    cancelButtonText: '取消',
                    draggable:true,
                    customStyle: {
                        'vertical-align':'unset'
                    },
                }).then(({ value }) => {
                    if(!value) return;
                    const id = Date.now().toString();
                    state.steps.push({
                        'id':id,
                        'stepTitle':value,
                        'items':[]
                    });
                    state.step = id;
                }).catch((e) => {
                    logger.value.error(`${e}`);
                });
            }else if(action == 'remove'){
                ElMessageBox.confirm('确定要删除该步骤吗？', '提示', {
                    confirmButtonText: '确定',
                    cancelButtonText: '取消',
                    type: 'warning',
                    draggable:true,
                    customStyle: {
                        'vertical-align':'unset'
                    },
                }).then(() => {
                    state.steps = state.steps.filter(item=>item.id != id);
                    if(state.step == id && state.steps.length > 0){
                        state.step = state.steps[0].id;
                    }
                }).catch((e) => {
                    logger.value.error(`${e}`);
                });
            }
        }

        const handleDelField = (step,index)=>{
            if(step.items[index].field == 'wizard_default'){
                step.items.splice(index,1);
                return;
            }
            ElMessageBox.confirm(`确定要删除[${step.items[index].field}]字段吗？`, '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning',
                draggable:true,
                customStyle: {
                    'vertical-align':'unset'
                },
            }).then(() => {
                step.items.splice(index,1);
            }).catch((e) => {
                logger.value.error(`${e}`);
            });
        }
        const handleAddField = (step,index) => {
            step.items.splice(index+1,0,JSON.parse(JSON.stringify(defaultItem)));
        }

        const handleSubmit = () => {
            const arr = JSON.parse(JSON.stringify(state.steps.filter(c=>{
                return c.items.filter(item=>item.field != 'wizard_default').length > 0;
            })));
            arr.forEach(c=>{
                delete c['id'];
                c.items.forEach(item=>{
                    item.initValue = item[`_${item.type}`];
                    types.forEach(type=>{
                        delete item[`_${type.value}`];
                    });
                });
            });
            if(arr.length == 0) {
                ElMessage.error('未配置表单字段');
                return;
            }
            emit('save',JSON.stringify(arr,null,2));
        }


        return {state,choiceComponent,handleStepEdit,handleDelField,handleAddField,handleSubmit}
    }
}
</script>

<style lang="stylus" scoped>
.wizard-tab{

    .fields{
        border:1px solid #ddd;
        margin:1rem 0;
    }
    
    .field-item{
        margin-bottom:1rem;
        border-bottom:1px solid #ddd;
        padding:.5rem .5rem .5rem 0;
        position:relative;
        &:nth-child(odd){
            background-color:#fafafa;
        }
        &:hover{
            background-color:#f5f5f5;
            .action{
                display:block;
            }
        }

        .action{
            display:none;
            position:absolute;
            left:50%;   
            bottom:-2.3rem;
            z-index 999
            &.del{
                color:red;
                margin-left:-2rem;
            }
            &.add{
                color:green;
                margin-left:2rem;
            }
        }
    }
}
</style>