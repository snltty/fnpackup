<template>
    <el-tabs v-model="state.step" type="border-card" class="wizard-tab h-100 bs"  editable @edit="handleStepEdit">
        <el-tab-pane :label="step.stepTitle" :name="step._id" :key="index" v-for="(step,index) in state.steps" class="h-100">
            <el-form ref="ruleFormRef" :model="step.items" label-width="80" class="wizard-form h-100 flex flex-column flex-nowrap">
                <WizardPlusField :step="step"></WizardPlusField>
                <div class="fields flex-1 scrollbar">
                    <template v-if="step.items.length > 0">
                        <el-form-item v-for="(item,index) in step.items" label-width="0" class="field-item mgb-0">
                            <a href="javascript:;" class="action del" @click="handleDelField(step,index)"><el-icon><CircleCloseFilled></CircleCloseFilled></el-icon></a>
                            <a href="javascript:;" class="action add" @click="handleAddField(step,index)"><el-icon><CirclePlusFilled></CirclePlusFilled></el-icon></a>
                            <component :is="choiceComponent(item.type)" :item="item" :types="state.types"></component>
                            <template v-if="item.type != 'tips'">
                                <WizardValidate :item="item" :types="state.types" :vtypes="state.vtypes"></WizardValidate>
                            </template>
                        </el-form-item>
                    </template>
                    <template v-else>
                        <el-form-item label-width="0" >
                            <div class="t-c w-100 mgt-1">
                                <el-button plain @click="handleAddField(step,0)">添加字段</el-button>
                            </div>
                        </el-form-item>
                    </template>
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
import { ElMessageBox } from 'element-plus';
import WizardText from './WizardText.vue';
import WizardOptions from './WizardOptions.vue';
import WizardSwitch from './WizardSwitch.vue';
import WizardTips from './WizardTips.vue';
import WizardValidate from './WizardValidate.vue';
import WizardPlusField from './WizardPlusField.vue';
export default {
    props: ['type'],
    components: { Edit,CircleCloseFilled,CirclePlusFilled,WizardValidate,WizardPlusField },
    setup (props,{emit}) {

        const logger = useLogger();
        const projects = useProjects();

        const validateTypes = [
            {label: '必填',value: 'required',default:{_required:true}},
            {label: '范围',value: 'min',default:{_min:0,_max:0}},
            {label: '长度',value: 'len',default:{_len:0}},
            {label: '正则',value: 'pattern',default:{_pattern:''}},
        ];
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
            rules:[],
            options:[]
        }, types.reduce((json,item)=>{
            json[`_${item.value}`] = item.default;
            return json;
        },{}));

        const _default = JSON.parse(projects.value.current.content == '[]' ? JSON.stringify([{'stepTitle':'欢迎使用','items':[]}]) : projects.value.current.content);
        _default.forEach((step,index)=>{
            step._id = index;
            step.items.forEach((item,index)=>{
                Object.assign(defaultItem,item);
            });
            step['_plus_field'] = Object.keys(step).filter(c=>['stepTitle','items','_id'].indexOf(c) < 0).map(c=>{
                const value = step[c];
                delete step[c];
                return {
                    field:c,
                    value:value
                }
            });
        });
        const state = reactive({
            step:_default.length > 0 ? _default[0]._id : '',
            steps:_default,
            types:types,
            loading:false,
            vtypes:validateTypes
        });

        const choiceComponent = (type) =>{
            return [WizardText,WizardOptions,WizardSwitch,WizardTips].filter(c=>c.allowTypes.indexOf(type) >= 0)[0];
        };

        const handleStepEdit = (_id,action) => {
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
                    const _id = Date.now().toString();
                    state.steps.push({
                        '_id':_id,
                        'stepTitle':value,
                        'items':[]
                    });
                    state.step = _id;
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
                    state.steps = state.steps.filter(item=>item._id != _id);
                    if(state.step == _id && state.steps.length > 0){
                        state.step = state.steps[0]._id;
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

        const deleteField = (item)=>{
            Object.keys(item).filter(c=>c.startsWith('_')).forEach(c=>{
                delete item[c];
            });
        }
        const handleSubmit = () => {
            const arr = JSON.parse(JSON.stringify(state.steps.filter(c=>{
                return c.items.filter(item=>item.field != 'wizard_default').length > 0;
            })));
            arr.forEach(step=>{

                step._plus_field.forEach(item=>{
                    step[item.field] = item.value;
                });

                //删除步骤的辅助字段
                deleteField(step);

                const items = step.items;
                delete step.items;
                step.items = items;
                step.items.forEach(item=>{
                    //删除字段的辅助字段，每个字段类型有一个单独的初始值辅助字段
                    item.initValue = item[`_${item.type}`];
                    deleteField(item);

                    //删除验证的辅助字段，并将对应不同类型的辅助字段的值还原到真正字段
                    //比如{required:true,_requred:true,message:'',_required_message:'111'}->{required:true,message:'11'}
                    item.rules = item.rules.reduce((arr,rule)=>{
                        const keys = Object.keys(validateTypes.filter(c=>c.value == rule._type)[0].default);
                        arr.push(Object.assign(keys.reduce((json,value)=>{
                            json[value.replace(/_/g,'')] = rule[value];
                            return json;
                        },{}),{'message':rule[`_${rule._type}_message`]}));
                        return arr;
                    },[]);
                });
            });
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