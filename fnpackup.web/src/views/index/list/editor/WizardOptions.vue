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
                <a href="javascript:;" class="table" title="管理选项"><el-icon><Grid></Grid></el-icon></a>
            </el-form-item>
        </el-col>
    </el-row>
</template>

<script>
import {Grid} from '@element-plus/icons-vue'
export default {
    props: ['item','types'],
    components: {
        Grid
    },
    setup (props) {
        return {}
    }
}
</script>

<style lang="stylus" scoped>
a.table{
    position: absolute;
    right:1rem;
    top:0rem;
}
</style>