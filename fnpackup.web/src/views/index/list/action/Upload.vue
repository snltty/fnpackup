<template>
    <el-dialog v-model="state.show" title="上传文件到当前目录" width="340" :close-on-click-modal="false" :close-on-press-escape="false"  draggable>
        <div class="upload-wrap" ref="drag">
            <div class="inner"> 
                <template v-if="state.loading">
                    <div>{{state.process.current}}({{ state.process.progress }}) / {{state.process.total}}</div>
                </template>
                <template v-else>
                    <p>
                        <el-button @click="triggerSelectFile"><el-icon><Document /></el-icon>上传文件</el-button>
                    </p>
                    <p>
                        <el-button @click="triggerSelectFolder"><el-icon><Folder /></el-icon>上传文件夹</el-button>
                    </p>
                    <p>点击选择或拖拽文件/文件夹到此处</p>
                    <p>上传单个.fpk文件视为导入应用</p>
                </template>
            </div>
            <div class="drag" v-if="state.draging"></div>
        </div>
    </el-dialog>
    <input multiple type="file" ref="input" @change="onFileChange"></input>
    <input webkitdirectory directory multiple type="file" ref="input1" @change="onFileChange"></input>
</template>

<script>
import { nextTick, onMounted, reactive, ref, watch } from 'vue';
import {Plus,Refresh,Document,Folder} from '@element-plus/icons-vue'
import { useLogger } from '../../logger';
import { useProjects } from '../list';
import {fetchApi, xhrApi} from '@/api/api'
export default {
    props: ['modelValue'],
    emits: ['update:modelValue'],
    components: {Plus,Refresh},
    setup (props,{emit}) {
        
        const logger = useLogger();
        const projects = useProjects();
        const state = reactive({
            show:true,
            loading:false,
            draging:false,
            dragingTimer:0,
            process:{
                total:0,
                current:0,
                progress:'0%'
            }
        });
        watch(() => state.show, (val) => {
            if (!val) {
                setTimeout(() => {
                    emit('update:modelValue', val);
                }, 300);
            }
        });

        const input = ref(null);
        const input1 = ref(null);
        const drag = ref(null);

        const upload = (files)=>{
            state.loading = true;
            state.process.total = files.length;
            state.process.current = 0;
            const fn = async (index = 0)=>{
                if(index > files.length-1){
                    setTimeout(() => {
                        state.loading = false;
                    }, 1000);
                    projects.value.load(); 
                    return;
                }
                const fileObj =  files[index];
                const formData = new FormData();
                formData.append('files', fileObj.file);
                state.process.progress = `0%`;
                xhrApi(`/files/upload`,{path:fileObj.path},formData,(progress)=>{
                    state.process.progress = `${progress.toFixed(2)}%`;
                }).then((res)=>{
                    if(res.length > 0){
                        res.forEach(item=>{
                            logger.value.error(item);
                        });
                    }else{
                        state.process.progress = `100%`;
                        logger.value.success(`已上传:${fileObj.file.name}`);
                    }
                    state.process.current = index+1;
                    fn(index+1);
                }).catch((e)=>{
                    logger.value.error(`${e}`);
                    state.process.current = index+1;
                    fn(index+1);
                });
            }
            fn();
        }
        const onFileChange = (object) => {
            const files = Array.from(object.target.files);
            input.value.value = '';
            input1.value.value = '';
            state.loading = true;
            upload(files.map(c=>{
                return {
                    file:c,
                    path:projects.value.page.path
                };
            }));
        }
        const triggerSelectFile = () => {
            input.value.click();
        }
        const triggerSelectFolder = () => {
            input1.value.click();
        }

        const dragover = (e) => {
            if(state.loading) return;
            state.draging = true;
            clearTimeout(state.dragingTimer);
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
        }
        const dragleave = (e) => {
            if(state.loading) return;
            e.preventDefault();
            state.dragingTimer=setTimeout(()=>{
                state.draging = false;
            },300);
        }
        const dragEnd = async(e) => {
            if(state.loading) return;
            state.draging = false;
            e.preventDefault();
            e.preventDefault();
            const items = e.dataTransfer.items;
            const files = [];
            for(const item of items){
                const entry = item.webkitGetAsEntry();
                if(entry.isFile){
                    const file = await asyncFn(entry.file,entry);
                    const arr = entry.fullPath.split('/');
                    const path = arr.slice(0,arr.length-1).join('/');
                    files.push({
                        file:file,
                        path:`${projects.value.page.path}${path}`
                    });
                }else{
                    await handleEntries(files,entry);
                }
            }
            state.loading = true;
            upload(files);
        }
        const asyncFn = (fn,obj)=>{
            return new Promise((resolve,reject)=>{
                fn.call(obj,(values)=>{
                    resolve(values);
                });
            });
        }
        const handleEntries = async (files,entry)=>{
            const reader = entry.createReader();
            const entries = await asyncFn(reader.readEntries,reader); 
            for(const entry of entries){
                if(entry.isFile){
                    const file = await asyncFn(entry.file,entry);
                    const arr = entry.fullPath.split('/');
                    const path = arr.slice(0,arr.length-1).join('/');
                    files.push({
                        file:file,
                        path:`${projects.value.page.path}${path}`
                    });
                }else{
                    await handleEntries(files,entry);
                }
            }
        }

        onMounted(() => {
            nextTick(()=>{
                drag.value.addEventListener('dragover',dragover);
                drag.value.addEventListener('dragleave',dragleave);
                drag.value.addEventListener('drop', dragEnd);
            });
        });

        return {state,input,input1,drag,onFileChange,triggerSelectFile,triggerSelectFolder}
    }
}
</script>

<style lang="stylus" scoped>
 input[type=file]
    opacity: 0;
    z-index: -1;
    position: absolute;
   
.upload-wrap{
    justify-content: center;
    align-items: center;

    height:30rem;
    border:2px dashed #ccc;
    cursor: pointer;

    position: relative;

    .inner{
        position:absolute;
        top:50%;
        width:100%;
        transform: translateY(-50%);
        text-align:center;

        p{
            padding-top:1rem;
        }
    }

    .drag{
        position:absolute;
        top:0;
        left:0;
        width:100%;
        height:100%;
        background:rgba(255,255,255,0.8);
    }
}
</style>