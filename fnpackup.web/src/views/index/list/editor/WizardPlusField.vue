<template>
    <el-form-item label-width="0" class="mgb-1">
        <el-row class="w-100">
            <el-col :span="23">
                <el-form-item label="步骤标题">
                    <el-input v-model="step.stepTitle"></el-input>
                </el-form-item>
            </el-col>
            <el-col :span="1">
                <div class="t-r">
                    <el-button link type="primary" size="small" @click="handleAdd(0)"><el-icon><Plus></Plus></el-icon></el-button>
                </div>
            </el-col>
        </el-row>
    </el-form-item>
    <template v-for="(plus,index) in step._plus_field">
        <el-form-item label-width="0" class="mgb-1">
            <el-row class="w-100">
                <el-col :span="11">
                    <el-form-item :label="`字段${index}`" label-width="80">
                        <el-input v-model="plus.field"></el-input>
                    </el-form-item>
                </el-col>
                <el-col :span="10">
                    <el-form-item label="值" label-width="30">
                        <el-input v-model="plus.value"></el-input>
                    </el-form-item>
                </el-col>
                <el-col :span="3">
                    <div class="t-r">
                        <el-button link type="danger" size="small" @click="handleDel(index)"><el-icon><Delete></Delete></el-icon></el-button>
                        <el-button link type="primary" size="small" @click="handleAdd(index+1)"><el-icon><Plus></Plus></el-icon></el-button>
                    </div>
                </el-col>
            </el-row>
        </el-form-item>
    </template>
</template>

<script>
import { Delete,Plus } from '@element-plus/icons-vue'
import { ElMessageBox } from 'element-plus';
import { useLogger } from '../../logger';
export default {
    props: ['step'],
    components:{ Delete,Plus},
    setup (props) {
        const logger = useLogger();
        const handleAdd = (index)=>{
            props.step._plus_field.splice(index,0,{'field':'field_name',value:''});
        }
        const handleDel = (index)=>{
            ElMessageBox.confirm('确定要删除吗？', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning',
            }).then(() => {
                props.step._plus_field.splice(index,1);
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