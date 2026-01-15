<template>
    <div class="env-wrap h-100 scrollbar">
        <el-descriptions title="manifest" :column="1" size="small" border class="w-100 mgb-2" :label-width="80">
            <template v-for="item in state.manifest" :key="item.value">
                <el-descriptions-item  :label="item.value">{{item.label}}</el-descriptions-item>
            </template>
        </el-descriptions>
        <template v-for="item in state.wizards">
            <el-descriptions :title="item.title" :column="1" size="small" border class="w-100 mgb-2" :label-width="80">
                <template v-for="item in item.items" :key="item.value">
                    <el-descriptions-item  :label="item.value">{{item.label}}</el-descriptions-item>
                </template>
            </el-descriptions>
        </template>
        <template v-for="item in state.list">
            <el-descriptions :title="item.title" :column="1" size="small" border class="w-100 mgb-2" :label-width="80">
                <template v-for="item in item.items" :key="item.value">
                    <el-descriptions-item  :label="item.value">{{item.label}}</el-descriptions-item>
                </template>
            </el-descriptions>
        </template>
    </div>
</template>

<script>
import { fetchFileRead } from '@/api/api';
import { onMounted, reactive } from 'vue';
import { useProjects } from '../list';

export default {
    match:/env$/,
    width:550,
    setup () {
        const projects = useProjects();
        const name = projects.value.page.path.split('/').filter(item=>item && item!='.')[0];
        const state = reactive({
            list:[
                {
                    "title": "基本信息",
                    "items": [
                        {
                            "value": "${TRIM_APPNAME}",
                            "label": "应用的名称（来自 manifest 中的 appname）"
                        },
                        {
                            "value": "${TRIM_APPVER}",
                            "label": "应用的版本号（来自 manifest 中的 version）"
                        },
                        {
                            "value": "${TRIM_OLD_APPVER}",
                            "label": "应用升级前的版本号（仅在升级时可用）"
                        }
                    ]
                },
                {
                    "title": "路径信息",
                    "items": [
                        {
                            "value": "${TRIM_APPDEST}",
                            "label": "应用可执行文件目录路径（target 文件夹）"
                        },
                        {
                            "value": "${TRIM_PKGETC}",
                            "label": "配置文件目录路径（etc 文件夹）"
                        },
                        {
                            "value": "${TRIM_PKGVAR}",
                            "label": "动态数据目录路径（var 文件夹）"
                        },
                        {
                            "value": "${TRIM_PKGTMP}",
                            "label": "临时文件目录路径（tmp 文件夹）"
                        },
                        {
                            "value": "${TRIM_PKGHOME}",
                            "label": "用户数据目录路径（home 文件夹）"
                        },
                        {
                            "value": "${TRIM_PKGMETA}",
                            "label": "元数据目录路径（meta 文件夹）"
                        },
                        {
                            "value": "${TRIM_APPDEST_VOL}",
                            "label": "应用安装的存储空间路径"
                        }
                    ]
                },
                {
                    "title": "网络和端口",
                    "items": [
                        {
                            "value": "${TRIM_SERVICE_PORT}",
                            "label": "应用监听的端口号（来自 manifest 中的 service_port）"
                        }
                    ]
                },
                {
                    "title": "用户和权限",
                    "items": [
                        {
                            "value": "${TRIM_USERNAME}",
                            "label": "应用专用用户名"
                        },
                        {
                            "value": "${TRIM_GROUPNAME}",
                            "label": "应用专用用户组名"
                        },
                        {
                            "value": "${TRIM_UID}",
                            "label": "应用用户 ID"
                        },
                        {
                            "value": "${TRIM_GID}",
                            "label": "应用用户组 ID"
                        },
                        {
                            "value": "${TRIM_RUN_USERNAME}",
                            "label": "当前执行脚本的用户名（可能是 root 或应用用户）"
                        },
                        {
                            "value": "${TRIM_RUN_GROUPNAME}",
                            "label": "当前执行脚本的用户组名"
                        },
                        {
                            "value": "${TRIM_RUN_UID}",
                            "label": "当前执行脚本的用户 ID"
                        },
                        {
                            "value": "${TRIM_RUN_GID}",
                            "label": "当前执行脚本的用户组 ID"
                        }
                    ]
                },
                {
                    "title": "数据共享",
                    "items": [
                        {
                            "value": "${TRIM_DATA_SHARE_PATHS}",
                            "label": "数据共享路径列表，多个路径用冒号分隔"
                        }
                    ]
                },
                {
                    "title": "临时日志",
                    "items": [
                        {
                            "value": "${TRIM_TEMP_LOGFILE}",
                            "label": "系统日志文件路径（用户可见的日志）"
                        },
                        {
                            "value": "${TRIM_TEMP_UPGRADE_FOLDER}",
                            "label": "升级过程的临时目录"
                        },
                        {
                            "value": "${TRIM_PKGINST_TEMP_DIR}",
                            "label": "安装包解压的临时目录"
                        },
                        {
                            "value": "${TRIM_TEMP_TPKFILE}",
                            "label": "fpk 包解压目录"
                        }
                    ]
                },
                {
                    "title": "CMD 脚本",
                    "items": [
                        {
                            "value": "${TRIM_APP_STATUS}",
                            "label": "当前状态(INSTALL、START、UPGRADE、UNINSTALL、STOP、CONFIG等)"
                        }
                    ]
                },
                {
                    "title": "获取授权目录列表}",
                    "items": [
                        {
                            "value": "${TRIM_DATA_ACCESSIBLE_PATHS}",
                            "label": "可访问路径列表，多个路径用冒号分隔，仅返回读写/只读的目录"
                        }
                    ]
                },
                {
                    "title": "版本信息",
                    "items": [
                        {
                            "value": "${TRIM_SYS_VERSION}",
                            "label": "完整的系统版本号"
                        },
                        {
                            "value": "${TRIM_SYS_VERSION_MAJOR}",
                            "label": "系统主版本号"
                        },
                        {
                            "value": "${TRIM_SYS_VERSION_MINOR}",
                            "label": "系统次版本号"
                        },
                        {
                            "value": "${TRIM_SYS_VERSION_BUILD}",
                            "label": "系统构建版本号"
                        }
                    ]
                },
                {
                    "title": "系统特征",
                    "items": [
                        {
                            "value": "${TRIM_SYS_ARCH}",
                            "label": "系统 CPU 架构（如 x86_64）"
                        },
                        {
                            "value": "${TRIM_KERNEL_VERSION}",
                            "label": "系统内核版本号"
                        },
                        {
                            "value": "${TRIM_SYS_MACHINE_ID}",
                            "label": "设备的唯一标识符"
                        },
                        {
                            "value": "${TRIM_SYS_LANGUAGE}",
                            "label": "系统语言设置"
                        }
                    ]
                }
            ],
            manifest:[],
            wizards:[]
        });

        const getManifest = () => {
            fetchFileRead(`./${name}/manifest`)
            .then((res)=>{
                state.manifest = res.split('\n').reduce((arr,item)=>{
                    const index = item.indexOf('=');
                    if(index>0){
                        const value = item.substring(0,index).trim();
                        const label = item.substring(index+1).trim();
                        arr.push({value:`\$\{${value}\}`,label});
                    }
                    return arr;
                },[]);
            }).catch(()=>{})
        }
        const getWizard = (filename,title) => {
            return new Promise((resolve,reject)=>{
                fetchFileRead(`./${name}/wizard/${filename}`)
                .then((res)=>{
                    resolve(JSON.parse(res).map(c=>{
                        return {title:`${title}-${c.stepTitle}`,items:c.items.map(c=>{
                            return {
                                label:c.label,
                                value:`\$\{${c.field}\}`
                            }
                        })};
                    }))
                }).catch(()=>{
                    resolve()
                }) 
            });
        }
        const getWizards = () => {
            return Promise.all([
                getWizard('install','安装向导'),
                getWizard('uninstall','卸载向导'),
                getWizard('upgrade','升级向导'),
                getWizard('config','配置向导')
            ]).then(res=>{
                state.wizards = res.filter(c=>c).reduce((arr,item)=>{
                    arr.push(...item);
                    return arr;
                },[]);
            });
        }

        onMounted(()=>{
            getManifest();
            getWizards();
        })
        

        return {state}
    }
}
</script>

<style lang="stylus" scoped>
.env-wrap{
    padding:1rem;
    border:1px solid var(--main-border-color);
    box-sizing:border-box;
    border-radius:5px;
}
</style>