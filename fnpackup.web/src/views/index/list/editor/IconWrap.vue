<template>
    <div class="icon-wrap">
        <el-tabs v-model="state.type" type="border-card" class="icon-tab">
            <template v-for="item in state.types">
                <el-tab-pane :label="item.label" :name="item.value" class="h-100">
                    <Icon :path="item.path"></Icon>
                </el-tab-pane>
            </template>
        </el-tabs>
    </div>
</template>

<script>
import { reactive } from 'vue';
import { useProjects } from '../list';
import Icon from './Icon.vue';

export default {
    match:/(ICON|icon).*(PNG|png)$/,
    width:500,
    height:'auto',
    components:{Icon},
    props:['path','content'],
    setup (props) {
        const projects = useProjects();

        const names = [
            /\/ui/.test(props.path)?'icon_256.png':'ICON_256.PNG',
            /\/ui/.test(props.path)?'icon_64.png':'ICON.PNG',
        ];
        const prefix = /\.PNG$/.test(props.path) ? '应用':'入口';
        const paths =  props.path.split('/');

        const state = reactive({
            type:paths[paths.length-1],
            types: [
                {label:`${prefix}大图标`,value:names[0],path:`${paths.filter((c,i)=>i<paths.length-1).join('/')}/${names[0]}`},
                {label:`${prefix}小图标`,value:names[1],path:`${paths.filter((c,i)=>i<paths.length-1).join('/')}/${names[1]}`}
            ]
        });
        return {projects,state}
    }
}
</script>

<style lang="stylus" scoped>
</style>