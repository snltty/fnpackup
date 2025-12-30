<template>
    <el-row class="w-100">
        <el-col :span="12">
            <el-form-item label="字段类型">
                <el-select v-model="item.type" placeholder="请选择">
                    <el-option v-for="(item) in types" :key="item.value" :label="item.label" :value="item.value"></el-option>
                </el-select>
            </el-form-item> 
        </el-col>
        <el-col :span="12">
            <el-form-item label="显示文本">
                <el-input v-model="item.label" ></el-input>
            </el-form-item>
        </el-col>
    </el-row>
    <el-row class="w-100 mgt-1">
        <el-col :span="12">
            <el-form-item label="字段名称">
                <el-input v-model="item.field" ></el-input>
            </el-form-item>
        </el-col>
        <el-col :span="12">
            <el-form-item label="初始值" class="relative">
                <template v-if="item.type=='checkbox'">
                    <el-select v-model="item[`_${item.type}`]" multiple collapse-tags collapse-tags-tooltip :max-collapse-tags="1">
                        <el-option v-for="(option,index) in item.options" :key="index" :label="option.label" :value="option.value"></el-option>
                    </el-select>
                </template>
                <template v-else>
                    <el-select v-model="item[`_${item.type}`]">
                        <el-option v-for="(option,index) in item.options" :key="index" :label="option.label" :value="option.value"></el-option>
                    </el-select>
                </template>
                <a href="javascript:;" class="table" title="管理选项" @click="handleOptions"><el-icon><Grid></Grid></el-icon></a>
            </el-form-item>
        </el-col>
    </el-row>
    <el-dialog v-model="state.show" title="选项列表" width="400" top="2vh" >
        <div v-if="state.show">
            <div class="head t-c flex">
                <el-button plain size="small" @click="handleAdd(0)">添加一项</el-button>
                <span class="flex-1 t-c">双击某栏编辑值</span>
                <el-button plain size="small" type="primary" @click="handleSubmit">确定保存</el-button>
            </div>
            <el-table :data="state.data" stripe  border size="small" @cell-dblclick="handleCellClick">
                <el-table-column prop="label" label="标签">
                    <template #default="scope">
                        <template v-if="scope.row._label">
                            <el-input size="small" v-model="scope.row.label" @change="handleCellChange(scope.row,'label')" />
                        </template>
                        <template v-else>
                            <span>{{scope.row.label}}</span>
                        </template>
                    </template>
                </el-table-column>
                <el-table-column prop="value" label="值">
                    <template #default="scope">
                        <template v-if="scope.row._value">
                            <el-input size="small" v-model="scope.row.value" @change="handleCellChange(scope.row,'value')" />
                        </template>
                        <template v-else>
                            <span>{{scope.row.value}}</span>
                        </template>
                    </template>
                </el-table-column>
                <el-table-column fixed="right" label="操作" min-width="40">
                    <template #default="scope">
                        <el-button link type="danger" size="small" @click="handleDel(scope.$index)"><el-icon><Delete></Delete></el-icon></el-button>
                        <el-button link type="primary" size="small" @click="handleAdd(scope.$index)"><el-icon><Plus></Plus></el-icon></el-button>
                    </template>
                </el-table-column>
            </el-table>
        </div>
    </el-dialog>
</template>

<script>
import {Grid,Delete,Plus} from '@element-plus/icons-vue'
import { ElMessageBox } from 'element-plus';
import { reactive } from 'vue';
import { useLogger } from '../../logger';
export default {
    allowTypes:['radio','checkbox','select'],
    props: ['item','types'],
    components: { Grid,Delete,Plus },
    setup (props) {

        const logger = useLogger();
        const state = reactive({
            show:false,
            data:JSON.parse(JSON.stringify(props.item.options)),
        });

        const handleCellClick = (row, column, cell, event) => {
            row[`_label`] = false;
            row[`_value`] = false;
            row[`_${column.property}`] = true;
        }
        const handleCellChange = (row, property) => {
            row[`_${property}`] = false;
        }
        const handleOptions = () => {
            state.data = props.item.options
            state.show = true;
        }
        const handleAdd = (index = 0) => {
            state.data.splice(index+1,0,{label:'',value:''})
        }
        const handleDel = (index) => {
            if(!state.data[index].label && !state.data[index].value){
                state.data.splice(index,1);
                return;
            }
            ElMessageBox.confirm('确定要删除吗？', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning',
                draggable:true,
                customStyle: {
                    'vertical-align':'unset'
                },
            }).then(() => {
                state.data.splice(index,1)
            }).catch((e) => {
                logger.value.error(`${e}`)
            });
        }

        const handleSubmit = () => {
            const arr = JSON.parse(JSON.stringify(state.data.filter(c=>c.label && c.value)));
            arr.forEach(c=>{
                delete c[`_label`];
                delete c[`_value`];
            })
            props.item.options = arr;
            state.show = false;
        }

        return {state,handleCellClick,handleCellChange,handleOptions,handleAdd,handleDel,handleSubmit}
    }
}
</script>

<style lang="stylus" scoped>
a.table{
    position: absolute;
    right:-0.5rem;
    top:0rem;
    color: #409eff;
}
</style>