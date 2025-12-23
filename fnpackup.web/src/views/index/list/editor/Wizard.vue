<template>
    <el-tabs v-model="state.step" type="border-card" class="wizard-tab"  editable @edit="handleStepEdit">
        <el-tab-pane :label="step.stepTitle" :name="step.id" :key="index" v-for="(step,index) in state.steps" class="h-100">
            <el-form ref="ruleFormRef" :model="step.items" label-width="80" class="wizard-form h-100 flex flex-column flex-nowrap">
                <el-form-item label="步骤标题" class="mgb-0">
                    <el-input v-model="step.stepTitle" ></el-input>
                </el-form-item>
                <div class="fields flex-1 scrollbar-10">
                    <el-form-item v-for="(item,index) in step.items" label-width="0" class="field-item">
                        <a href="javasceipt:;" class="action del" @click="handleDelField(step,index)"><el-icon><CircleCloseFilled></CircleCloseFilled></el-icon></a>
                        <a href="javasceipt:;" class="action add" @click="handleAddField(step,index)"><el-icon><CirclePlusFilled></CirclePlusFilled></el-icon></a>
                        <template v-if="item.type == 'text' || item.type == 'password'">
                            <WizardText :item="item" :types="state.types"></WizardText>
                        </template>
                        <template v-else-if="item.type == 'radio' || item.type == 'checkbox' || item.type == 'select'">
                            <WizardOptions :item="item" :types="state.types"></WizardOptions>
                        </template>
                        <template v-else-if="item.type == 'switch'">
                            <WizardSwitch :item="item" :types="state.types"></WizardSwitch>
                        </template>
                        <template v-else-if="item.type == 'tips'">
                            <WizardTips :item="item" :types="state.types"></WizardTips>
                        </template>
                    </el-form-item>
                </div>
                <el-form-item label-width="0">
                    <div class="t-c w-100">
                        <el-button type="primary">保存修改</el-button>
                    </div>
                </el-form-item>
            </el-form>
        </el-tab-pane>
    </el-tabs>
</template>

<script>
import { reactive } from 'vue';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import {Edit,CircleCloseFilled,CirclePlusFilled} from '@element-plus/icons-vue'
import { ElMessageBox } from 'element-plus';
import WizardText from './WizardText.vue';
import WizardOptions from './WizardOptions.vue';
import WizardSwitch from './WizardSwitch.vue';
import WizardTips from './WizardTips.vue';
export default {
    props: ['type'],
    components: {WizardText,WizardOptions,WizardSwitch,WizardTips,
        Edit,CircleCloseFilled,CirclePlusFilled
    },
    setup (props) {
        
        const logger = useLogger();
        const projects = useProjects();

        const titles = {
            install:'1、欢迎安装',
            uninstall:'1、感谢使用',
            upgrade:'1、欢迎更新',
            config:'1、欢迎配置'
        };
        const types = [
            {label:'文本框',value:'text',default:''},
            {label:'密码框',value:'password',default:''},
            {label:'单选按钮',value:'radio',default:''},
            {label:'多选框',value:'checkbox',default:[]},
            {label:'下拉框',value:'select',default:''},
            {label:'开关',value:'switch',default:true},
            {label:'提示文本',value:'tips',default:''},
        ];
        const plusFields = types.reduce((json,item)=>{
            json[`_${item.value}`] = item.default;
            return json;
        },{});

        const defaultItem = Object.assign({
            type:'text',
            field:'wizard_default',
            label:'示例项',
            initValue:'',
            rules:[{required:true,message:'请输入内容'}],
        },plusFields);

        const _default = projects.value.current.content && projects.value.current.content!='{}' 
        ? JSON.parse(projects.value.current.content)
        : JSON.parse(JSON.stringify([
            {
                id:Date.now().toString(),
                stepTitle:titles[props.type],
                'items':[JSON.parse(JSON.stringify(defaultItem))]
            }
        ]));

        const state = reactive({
            step:_default[0].id,
            steps:_default,
            types:types
        });

        const handleStepEdit = (name,action) => {
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
                    state.steps = state.steps.filter(item=>item.id != name);
                    if(state.steps.length == 0){
                        state.steps.push({
                            'id':Date.now().toString(),
                            'stepTitle':titles[props.type],
                            'items':[]
                        });
                    }
                    if(state.step == name){
                        state.step = state.steps[0].id;
                    }
                }).catch((e) => {
                    logger.value.error(`${e}`);
                });
            }
        }

        const handleDelField = (step,index)=>{
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
                if(step.items.length == 0){
                    step.items.push(JSON.parse(JSON.stringify(defaultItem)));
                }
            }).catch((e) => {
                logger.value.error(`${e}`);
            });
        }
        const handleAddField = (step,index) => {
            step.items.splice(index+1,0,JSON.parse(JSON.stringify(defaultItem)));
        }


        return {state,handleStepEdit,handleDelField,handleAddField}
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

        .action{
            position:absolute;
            left:50%;   
            bottom:-2.3rem;
            z-index 999
            &.del{
                color:red;
                margin-left:-3rem;
            }
            &.add{
                color:green;
                margin-left:3rem;
            }
        }
    }
}
</style>