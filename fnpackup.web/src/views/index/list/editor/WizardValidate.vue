<template>
    <template v-for="(rule,index) in item.rules" v-if="item.rules.length > 0">
        <el-row class="w-100">
            <el-col :span="7">
                <el-form-item label="验证类型">
                    <el-select v-model="rule._type" placeholder="请选择" size="small">
                        <el-option v-for="(vt) in vtypes" :key="vt.value" :label="vt.label" :value="vt.value"></el-option>
                    </el-select>
                </el-form-item> 
            </el-col>
            <el-col :span="14">
                <template v-if="rule._type == 'required'">
                    <el-row class="mgl-1">
                        <el-col :span="6"><el-checkbox v-model="rule._required">必填</el-checkbox></el-col>
                        <el-col :span="18"><el-input v-model="rule._required_message" size="small"/></el-col>
                    </el-row>
                    
                </template>
                <template v-else-if="rule._type == 'min'">
                    <el-row class="mgl-1">
                        <el-col :span="6"><el-input-number v-model="rule._min" size="small" controls-position="right"/></el-col>
                        <el-col :span="1" class="t-c">-</el-col>
                        <el-col :span="6"><el-input-number v-model="rule._max" size="small" controls-position="right"/></el-col>
                        <el-col :span="1" class="t-c"></el-col>
                        <el-col :span="10"><el-input v-model="rule._min_message" size="small"/></el-col>
                    </el-row>
                </template>
                <template v-else-if="rule._type == 'len'">
                    <el-row class="mgl-1">
                        <el-col :span="6"><el-input-number v-model="rule._len" size="small" controls-position="right"/></el-col>
                        <el-col :span="1" class="t-c"></el-col>
                        <el-col :span="17"><el-input v-model="rule._pattern_message" size="small"/></el-col>
                    </el-row>
                </template>
                <template v-else-if="rule._type == 'pattern'">
                    <el-row class="mgl-1">
                        <el-col :span="12"><el-input v-model="rule._pattern" size="small"/></el-col>
                        <el-col :span="1" class="t-c"></el-col>
                        <el-col :span="11"><el-input v-model="rule._pattern_message" size="small"/></el-col>
                    </el-row>
                    
                </template>
            </el-col>
            <el-col :span="3">
                <div class="t-r">
                    <el-button link type="danger" size="small" @click="handleDel(index)"><el-icon><Delete></Delete></el-icon></el-button>
                    <el-button link type="primary" size="small" @click="handleAdd(index)"><el-icon><Plus></Plus></el-icon></el-button>
                </div>
            </el-col>
        </el-row>
    </template>
    <template v-else>
        <el-row class="w-100">
            <el-col :span="4">
                <div class="t-r">
                    <el-button link type="danger" size="small" @click="handleDel(0)"><el-icon><Delete></Delete></el-icon></el-button>
                    <el-button link type="primary" size="small" @click="handleAdd(0)"><el-icon><Plus></Plus></el-icon></el-button>
                </div>
            </el-col>
        </el-row>
    </template>
</template>

<script>
import { Delete,Plus } from '@element-plus/icons-vue'
import { ElMessageBox } from 'element-plus';
import { useLogger } from '../../logger';
export default {
    props: ['item','types','vtypes'],
    components:{ Delete,Plus},
    setup (props) {

        const logger = useLogger();
        const buildValidateField = (item)=>{
            if(!item['_type']){
                item['_type'] = props.vtypes.filter(c=>item[c.value] !== undefined)[0].value;
            }
            Object.assign(item,props.vtypes.reduce((json,value)=>{
                Object.assign(json,value.default,{
                    [`_${value.value}_message`]: item[value.value] !== undefined ?  item.message : ''
                });
                return json;
            },{}));
            return item;
        }
        
        props.item.rules.forEach(c=>{
            buildValidateField(c);
        });

        const handleAdd = (index)=>{
            props.item.rules.splice(index+1,0,buildValidateField({'required':true,message:'请输入内容'}));
        }
        const handleDel = (index)=>{
            ElMessageBox.confirm('确定要删除吗？', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning',
            }).then(() => {
                props.item.rules.splice(index,1);
            }).catch((e) => {
                logger.value.error(`${e}`)
            });
        }
        
        return {handleDel,handleAdd}
    }
}
</script>

<style lang="stylus" scoped>

</style>