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
            <el-form-item label="初始值">
                <el-input v-model="item[`_${item.type}`]" ></el-input>
            </el-form-item>
        </el-col>
    </el-row>
</template>

<script>
export default {
    allowTypes:['text','password'],
    props: ['item','types'],
    setup (props) {

        return {}
    }
}
</script>

<style lang="stylus" scoped>

</style>