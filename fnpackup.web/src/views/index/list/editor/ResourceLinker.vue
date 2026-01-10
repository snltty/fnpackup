<template>
    <el-card class="mgb-1" shadow="never">
        <template #header>
            <div class="card-header flex">
                <span>系统集成</span>
            </div>
        </template>
        <div class="card-body">
            <el-form-item label="bin" label-width="30" class="mgb-1">
                <el-select v-model="data.bin" placeholder="输入资源" 
                filterable clearable allow-create  multiple collapse-tags collapse-tags-tooltip :max-collapse-tags="3">
                </el-select>
            </el-form-item>
            <el-form-item label="lib" label-width="30" class="mgb-1">
                <el-select v-model="data.lib" placeholder="输入资源" 
                filterable clearable allow-create  multiple collapse-tags collapse-tags-tooltip :max-collapse-tags="3">
                </el-select>
            </el-form-item>
            <el-form-item label="etc" label-width="30" class="mgb-1">
                <el-select v-model="data.etc" placeholder="输入资源" 
                filterable clearable allow-create  multiple collapse-tags collapse-tags-tooltip :max-collapse-tags="3">
                </el-select>
            </el-form-item>
        </div>
    </el-card>
</template>

<script>
import { Delete, Plus } from '@element-plus/icons-vue';
import { reactive } from 'vue';
export default {
    props:['data'],
    components:{Delete,Plus},
    setup (props) {
        const state = reactive({
        });

        return {
            state
        }
    }
}
</script>

<style lang="stylus" scoped>
</style>