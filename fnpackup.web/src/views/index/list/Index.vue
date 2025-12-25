<template>
    <div class="h-100 flex flex-column flex-nowrap">
        <Actions></Actions>
        <Path></Path>
        <Table></Table>
    </div>
    <Editor></Editor>
    <Guide v-if="projects.current.guide"></Guide>
</template>

<script>
import Path from './Path.vue';
import Actions from './action/Index.vue';
import Table from './Table.vue';
import { provideProjects } from './list';
import Editor from './editor/Index.vue';
import Guide from './editor/Guide.vue';
export default {
    components: { Path, Actions,Table,Editor,Guide },
    setup () {
        
        const {projects} = provideProjects();

        return {projects}
    }
}
</script>

<style lang="stylus" scoped>

</style>