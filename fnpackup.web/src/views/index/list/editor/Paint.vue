<template>
    <el-dialog v-model="projects.current.paint" title="图标编辑"  width="310" top="1vh"
    :close-on-click-modal="false" :close-on-press-escape="false" draggable>
    <div class="paint-wrap flex flex-nowrap">
        <span class="flex-1"></span>
        <div class="svg-wrap" ref="wrap" @click="handleWrapClick">
            <svg width="256" height="256" viewBox="0 0 256 256" xmlns="http://www.w3.org/2000/svg">
                <g>
                    <rect :x="state.svg.stroke.size/2" :y="state.svg.stroke.size/2" 
                    :stroke-width="state.svg.stroke.size" :stroke="state.svg.stroke.color" :fill="state.svg.fill.color"
                    :width="256-state.svg.stroke.size" :height="256-state.svg.stroke.size" 
                    :rx="state.svg.fill.rx" @click.stop="handleClick(state.svg)" id="svg-bg"></rect>
                    <template v-if="state.svg.path.show">
                        <rect :x="state.svg.path.x" :y="state.svg.path.y" :width="state.svg.path.width" :height="state.svg.path.height" 
                        :stroke="state.svg.path.color" stroke-width="2" stroke-dasharray="5,3" fill="none"></rect>
                    </template>
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
                            :stroke="text.path.color" stroke-width="2" stroke-dasharray="5,3" fill="none"></rect>
                            <rect @click.stop :fill="text.path.color" :x="text.path.x-4" :y="text.path.y+text.path.height/2-4" width="8" height="8" class="resize-l"></rect>
                            <rect @click.stop :fill="text.path.color" :x="text.path.x+text.path.width-4" :y="text.path.y+text.path.height/2-4" width="8" height="8" class="resize-r"></rect>
                            <rect @click.stop :fill="text.path.color" :x="text.path.x+text.path.width/2-4" :y="text.path.y-4" width="8" height="8" class="resize-t"></rect>
                            <rect @click.stop :fill="text.path.color" :x="text.path.x+text.path.width/2-4" :y="text.path.y+text.path.height-4" width="8" height="8" class="resize-b"></rect>
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

            svg:{
                stroke:{
                    size: 2,
                    color: '#4077e1'
                },
                fill:{
                    color: '#2568ed',
                    rx:68
                },
                path:{
                    show:false,
                    x:0,y:0,width:0,height:0,
                    color:'blue',
                    chooiceColor:'blue',
                    dragColor:'yellow',
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
                            color:'blue',
                            chooiceColor:'blue',
                            dragColor:'yellow',
                        }
                    }
                ]

            }  
        });

        const wrap = ref(null);
        const handleWrapClick = (e)=>{ 
            state.svg.texts.forEach((text,index)=>{ 
                text.path.show = false;
            });
        }
        const handleClick = (element)=>{ 
            state.svg.texts.forEach((text,index)=>{ 
                text.path.show = false;
            });
            state.svg.path.show = false;
            element.path.show = true;

            nextTick(()=>{
                strokeBorder();
            });
        }
        const handleDown = (event,element)=>{
            clearTimeout(state.timer);
            
            state.timer = setTimeout(()=>{
                element.path.show = true;
                element.path.color = element.path.dragColor;
                nextTick(()=>{
                    strokeBorder();
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
            if(state.drag.element)
                state.drag.element.path.color = state.drag.element.path.chooiceColor;
            state.drag.element = null;
        }

        const strokeBorderPath = (path,el)=>{
            path.width = el.getBBox().width;
            path.height = el.getBBox().height;
            path.x = el.getBBox().x;
            path.y = el.getBBox().y;
        }
        const strokeBorder = ()=>{ 
            state.svg.texts.forEach((text,index)=>{ 
                strokeBorderPath(text.path,document.querySelector(`#text-${index}`));
            });
            strokeBorderPath(state.svg.path,document.querySelector(`#svg-bg`));
        }

        onMounted(()=>{
            nextTick(()=>{
                strokeBorder();
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
rect.resize-l,rect.resize-r{
    cursor: e-resize;
}
rect.resize-t,rect.resize-b{
    cursor: s-resize;
}

.paint-wrap{
    border:1px solid #ddd;
    border-radius: 0.5rem;
    padding:1rem;

    .svg-wrap{
        font-size:0;
    }
}
</style>