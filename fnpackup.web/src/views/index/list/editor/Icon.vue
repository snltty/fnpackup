<template>
    <div class="icon-wrap">
        <div class="image" ref="drag" v-loading="state.loading" @click="triggerSelectFile">
            <img :src="image"></img>
            <div class="drag" v-if="state.draging"></div>
        </div>
        <p class="t-c">点击图片上传，或拖拽到图片处上传</p>
    </div>
    <input multiple type="file" ref="input" @change="onFileChange"></input>
</template>

<script>
import { computed, nextTick, onMounted, reactive, ref } from 'vue';
import { useProjects } from '../list';
import { fetchApi } from '@/api/api';
import { useLogger } from '../../logger';

export default {
    match:/(ICON|icon).*(PNG|png)$/,
    width:500,
    setup () {

        const logger = useLogger();
        const projects = useProjects();
        const image = computed(()=>{
            if(process.env.NODE_ENV==='development'){
                return `http://localhost:1069/files/img?path=${projects.value.current.path}&t=${state.version}`;
            }
            return `/files/img?path=${projects.value.current.path}&t=${state.version}`;
        });

        const state = reactive({
            loading:false,
            draging:false,
            dragingTimer:0,
            version:0

        });
        const input = ref(null);
        const drag = ref(null);
        const upload = (file)=>{
            state.loading = true;
            const formData = new FormData();
            formData.append('files', file);
            fetchApi(`/files/upload`,{
                params:{path:projects.value.current.path},
                method:'POST',
                body:formData,
            }).then(res=>res.json()).then((res)=>{
                state.loading = false;
                if(res.length > 0){
                    res.forEach(item=>{
                        logger.value.error(item);
                    });
                }else{
                    logger.value.success(`已上传:${file.name} 到 ${projects.value.current.path}`);
                    state.version++
                }
            }).catch((e)=>{
                state.loading = false;
                logger.value.error(`${e}`);
            });
        }
        const onFileChange = (object) => {
            const files = Array.from(object.target.files);
            input.value.value = '';
            upload(files[0]);
        }
        const triggerSelectFile = () => {
            input.value.click();
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
            const files = e.dataTransfer.files;
            upload(files[0]);
        }

        onMounted(() => {
            nextTick(()=>{
                drag.value.addEventListener('dragover',dragover);
                drag.value.addEventListener('dragleave',dragleave);
                drag.value.addEventListener('drop', dragEnd);
            });
        });
        
        return {projects,image,state,input,drag,onFileChange,triggerSelectFile}
    }
}
</script>

<style lang="stylus" scoped>
input[type=file]
    opacity: 0;
    z-index: -1;
    position: absolute;
.icon-wrap{
    margin:2rem auto
    .image{
        width:256px
        height:256px
        border:1px dashed #ddd;
        margin:2rem auto;
        position: relative

        display: flex
        justify-content: center
        align-items: center

        img{
            max-width:256px
            max-height:256px
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