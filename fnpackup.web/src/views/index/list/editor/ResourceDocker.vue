<template>
    <el-card class="mgb-1" shadow="never">
        <template #header>
            <div class="card-header flex">
                <span>Docker</span>
                <span class="flex-1"></span>
                <el-button size="small" @click="handleAdd(0)"><el-icon><Plus></Plus></el-icon></el-button>
            </div>
        </template>
        <div class="card-body">
            <template v-for="(item,index) in data">
                <el-form-item label="" label-width="0" class="mgb-0">
                    <el-row class="w-100">
                        <el-col :span="9">
                            <el-form-item label="名称" label-width="40">
                                <el-input v-model="item.name" size="small"></el-input>
                            </el-form-item>
                        </el-col>
                        <el-col :span="9">
                            <el-form-item label="路径" label-width="40">
                                <el-input v-model="item.path" size="small"></el-input>
                            </el-form-item>
                        </el-col>
                        <el-col :span="6">
                            <div class="t-r btns h-100">
                                <el-button type="danger" size="small" @click="handleDel(index)"><el-icon><Delete></Delete></el-icon></el-button>
                                <el-button type="primary" size="small" @click="handleAdd(index+1)"><el-icon><Plus></Plus></el-icon></el-button>
                            </div>
                        </el-col>
                    </el-row>
                </el-form-item>
            </template>
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

        const handleDel = (index) => {
            props.data.splice(index,1)
        }
        const handleAdd = (index) => {
            props.data.splice(index,0,{name:'',path:''})
        }

        return {
            state,handleDel,handleAdd
        }
    }
}
</script>

<style lang="stylus" scoped>
.btns
{
    display flex
    justify-content center
    align-items center
}
</style>