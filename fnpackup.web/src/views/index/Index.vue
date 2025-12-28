<template>
    <el-splitter layout="vertical">
        <el-splitter-panel>
            <List></List>
        </el-splitter-panel>
        <el-splitter-panel min="10%" size="20%" max="80%">
            <Logger></Logger>
        </el-splitter-panel>
    </el-splitter>
</template>

<script>
import Logger from './Logger.vue';
import List from './list/Index.vue'
import { provideLogger } from './logger';
export default {
    components: {
        List,
        Logger
    },
    setup () {
        
        const {logger} = provideLogger();

        return {}
    }
}
</script>

<style lang="stylus" scoped>
</style>