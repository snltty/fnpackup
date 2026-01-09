<template>
    <div class="h-100 list-wrap">
        <div class="h-100 inner flex flex-column flex-nowrap">
            <Actions></Actions>
            <Path></Path>
            <Table></Table>
        </div>
    </div>
    <Editor v-if="projects.editor.show" v-model="projects.editor.show"></Editor>
    <Guide v-if="projects.editor.guide" v-model="projects.editor.guide"></Guide>
    <SourceWrap v-if="projects.editor.source" v-model="projects.editor.source"></SourceWrap>
    <Paint v-if="projects.editor.paint" v-model="projects.editor.paint"></Paint>
</template>

<script>
import Path from './Path.vue';
import Actions from './action/Index.vue';
import Table from './Table.vue';
import { provideProjects } from './list';
import Editor from './editor/Index.vue';
import Guide from './editor/Guide.vue';
import SourceWrap from './editor/SourceWrap.vue';
import Paint from './editor/Paint.vue';
export default {
    components: { Path, Actions,Table,Editor,Guide,SourceWrap,Paint },
    setup () {
        
        const {projects} = provideProjects();

        return {projects}
    }
}
</script>

<style lang="stylus" scoped>
html.dark .list-wrap{
    .inner{
        border-color:#39434c;
    }
}
.list-wrap{
    padding:1rem 1rem .5rem 1rem;
    box-sizing: border-box;

    .inner{
        border:1px solid #e2e8f0e6;
        border-radius: 5px;
    }
}
</style>