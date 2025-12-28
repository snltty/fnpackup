<template>
    <div class="fnpack-wrap h-100">
        <div class="t-c">
            <p>
                <el-button @click="handleBuild(true)" :loading="projects.building">打包并下载</el-button>
            </p>
            <p class="mgt-1">
                打包后在项目根目录下找到fpk
            </p>
            <p class="mgt-1">
                <el-button type="primary" @click="handleBuild(false)" :loading="projects.building">仅打包</el-button>
            </p>
        </div>
    </div>
</template>

<script>
import { fetchApi } from '@/api/api';
import { useLogger } from '../../logger';
import { useProjects } from '../list';

export default {
    match:/fnpack$/,
    width:300,
    height:200,
    setup () {
        
        const logger = useLogger();
        const projects = useProjects();
        const handleBuild = (download)=>{
            projects.value.building = true;
            logger.value.debug('开始打包...');

            const name = projects.value.page.path.split('/').filter(item=>item && item!='.')[0];
            fetchApi(`/files/build`,{
                params:{name:name},
                method:'POST',
                headers:{'Content-Type':'application/json'},
            }).then(res=>res.text()).then((res)=>{
                if(res.indexOf('Packing successfully')){
                    logger.value.success(res);
                    projects.value.load(); 

                    if(download){
                        let href = process.env.NODE_ENV === 'development' 
                        ? `http://localhost:1069/files/download?path=./${name}/${name}.fpk`
                        : `/files/download?path=./${name}/${name}.fpk`;
                        const a = document.createElement('a');
                        a.target='_blank';
                        a.href = href;
                        document.body.appendChild(a);
                        a.click();
                        document.body.removeChild(a);
                    }
                }else{
                    logger.value.error(res);
                }
            }).catch((e)=>{
                logger.value.error(`${e}`);
            }).finally(()=>{
                projects.value.building = false;
            });
        }

        return {projects,handleBuild}
    }
}
</script>

<style lang="stylus" scoped>
.fnpack-wrap{
    display: flex;
    align-items: center;
    justify-content: center;
}
</style>