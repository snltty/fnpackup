import { inject, provide, ref, watch } from "vue";
import { useLogger } from "../logger";
import { fetchApi } from "@/api/api";


const remarks = {
    'app':'应用目录，放你的程序和程序UI/入口',
    'app/docker':'Docker应用独有目录',
    'app/server':'原生应用独有目录',
    'app/www':'原生应用独有目录',
    'app/ui':'UI入口配置',
    'app/ui/images':'UI入口配置图片',
    'app/ui/images/icon_256.png':'UI入口大图标，一般256x256px',
    'app/ui/images/icon_64.png':'UI入口大图标，一般64x64px',
    'app/ui/config':'UI入口配置文件',
    'cmd':'应用配置/安装/升级/卸载脚本',
    'cmd/config_callback':'应用配置后执行脚本',
    'cmd/config_init':'应用配置前执行脚本',
    'cmd/install_callback':'应用安装后执行脚本',
    'cmd/install_init':'应用安装前脚本',
    'cmd/main':'应用启动/停止/状态检查脚本',
    'cmd/uninstall_callback':'应用卸载后执行脚本',
    'cmd/uninstall_init':'应用卸载前脚本',
    'cmd/upgrade_callback':'应用升级后执行脚本',
    'cmd/upgrade_init':'应用升级前脚本',
    'config':'应用权限/资源',
    'config/privilege':'应用权限清单配置',
    'config/resource':'应用资源清单配置',
    
    'wizard':'应用安装/卸载向导',
    'wizard/install':'应用安装时UI',
    'wizard/uninstall':'应用卸载时UI',
    'wizard/upgrade':'应用升级时UI',
    'wizard/config':'应用配置时UI',
    'ICON_256.PNG':'应用大图标，一般256x256px',
    'ICON.PNG':'应用小图标，一般64x64px',
    'LICENSE':'用户安装前需要同意的隐私协议（可选）',
    'manifest':'应用信息清单',

    'building':'应用构建',
    'building/building':'应用构建配置',

}
const documents = [
    {match:/^manifest$/,url:'https://developer.fnnas.com/docs/core-concepts/manifest'},
    {match:/app\/docker/,url:'https://developer.fnnas.com/docs/core-concepts/docker'},
    {match:/app\/ui/,url:'https://developer.fnnas.com/docs/core-concepts/app-entry'},
    {match:/cmd/,url:'https://developer.fnnas.com/docs/core-concepts/native#%E7%BC%96%E8%BE%91%E5%BA%94%E7%94%A8%E5%90%AF%E5%81%9C%E8%84%9A%E6%9C%AC'},
    {match:/config\/privilege/,url:'https://developer.fnnas.com/docs/core-concepts/privilege'},
    {match:/config\/resource/,url:'https://developer.fnnas.com/docs/core-concepts/resource'},
    {match:/config/,url:'https://developer.fnnas.com/docs/core-concepts/resource'},
    {match:/wizard/,url:'https://developer.fnnas.com/docs/core-concepts/wizard'},
    {match:/(ICON|icon).*(PNG|png)$/,url:'https://developer.fnnas.com/docs/core-concepts/icon'},
]

const projectsSymbol = Symbol();
export const provideProjects = () => {

    const logger = useLogger();
    const projects = ref({
        page:{
            path:localStorage.getItem('projects_path') || './',
            p:1,
            ps:20,
            count:0,
            list:[]
        },
        current:{
            path:'',
            content:'',
            remark:'',
            load:true,
            show:false,
            loading:false
        },

        showCreate:false,
        showUpload:false,

        contextMenu:{
            show:false,
            x:0,
            y:0,
            row:null,
            cell:null
        },

        building:false,
        loading:false,
        load(){
            this.loading = true;
            localStorage.setItem('projects_path', this.page.path);
            fetchApi(`/files/get`,{
                params:{
                    path:this.page.path,
                    p:this.page.p,
                    ps:this.page.ps
                },
                method:'GET',
                headers:{'Content-Type':'application/json'}
            })
            .then(res=>res.json())
            .then(json=>{
                this.loading = false;
                this.page.p = json.p;
                this.page.ps = json.ps;
                this.page.count = json.count;
                json.list.forEach(c=>{
                    const paths = `${this.page.path}/${c.name}`.split('/').filter(c=>c);
                    c.remark = paths.filter((item,index)=>index>1).join('/');
                    c.doc = (documents.filter(d=>d.match.test(c.remark))[0] || {url:''}).url;
                    c.remark = remarks[c.remark] || c.remark;
                    
                })
                this.page.list = json.list;
                logger.value.success(`[${this.page.path}]文件列表加载成功，${this.page.list.length}/${this.page.count}`);
            }).catch((e)=>{
                logger.value.error(`文件列表加载失败：${e}`);
                this.loading = false;
            });
        }
    });
    watch(()=>projects.value.page.path, ()=>{
        projects.value.load();
    });

    provide(projectsSymbol, projects);
    return {
        projects
    }
}
export const useProjects = () => {
    return inject(projectsSymbol);
}