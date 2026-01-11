<template>
    <div class="fnpack-wrap h-100">
        <el-descriptions :column="1" size="small" border class="w-100" :label-width="80">
            <el-descriptions-item label="发布模式">
                <el-switch v-model="state.platform" active-text="多平台" inactive-text="单平台"/>
            </el-descriptions-item>
            <template v-if="state.platform">
                <el-descriptions-item label="已找到">
                    <template v-if="state.platforms.length > 0">
                        <el-checkbox-group v-model="state.platforms"> 
                            <template v-for="item in state.platformNames">
                                <el-checkbox :label="item" :value="item" />
                            </template>
                        </el-checkbox-group>
                    </template>
                    <template v-else>
                        <span class="red">未找到，请按说明新建文件夹和放入程序</span>
                    </template>
                </el-descriptions-item>
                <el-descriptions-item label="目标路径">
                    <el-input v-model="state.server"></el-input>
                </el-descriptions-item>
                <el-descriptions-item label="说明">
                    <div>
                        <p>
                            新建对应平台的文件夹，如 building/platform/arm、building/platform/x86，打包时会先清空[{{state.server}}]，再复制对应目录下的文件到[{{state.server}}]，如果文件夹为空则不清空不复制，仅打包
                        </p>
                    </div>
                </el-descriptions-item>
            </template>
            <template v-else>
                <el-descriptions-item label="说明">
                    <div>
                        仅打包manifest中platform指定的平台
                    </div>
                </el-descriptions-item>
            </template>
            <el-descriptions-item label="下载">
                <el-checkbox label="打包后下载" v-model="state.download" />
            </el-descriptions-item>
            <el-descriptions-item>
                <el-button class="mgl-1" plain type="primary" @click="handleBuild" :loading="state.loading">开始打包</el-button>
            </el-descriptions-item>
        </el-descriptions>
    </div>
</template>

<script>
import { fetchFileList, fetchFileRead, fetchFileWrite, fetchProjectBuild } from '@/api/api';
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import { ElMessage, ElNotification } from 'element-plus';
import { onMounted, reactive } from 'vue';

export default {
    match:/fnpack$/,
    width:500,
    height:'auto',
    setup () {
        
        const logger = useLogger();
        const projects = useProjects();
        const name = projects.value.page.path.split('/').filter(item=>item && item!='.')[0];

        const state = reactive({
            loading:false,
            platform:false,
            platformNames:[],
            platforms:[],
            download:false,
            server:'app/server/'
        });

        const download = (projectName,name)=>{
            let href = process.env.NODE_ENV === 'development' 
            ? `http://localhost:1069/file/download?path=./${projectName}/${name}`
            : `/file/download?path=./${projectName}/${name}`;
            const a = document.createElement('a');
            a.target='_blank';
            a.href = href;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        }
        const loadSettings = ()=>{
            return new Promise((resolve,reject)=>{ 
                fetchFileRead(`./${name}/building/settings.json`)
                .then(res=>{
                    res = JSON.parse(res);
                    if(res){
                        state.platform = res.platform || false;
                        state.server = res.server || 'app/server/';
                        state.download = res.download  || false;
                        state.platforms = res.platforms || [];
                    }
                    resolve();
                }).catch(()=>{
                    resolve();
                })
            });
            
        }
        const saveSettings = ()=>{
            fetchFileWrite(`./${name}/building/settings.json`,JSON.stringify({
                platform:state.platform || false,
                server:state.server || 'app/server/',
                download:state.download || false,
                platforms:state.platforms || []
            },null,2)).then(()=>{}).catch(()=>{});
        }

        const handleBuild = async ()=>{
            saveSettings();
            if(state.platform && state.platforms.length == 0){
                ElMessage.error('请选择平台');
                logger.value.error('请选择平台');
                return false;
            }
            state.loading = true;
            logger.value.debug('开始打包...');
            fetchProjectBuild(name,state.platform ? state.platforms.join(','):'',state.server)
            .then(async (res)=>{
                res.forEach(c=>{
                    if(c.msg.indexOf('Packing successfully')>=0){
                        logger.value.success(c.msg);
                        ElNotification({
                            type: 'success',
                            title: '打包',
                            message: `[${c.fileName}]打包成功`,
                            duration:3000,
                        });
                        if(state.download){
                            download(name,c.fileName);
                        }
                        
                    }else{
                        ElNotification({
                            type: 'error',
                            title: '打包',
                            message: `[${c.fileName}]打包失败`,
                            duration:3000,
                        });
                        logger.value.error(c.msg);
                    }
                });
                projects.value.load(); 
                
            }).catch((e)=>{
                logger.value.error(`${e}`);
                ElMessage.error('打包失败');
            }).finally(()=>{
                state.loading = false;
            });
        }

        const loadPlatforms = ()=>{
            fetchFileList(`./${name}/building/platform/`,1,100)
            .then((res)=>{
                state.platformNames = res.list.filter(c=>c.if==false).map(c=>c.name);
                if(state.platforms.length == 0){
                    state.platforms = res.list.filter(c=>c.if==false).map(c=>c.name);
                }else{
                    state.platforms = state.platforms.filter(c=>state.platformNames.includes(c));
                }
                state.platform = state.platforms.length>0 && state.platform;
            }).catch(e=>{
                logger.value.error(`${e}`);
                ElMessage.error('获取平台失败');
            });
        }
        onMounted(()=>{
            loadSettings().then(()=>{
                loadPlatforms();
            });
        })

        return {projects,state,handleBuild}
    }
}
</script>

<style lang="stylus" scoped>
.fnpack-wrap{
    display: flex;
    justify-content: center;
}
</style>