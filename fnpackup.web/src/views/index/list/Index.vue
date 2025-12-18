<template>
    <div class="h-100 flex flex-column flex-nowrap">
        <Actions></Actions>
        <Path></Path>
        <Table></Table>
    </div>
    <Editor></Editor>
</template>

<script>
import Path from './Path.vue';
import Actions from './Actions.vue';
import Table from './Table.vue';
import { provideProjects } from './list';
import Editor from './editor/Index.vue';
export default {
    components: { Path, Actions,Table,Editor },
    setup () {
        
        const {projects} = provideProjects();

        return {}
    }
}
</script>

<style lang="stylus" scoped>

</style>