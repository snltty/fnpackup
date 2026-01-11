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
        </div>
    </div>
    <div class="t-c">
        <el-button type="primary" @click="handleSave" :loading="state.loading">确定保存</el-button>
    </div>
    <el-dialog v-model="state.showSave" width="360" top="1vh"
    :close-on-click-modal="false" :close-on-press-escape="false" draggable>
        <el-descriptions title="选择保存项" :column="1" size="small" border class="w-100" :label-width="80">
            <el-descriptions-item label="小图缩放吗">
                <div class="flex">
                    <el-radio-group v-model="state.toSmall" size="small" @change="handleSmallChange">
                        <el-radio :value="1" size="large" class="mgr-1">不变</el-radio>
                        <el-radio :value="0.5" size="large" class="mgr-1">1/2</el-radio>
                        <el-radio :value="0.25" size="large" class="mgr-1">1/4</el-radio>
                        <el-radio :value="0.125" size="large">1/8</el-radio>
                    </el-radio-group>
                </div>
            </el-descriptions-item>
            <el-descriptions-item label="保存项">
                <div class="flex">
                    <template v-for="icon in state.icons">
                        <el-checkbox v-model="icon.use" size="small">{{icon.name}}</el-checkbox>
                    </template>
                </div>
            </el-descriptions-item>
            
            <el-descriptions-item>
                <el-button plain type="primary" :loading="state.loading" @click="handleConfirmIcon">
                    <template v-if="state.loading">
                        {{ state.process.progress }}
                    </template>
                    <template v-else>确定保存</template>
                </el-button>
            </el-descriptions-item>
        </el-descriptions>
    </el-dialog>
    <input multiple type="file" ref="input" accept="image/*" @change="handleFileChange"></input>
</template>

<script>
import { nextTick, onMounted, reactive, ref } from 'vue';
import { useLogger } from '../../logger';
import { ElMessage, ElNotification } from 'element-plus';
import { fetchFileUpload, xhrApi } from '@/api/api';
import { useProjects } from '../list';

export default {
    match:/(ICON|icon).*(PNG|png)$/,
    width:500,
    props:['path','content'],
    setup (props) {

       
        const logger = useLogger();
        const projects = useProjects();
        const root = projects.value.page.root.join('/');
        const defaultOption = props.path.replace(root,'');
        const icons = [
            {name:'应用大图标',path:`${root}/ICON_256.PNG`,use:defaultOption == '/ICON_256.PNG',size:256},
            {name:'应用小图标',path:`${root}/ICON.PNG`,use:defaultOption == '/ICON.PNG',size:256,small:true},
            {name:'入口大图标',path:`${root}/app/ui/images/icon_256.png`,use:defaultOption == '/app/ui/images/icon_256.png',size:256},
            {name:'入口小图标',path:`${root}/app/ui/images/icon_64.png`,use:defaultOption == '/app/ui/images/icon_64.png',size:256,small:true},
        ];

        const state = reactive({
            loading:false,
            draging:false,
            dragingTimer:0,
            version:Date.now(),
            fileName:props.path.split('/').pop(),
            fileImage:undefined,
            fileShowImage:undefined,
            file:null,
            changed:false,
            size:0,
            radius:0,

            toSmall:1,
            icons:icons,
            showSave:false,
            process:{
                total:0,
                current:0,
                progress:'0%'
            },

        });
        const input = ref(null);
        const drag = ref(null);
        const handleSave = () => { 
            state.showSave = true;
            state.icons.forEach(item=>{ 
                item.size = state.size;
            });
        }
        const handleConfirmIcon = async ()=>{ 
            const icons = state.icons.filter(c=>c.use);
            if(icons.length == 0){
                ElMessage.error('请选择保存项');
                return;
            }
            state.loading = true;
            
            const fn = async (index = 0)=>{
                if(index > icons.length-1){
                    state.loading = false;
                    state.showSave = false;
                    logger.value.success(`已上传${icons.length}个文件`);
                    ElNotification({
                        type: 'success',
                        title: '文件上传',
                        message: `已上传${icons.length}个文件`,
                        duration:3000,
                    });
                    return;
                }

                const fileObj =  icons[index];
                const file = await toFile(state.fileShowImage,fileObj.size,fileObj.path.split('/').pop());
                const formData = new FormData();
                formData.append('files', file);

                state.process.progress = `0%`;
                fetchFileUpload(fileObj.path,'',formData,(progress)=>{
                    state.process.progress = `${progress.toFixed(2)}%`;
                }).then((res)=>{
                    if(res.length > 0){
                        res.forEach(item=>{
                            logger.value.error(item);
                        });
                    }else{
                        state.process.progress = `100%`;
                        logger.value.success(`已上传:${fileObj.path}`);
                        nextTick(()=>{
                            ElNotification({
                                type: 'success',
                                title: '文件上传',
                                message: `已上传:${fileObj.path}`,
                                duration:3000,
                            });
                        });
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
        const handleSmallChange = ()=>{ 
            state.icons.forEach((item,index)=>{ 
                if(item.small){
                    item.size = Math.round(state.icons[index-1].size * state.toSmall);
                }
            });
        }

        const loadIcon = () => { 
            const url = process.env.NODE_ENV==='development' 
                ? `http://localhost:1069/file/img?path=${props.path}&t=${state.version}`
                : `/file/img?path=${props.path}&t=${state.version}`;

            const image = new Image();
            image.crossOrigin = 'anonymous';
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
                img.crossOrigin = 'anonymous';
                img.src = canvas.toDataURL('image/png', 1);
                img.onload = () => { 
                    resolve(img);
                };

            });
        }  
        const toImage = async (file) => { 
            const image = new Image();
            image.crossOrigin = 'anonymous';
            image.src = URL.createObjectURL(file);
            image.onload = async () => { 
                let img = image.width != image.height? await clipSize(image,state.size) : await toSize(image,state.size);
                if(state.radius > 0){
                    img = await toRadius(img,state.radius);
                }
                state.size = img.width;

                state.fileImage = image.src;
                state.fileShowImage = img.src;
                state.changed = true;
            };
        }
        const toFile = (src,size,name) => { 
            return new Promise(async (resolve,reject)=>{
                const image = new Image();
                image.crossOrigin = 'anonymous';
                image.onload = async () => {
                    const dpr = window.devicePixelRatio || 1;
                    const canvas = document.createElement('canvas');
                    const canvas1 = document.createElement('canvas');
                    
                    canvas.width = size * dpr;
                    canvas.height = size * dpr;
                    canvas.style.width = size + 'px';
                    canvas.style.height = size + 'px';

                    canvas1.width = size ;
                    canvas1.height = size;

                    const ctx = canvas.getContext('2d');
                    const ctx1 = canvas1.getContext('2d');
                    ctx.scale(dpr, dpr);
                    ctx.imageSmoothingEnabled = true;
                    ctx1.imageSmoothingEnabled = true;
                    ctx.imageSmoothingQuality = 'high';
                    ctx1.imageSmoothingQuality = 'high';
                    ctx.drawImage( image, 0,0, image.width,image.height, 0,0,size,size);
                    ctx1.drawImage( canvas, 0,0, canvas.width,canvas.height,0,0,canvas1.width,canvas1.height);
                    canvas1.toBlob(blob => {
                        resolve(new File([blob], name, {
                            type: 'image/png',
                            lastModified: Date.now()
                        }));
                    }, 'image/png');
                };
                image.src = src;
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
                img.crossOrigin = 'anonymous';
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
                img.crossOrigin = 'anonymous';
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
        
        return {state,input,drag,handleSave,handleFileChange,handleSelectFile,handleArgChange,handleConfirmIcon,handleSmallChange}
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