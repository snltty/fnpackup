<template>
    <div class="icon-wrap">
        <div class="image" :class="{file:!!state.changed}" ref="drag" v-loading="state.loading" @click="handleSelectFile">
            <img :src="state.fileShowImage"></img>
            <div class="drag" v-if="state.draging"></div>
        </div>
        <p class="t-c">点击选择或拖拽图片上传，自动转PNG，自动裁剪为正方形</p>
        <div class="flex args">
            <div>尺寸:</div>
            <div class="number">
                <el-input-number size="small" @change="handleArgChange" v-model="state.size" :min="32" :max="8192" controls-position="right" />
            </div>
            <div class="split"></div>
            <div>圆角:</div>
            <div class="flex-1 mgl-2">
                <el-slider v-model="state.radius"  @change="handleArgChange" size="small" :min="0" :max="state.size/2" />
            </div>
            <div class="split"></div>
            <div class="to-small">
                <el-tooltip content="保存时也保存到小图标，使用同一个图标" placement="top" effect="light">
                    <el-checkbox v-model="state.toSmall" size="small" label="到小图标"/>
                </el-tooltip>
            </div>
        </div>
    </div>
    <div class="t-c">
        <el-button type="primary" @click="handleSave" :loading="state.loading">确定保存</el-button>
    </div>
    <input multiple type="file" ref="input" accept="image/*" @change="handleFileChange"></input>
</template>

<script>
import { nextTick, onMounted, reactive, ref } from 'vue';
import { useProjects } from '../list';
import { fetchApi } from '@/api/api';
import { useLogger } from '../../logger';
import { ElMessage } from 'element-plus';

export default {
    match:/(ICON|icon).*(PNG|png)$/,
    width:500,
    setup () {

        const logger = useLogger();
        const projects = useProjects();
        const state = reactive({
            loading:false,
            draging:false,
            dragingTimer:0,
            version:0,
            fileName:projects.value.current.path.split('/').pop(),
            fileImage:undefined,
            fileShowImage:undefined,
            file:null,
            changed:false,
            size:0,
            radius:0,
            toSmall:false

        });
        const input = ref(null);
        const drag = ref(null);
        const upload = (file,path)=>{
            return new Promise((resolve,reject)=>{
                state.loading = true;
                const formData = new FormData();
                formData.append('files', file);
                const filename = path.split('/').pop();
                fetchApi(`/files/upload`,{
                    params:{path:path},
                    method:'POST',
                    body:formData,
                }).then(res=>res.json()).then((res)=>{
                    state.loading = false;
                    if(res.length > 0){
                        res.forEach(item=>{
                            logger.value.error(item);
                        });
                        reject();
                    }else{
                        logger.value.success(`已上传:${filename} 到 ${path}`);
                        ElMessage.success(`已上传:${filename}`);
                        state.version ++ ;
                        loadIcon();
                        resolve();
                    }
                }).catch((e)=>{
                    state.loading = false;
                    logger.value.error(`${e}`);
                    reject();
                }); 
            });
        }
        const handleSave = () => { 
            state.loading = true;
            toFile(state.fileShowImage).then((file)=>{
                upload(file,projects.value.current.path).then(()=>{
                    if(state.toSmall){
                        const arr = projects.value.current.path.split('/');
                        arr[arr.length-1] = /\.PNG$/.test(projects.value.current.path) ? 'ICON.PNG' : 'icon_64.png';
                        upload(file,arr.join('/'));
                    }
                });
                
            });
        }

        const loadIcon = () => { 
            const url = process.env.NODE_ENV==='development' 
                ? `http://localhost:1069/files/img?path=${projects.value.current.path}&t=${state.version}`
                : `/files/img?path=${projects.value.current.path}&t=${state.version}`;

            const image = new Image();
            image.src = url;
            image.onload = async () => { 
                state.size = Math.min(image.width, image.height);
                state.fileImage = image.src;
                state.fileShowImage = image.src;
                state.changed = false;
            };
        }

        const clipSize = (image,size)=>{
            return new Promise((resolve,reject)=>{ 
                const canvas = document.createElement('canvas');
                canvas.width = size;
                canvas.height = size;
                const ctx = canvas.getContext('2d');
                const sourceX = (image.width - size) / 2;
                const sourceY = (image.height - size) / 2;
                
                ctx.imageSmoothingEnabled = true;
                ctx.imageSmoothingQuality = 'high';
                ctx.drawImage(
                    image, 
                    sourceX,sourceY,
                    size,size,
                    0,0,
                    size,size
                );
                const img = new Image();
                img.src = canvas.toDataURL('image/png', 1);
                img.onload = () => { 
                    resolve(img);
                };

            });
        }  
        const toImage = async (file) => { 
            const image = new Image();
            image.src = URL.createObjectURL(file);
            image.onload = async () => { 
                const img = image.width != image.height ? await clipSize(image,Math.min(image.width, image.height)): image;
                state.size = Math.min(img.width, img.height);

                state.fileImage = img.src;
                state.fileShowImage = img.src;
                state.changed = true;
            };
        }
        const toFile = (src) => { 
            return new Promise(async (resolve,reject)=>{ 
                const response = await fetch(src);
                const blob = await response.blob();
                const file = new File([blob], state.fileName, {
                    type: 'image/png',
                    lastModified: Date.now()
                });
                resolve(file);
            });
        }

        const toSize = (image,size)=>{
            return new Promise((resolve,reject)=>{ 
                if(image.width == size && image.height == size){
                    resolve(image);
                    return;
                }
                const canvas = document.createElement('canvas');
                canvas.width = size;
                canvas.height = size;
                const ctx = canvas.getContext('2d');
                ctx.imageSmoothingEnabled = true;
                ctx.imageSmoothingQuality = 'high';
                ctx.drawImage(
                    image, 
                    0,0,
                    image.width,image.width,
                    0,0,
                    size,size
                );
                const img = new Image();
                img.src = canvas.toDataURL('image/png', 1);
                img.onload = () => { 
                    resolve(img);
                };
            });
        }

        const createRoundedRectPath = (ctx, x, y, width, height, radius)=>{
            ctx.beginPath();
            ctx.moveTo(x + radius, y);
            ctx.lineTo(x + width - radius, y);
            ctx.arcTo(x + width, y, x + width, y + radius, radius);
            ctx.lineTo(x + width, y + height - radius);
            ctx.arcTo(x + width, y + height, x + width - radius, y + height, radius);
            ctx.lineTo(x + radius, y + height);
            ctx.arcTo(x, y + height, x, y + height - radius, radius);
            ctx.lineTo(x, y + radius);
            ctx.arcTo(x, y, x + radius, y, radius);
            
            ctx.closePath();
        }
        function toRadius(image, radius = 0) {
            return new Promise((resolve, reject) => { 
                if(radius == 0){
                    resolve(image);
                    return;
                }
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                
                canvas.width = image.width;
                canvas.height = image.height;
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.drawImage(image, 0, 0);
                
                // 2. 创建反蒙版（圆角外部为白色）
                const inverseMask = document.createElement('canvas');
                const inverseCtx = inverseMask.getContext('2d');
                inverseMask.width = canvas.width;
                inverseMask.height = canvas.height;
                
                // 填充黑色背景
                inverseCtx.fillStyle = 'black';
                inverseCtx.fillRect(0, 0, inverseMask.width, inverseMask.height);
                
                // 在黑色背景上挖出白色圆角矩形
                inverseCtx.globalCompositeOperation = 'destination-out';
                inverseCtx.fillStyle = 'white';
                createRoundedRectPath(inverseCtx, 0, 0, inverseMask.width, inverseMask.height, radius);
                inverseCtx.fill();
                
                // 3. 使用source-out模式擦除非圆角部分
                ctx.globalCompositeOperation = 'destination-out';
                ctx.drawImage(inverseMask, 0, 0);
                ctx.globalCompositeOperation = 'source-over';

                const img = new Image();
                img.src = canvas.toDataURL('image/png', 1);
                img.onload = () => { 
                    resolve(img);
                };
            });
        }
        const handleArgChange = ()=>{
            const image = new Image();
            image.crossOrigin = 'anonymous';
            image.src = state.fileImage;
            image.onload = () => { 
                toSize(image,state.size).then((image)=>{ 
                    toRadius(image,state.radius).then((image)=>{ 
                        state.fileShowImage = image.src;
                        state.changed = true;
                    });
                });
            };
        }

        const handleFileChange = (object) => {
            const files = Array.from(object.target.files);
            input.value.value = '';
            toImage(files[0]);
        }
        const handleSelectFile = () => {
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
            toImage(files[0]);
        }

        onMounted(() => {
            loadIcon();
            nextTick(()=>{
                drag.value.addEventListener('dragover',dragover);
                drag.value.addEventListener('dragleave',dragleave);
                drag.value.addEventListener('drop', dragEnd);
            });
        });
        
        return {projects,state,input,drag,handleSave,handleFileChange,handleSelectFile,handleArgChange}
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

    .args{
        padding-top:1rem;
        .split{
            width:1rem;
        }
        .number{
            width:8rem;
        }
        .to-small{
            width:6rem;
        }
    }

    .image{
        width:256px
        height:256px
        border:1px dashed #409eff;
        margin:2rem auto;
        position: relative

        display: flex
        justify-content: center
        align-items: center

        &.file{
            border-color: #d96e00;
        }

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