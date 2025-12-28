<template>
    <el-dialog v-model="projects.current.paint" title="图标编辑"  width="336" top="1vh"
    :close-on-click-modal="false" :close-on-press-escape="false" draggable>
    <div class="paint-wrap flex flex-nowrap">
        <div class="menu">
            <template v-for="menu in state.menu.list">
                <a href="javascript:;" :class="{current:menu.type==state.menu.type}"><img :src="menu.img"></a>
            </template>
        </div>
        <span class="flex-1"></span>
        <div class="svg-wrap" ref="wrap" @click="handleWrapClick">
            <svg width="256" height="256" viewBox="0 0 256 256" xmlns="http://www.w3.org/2000/svg">
                <g>
                    <rect :x="state.svg.stroke.size/2" :y="state.svg.stroke.size/2" 
                    :stroke-width="state.svg.stroke.size" :stroke="state.svg.stroke.color" :fill="state.svg.fill.color"
                    :width="256-state.svg.stroke.size" :height="256-state.svg.stroke.size" 
                    :rx="state.svg.fill.rx" ></rect>
                </g>
                <template v-for="(text,index) in state.svg.texts">
                    <g>
                        <text :x="text.x" :y="text.y" :id="`text-${index}`" :class="{move:text.path.show}"
                        @mousedown.stop="handleDown($event,text)" @click.stop="handleClick(text)"
                        :font-family="text.font" 
                        :font-size="text.size" 
                        :fill="text.fill"
                        :font-weight="text.weight"
                        :stroke-width="text.stroke.size"
                        :stroke="text.stroke.color">{{text.text}}</text>
                        <template v-if="text.path.show">
                            <rect :x="text.path.x" :y="text.path.y" :width="text.path.width" :height="text.path.height" 
                            stroke="blue" stroke-width="2" fill="none"></rect>
                        </template>
                    </g>
                </template>
            </svg>
        </div>
    </div>
    </el-dialog>
</template>

<script>
import { nextTick, onMounted, onUnmounted, reactive, ref } from 'vue';
import { useProjects } from '../list';
export default {
    setup () {
        const projects = useProjects();

        const state = reactive({
            timer:0,
            drag:{
                element:null,
                dom:null,
                boxLeft: 0,
                boxTop:  0,
                boxWidth: 0,
                boxHeight: 0,
                
                startX: 0,
                startY: 0,
            },
            menu:{
                type:'bg',
                list:[
                    {type:'bg',img:'paint-bg.svg'},
                    {type:'text',img:'paint-text.svg'},
                    {type:'image',img:'paint-image.svg'}
                ]
            },

            svg:{
                stroke:{
                    size: 2,
                    color: '#4077e1'
                },
                fill:{
                    color: '#2568ed',
                    rx:68
                },
                texts:[
                    {text:'Styled Text',x:20,y:60,font:'Arial',size:20,fill:'#fff',weight:'bold',
                        stroke:{
                            size: 0,
                            color: '#000'
                        },
                        path:{
                            show:false,
                            x:0,y:0,width:0,height:0,
                        }
                    }
                ]

            }  
        });

        const wrap = ref(null);
        const handleWrapClick = (e)=>{ 
            state.svg.texts.forEach((text,index)=>{ 
                text.path.show = false;
                text.path.width = 0;
                text.path.height = 0;
            });
        }
        const handleClick = (element)=>{ 
            element.path.show = true;
            nextTick(()=>{
                strokeText();
            });
        }
        const handleDown = (event,element)=>{
            clearTimeout(state.timer);
            
            state.timer = setTimeout(()=>{
                element.path.show = true;
                nextTick(()=>{
                    strokeText();
                });
                state.drag.dom = event.target;
                state.drag.element = element;
                state.drag.boxLeft = element.x;
                state.drag.boxTop = element.y;
                state.drag.boxWidth = event.target.getBBox().width;
                state.drag.boxHeight = event.target.getBBox().height;
                state.drag.startX = event.clientX;
                state.drag.startY = event.clientY;
            },100);
            
        }
        const handleMove = (e)=>{ 
            if(state.drag.element){
                const moveX = e.clientX - state.drag.startX;
                const moveY = e.clientY - state.drag.startY;
                let newLeft = state.drag.boxLeft + moveX;
                let newTop = state.drag.boxTop + moveY;
                const parentWidth = wrap.value.offsetWidth;
                const parentHeight = wrap.value.offsetHeight;
                newLeft = Math.max(0, newLeft);
                newLeft = Math.min(parentWidth -  state.drag.boxWidth, newLeft);
                newTop = Math.max(0, newTop);
                newTop = Math.min(parentHeight, newTop);

                state.drag.element.x = newLeft;
                state.drag.element.y = newTop;

                state.drag.element.path.width = state.drag.dom.getBBox().width;
                state.drag.element.path.height = state.drag.dom.getBBox().height;
                state.drag.element.path.x = state.drag.dom.getBBox().x;
                state.drag.element.path.y = state.drag.dom.getBBox().y;
            }
        }
        const handleUp = ()=>{
            clearTimeout(state.timer);
            state.drag.element = null;
        }

        const strokeText = ()=>{ 
            state.svg.texts.forEach((text,index)=>{ 
                if(text.path.show && text.path.width==0){
                    const textEl = document.querySelector(`#text-${index}`);
                    text.path.width = textEl.getBBox().width;
                    text.path.height = textEl.getBBox().height;
                    text.path.x = textEl.getBBox().x;
                    text.path.y = textEl.getBBox().y;
                }
            });
        }

        onMounted(()=>{
            nextTick(()=>{
                strokeText();
            });
            document.addEventListener('mouseup',handleUp);
            document.addEventListener('mousemove',handleMove);
        });
        onUnmounted(()=>{
            document.removeEventListener('mouseup',handleUp);
            document.removeEventListener('mousemove',handleMove);
        });

        return {projects,state,wrap,handleWrapClick,handleClick,handleDown}
    }
}
</script>

<style>

</style>
<style lang="stylus">
.editor-dialog{
    max-width: 80%;
    max-height: 90%;
    .el-dialog__body{
        height:100%;
    }
}
</style>
<style lang="stylus" scoped>

text{
    cursor: default;
    &.move{ 
        cursor move
    }
    
}

.paint-wrap{
    border:1px solid #ddd;
    border-radius: 0.5rem;
    padding:1rem;

    .svg-wrap{
        font-size:0;
    }
}
.menu{
    display:flex
    flex-direction: column;
    justify-content: center;
    align-items: left;

    a{
        display: block;
        border:1px solid #eee;
        width:2rem;
        height:2rem;
        margin-bottom:1rem;
        border-radius: 0.5rem;
        &.current,&:hover{
            border-color:#008000;
            img{
                opacity:1;
            }
        }


        img{
            width:2rem;
            height:2rem;
            object-fit: cover;
            opacity 0.5;
        }
    }
    
}
</style>