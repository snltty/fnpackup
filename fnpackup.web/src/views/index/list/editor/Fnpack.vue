<template>
    <div class="fnpack-wrap h-100">
        <div class="t-c">
            <p>
                <el-button plain @click="handleBuild(true)" :loading="projects.building">打包并下载</el-button>
            </p>
            <p class="mgt-1">
                打包后在应用根目录下找到fpk
            </p>
            <p class="mgt-1">
                <el-button plain type="primary" @click="handleBuild(false)" :loading="projects.building">仅打包</el-button>
            </p>
        </div>
    </div>
</template>

<script>
import { fetchApi } from '@/api/api';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import { ElMessage } from 'element-plus';

export default {
    match:/fnpack$/,
    width:300,
    height:200,
    setup () {
        
        const logger = useLogger();
        const projects = useProjects();
        const getExt = async (name)=>{
            const manifest = (await fetchApi(`/files/read`,{params:{path:`./${name}/manifest`}})
            .then(res=>res.text())).split('\n').reduce((json,item)=>{
                const index = item.indexOf('=');
                if(index>0){
                    const key = item.substring(0,index).trim();
                    const value = item.substring(index+1).trim();
                    json[key] = value;
                }
                return json;
            },{});
            return `-${manifest['version']}-${manifest['platform']||manifest['arch']}`;
        }
        const rename = async (name)=>{
            const ext = await getExt(name);
            const newName = `${name}${ext}`;
            await fetchApi(`/files/renamefile`,{
                params:{path:`./${name}/${name}.fpk`,path1:`./${name}/${newName}.fpk`},
                method:'POST',
                headers:{'Content-Type':'application/json'},
            });
            return newName;
        }
        const download = (projectName,name)=>{
            let href = process.env.NODE_ENV === 'development' 
            ? `http://localhost:1069/files/download?path=./${projectName}/${name}.fpk`
            : `/files/download?path=./${projectName}/${name}.fpk`;
            const a = document.createElement('a');
            a.target='_blank';
            a.href = href;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        }
        const handleBuild = async (_download)=>{
            projects.value.building = true;
            logger.value.debug('开始打包...');

            let name = projects.value.page.path.split('/').filter(item=>item && item!='.')[0];
            let projectName = name;
            fetchApi(`/files/build`,{
                params:{name:projectName},
                method:'POST',
                headers:{'Content-Type':'application/json'},
            }).then(res=>res.text()).then(async (res)=>{
                if(res.indexOf('Packing successfully')){
                    logger.value.success(res);
                    ElMessage.success('打包成功');

                    name = await rename(projectName);
                    projects.value.load(); 

                    if(_download)download(projectName,name);
                }else{
                    ElMessage.error('打包失败');
                    logger.value.error(res);
                }
            }).catch((e)=>{
                logger.value.error(`${e}`);
                ElMessage.error('打包失败');
            }).finally(()=>{
                projects.value.building = false;
                projects.value.current.show = false;
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