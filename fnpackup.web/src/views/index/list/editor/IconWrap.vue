<template>
    <div class="icon-wrap">
        <el-tabs v-model="current" type="border-card" class="icon-tab" @tab-change="handleChange">
            <template v-for="item in options">
                <el-tab-pane :label="item.label" :name="item.value" v-loading="projects.current.loading" class="h-100">
                    <Icon v-if="current==item.value"></Icon>
                </el-tab-pane>
            </template>
        </el-tabs>
    </div>
</template>

<script>
import { onMounted,  ref } from 'vue';
import { useProjects } from '../list';
import Icon from './Icon.vue';

export default {
    match:/(ICON|icon).*(PNG|png)$/,
    width:500,
    height:560,
    components:{Icon},
    setup () {
        const projects = useProjects();

        const names = [
            /\/ui/.test(projects.value.current.path)?'icon_256.png':'ICON_256.PNG',
            /\/ui/.test(projects.value.current.path)?'icon_64.png':'ICON.PNG',
        ];
        const prefix = /\.PNG$/.test(projects.value.current.path) ? '应用':'入口';

        const paths = (/\/cmd$/.test(projects.value.current.path) 
        ?`${projects.value.current.path}/${names[0]}`
        :projects.value.current.path).split('/');

        const current = ref('');
        
        const options = [
            {label:`${prefix}大图标`,value:names[0]},
            {label:`${prefix}小图标`,value:names[1]}
        ]
        const handleChange = (type) => {
            current.value = type;
            projects.value.current.remark = options.reduce((json,item)=>{ json[item.value]=item.label; return json;  },{})[type];
            paths[paths.length-1] = type;
            projects.value.current.path = paths.join('/');
        };
        onMounted(()=>{ 
            handleChange(paths[paths.length-1]);
        });

        return {projects,current,options,handleChange}
    }
}
</script>

<style lang="stylus" scoped>
</style>