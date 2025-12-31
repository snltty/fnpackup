<template>
    <el-dialog v-model="projects.current.paint" title="图标设计器"  width="510" top="1vh"
    :close-on-click-modal="false" :close-on-press-escape="false" draggable>
    <div class="paint-wrap flex flex-nowrap">
        
        <div class="svg-wrap" ref="wrap" @click="handleWrapClick">
            <svg width="256" height="256" viewBox="0 0 256 256" xmlns="http://www.w3.org/2000/svg">
                <g>
                    <defs v-if="state.svg.background.fill.type=='lr-gradient'">
                        <linearGradient :id="state.svg.background.fill.type" x1="0%" y1="0%" x2="100%" y2="0%">
                            <stop offset="0%" :stop-color="state.svg.background.fill.color1"/>
                            <stop offset="100%" :stop-color="state.svg.background.fill.color2"/>
                        </linearGradient>
                    </defs>
                    <defs v-if="state.svg.background.fill.type=='tb-gradient'">
                        <linearGradient :id="state.svg.background.fill.type" x1="0%" y1="0%" x2="0%" y2="100%">
                            <stop offset="0%" :stop-color="state.svg.background.fill.color1"/>
                            <stop offset="100%" :stop-color="state.svg.background.fill.color2"/>
                        </linearGradient>
                    </defs>
                    <defs v-if="state.svg.background.fill.type=='ltrb-gradient'">
                        <linearGradient :id="state.svg.background.fill.type" x1="0%" y1="0%" x2="100%" y2="100%">
                            <stop offset="0%" :stop-color="state.svg.background.fill.color1"/>
                            <stop offset="100%" :stop-color="state.svg.background.fill.color2"/>
                        </linearGradient>
                    </defs>
                    <defs v-if="state.svg.background.fill.type=='radial-gradient'">
                        <radialGradient :id="state.svg.background.fill.type" cx="50%" cy="50%" :r="`${state.svg.background.fill.r}%`">
                            <stop offset="0%" :stop-color="state.svg.background.fill.color1"/>
                            <stop offset="100%" :stop-color="state.svg.background.fill.color2"/>
                        </radialGradient>
                    </defs>
                    <rect 
                    :x="state.svg.background.stroke.size/2" :y="state.svg.background.stroke.size/2" 
                    :stroke-width="state.svg.background.stroke.size" :stroke="state.svg.background.stroke.color" 
                    :fill="state.svg.background.fill.type =='fill' ? state.svg.background.fill.color : `url(#${state.svg.background.fill.type})`"
                    :width="256-state.svg.background.stroke.size" :height="256-state.svg.background.stroke.size" :rx="state.svg.background.fill.rx" 
                    @click.stop="handleClick(state.svg.background)" id="svg-bg"></rect>
                    <template v-if="state.svg.background.path.show">
                        <rect :x="state.svg.background.path.x" :y="state.svg.background.path.y" 
                        :width="state.svg.background.path.width" :height="state.svg.background.path.height" 
                        :stroke="state.svg.background.path.color" stroke-width="2" stroke-dasharray="5,3" fill="none"></rect>
                    </template>
                </g>
                <template v-for="(text,index) in state.svg.texts">
                    <g>
                        <text :id="`text-${index}`" :class="{move:text.path.show}"
                        @mousedown.stop="handleDown($event,text)" @click.stop="handleClick(text)"
                        :x="text.x" :y="text.y"
                        :font-family="text.font" :font-size="text.size" :font-weight="text.weight" :fill="text.fill"
                        :stroke-width="text.stroke.size" :stroke="text.stroke.color">{{text.text}}</text>
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
        <div class="control-wrap flex-1" v-if="state.element">
            <template v-if="state.element.type == 'background'">
                <el-descriptions title="设置背景" :column="1" size="small" border class="w-100" :label-width="70">
                    <el-descriptions-item label="填充类型">
                        <el-select v-model="state.element.fill.type" class="flex-1" size="small">
                            <template v-for="(item,index) in state.fillTypes">
                                <el-option :value="item.value" :label="item.name"></el-option>
                            </template>
                        </el-select>
                    </el-descriptions-item>
                    <el-descriptions-item label="填充颜色">
                        <template v-if="state.element.fill.type == 'fill'">
                            <el-color-picker v-model="state.element.fill.color" size="small" />
                        </template>
                        <template v-else>
                            <el-color-picker v-model="state.element.fill.color1" size="small" /> -> 
                            <el-color-picker v-model="state.element.fill.color2" size="small" />
                        </template>
                    </el-descriptions-item>
                    <el-descriptions-item label="径向半径" v-if="state.element.fill.type == 'radial-gradient'">
                        <div class="mgl-1">
                            <el-slider v-model="state.element.fill.r" size="small" :min="0" :max="500" />
                        </div>
                    </el-descriptions-item>
                    <el-descriptions-item label="描边">
                        <el-input-number v-model="state.element.stroke.size" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="描边颜色">
                        <el-color-picker v-model="state.element.stroke.color" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="操作">
                        <el-button size="small">
                            <img src="paint-text.svg" height="12">
                        </el-button>
                        <el-button size="small">
                            <img src="paint-image.svg" height="12">
                        </el-button>
                    </el-descriptions-item>
                </el-descriptions>
            </template>
            <template v-if="state.element.type == 'text'">
                <el-descriptions title="设置文本" :column="1" size="small" border class="w-100" :label-width="70">
                    <el-descriptions-item label="文本">
                        <el-input v-model="state.element.text" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="字体">
                        <el-select v-model="state.element.font" class="flex-1" size="small">
                            <template v-for="(item,index) in state.fonts">
                                <el-option :value="item.value" :label="item.name"></el-option>
                            </template>
                        </el-select>
                    </el-descriptions-item>
                    <el-descriptions-item label="大小">
                        <el-input-number v-model="state.element.size" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="字重">
                        <el-select v-model="state.element.weight" class="flex-1" size="small">
                            <el-option value="normal" label="normal"></el-option>
                            <el-option value="100" label="100"></el-option>
                            <el-option value="200" label="200"></el-option>
                            <el-option value="300" label="300"></el-option>
                            <el-option value="400" label="400"></el-option>
                            <el-option value="500" label="500"></el-option>
                            <el-option value="600" label="600"></el-option>
                            <el-option value="700" label="700"></el-option>
                            <el-option value="800" label="800"></el-option>
                            <el-option value="900" label="900"></el-option>
                            <el-option value="bold" label="bold"></el-option>
                        </el-select>
                    </el-descriptions-item>
                    <el-descriptions-item label="颜色">
                        <el-color-picker v-model="state.element.fill" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="描边">
                        <el-input-number v-model="state.element.stroke.size" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="描边颜色">
                        <el-color-picker v-model="state.element.stroke.color" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="x">
                        <el-input-number v-model="state.element.x" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="y">
                        <el-input-number v-model="state.element.y" size="small" />
                    </el-descriptions-item>
                    <el-descriptions-item label="操作">
                        <el-button type="danger" size="small"><el-icon><DeleteFilled></DeleteFilled></el-icon></el-button>
                    </el-descriptions-item>
                </el-descriptions>
            </template>
        </div>
    </div>
    </el-dialog>
</template>

<script>
import { nextTick, onMounted, onUnmounted, reactive, ref } from 'vue';
import { useProjects } from '../list';
import { DeleteFilled } from '@element-plus/icons-vue';
export default {
    components: {DeleteFilled},
    setup () {
        const projects = useProjects();

        const defaultImg = 'data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBzdGFuZGFsb25lPSJubyI/PjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+PHN2ZyB0PSIxNzY3MjA4MDEwMjg5IiBjbGFzcz0iaWNvbiIgdmlld0JveD0iMCAwIDEwMjUgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHAtaWQ9IjE5NDIiIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIiB3aWR0aD0iNTEyLjUiIGhlaWdodD0iNTEyIj48cGF0aCBkPSJNNTEyIDBDMzc4LjE5NzMzMyAwIDI1Ny4wMjQgMTkuMiAxNjUuNTQ2NjY3IDUyLjQ4Yy00NS44MjQgMTYuNjQtODQuMzA5MzMzIDM2LjYwOC0xMTMuOTIgNjIuMDM3MzMzQzIyLjE4NjY2NyAxMzkuOTQ2NjY3IDAgMTczLjkwOTMzMyAwIDIxMy4zMzMzMzNjMCAzOS40MjQgMjIuMTAxMzMzIDczLjM4NjY2NyA1MS42MjY2NjcgOTguODE2IDI5LjYxMDY2NyAyNS40MjkzMzMgNjguMDk2IDQ1LjM5NzMzMyAxMTMuODM0NjY2IDYyLjAzNzMzNCA1Ljk3MzMzMyAyLjEzMzMzMyAxMy4xNDEzMzMgMy43NTQ2NjcgMTkuMzcwNjY3IDUuODAyNjY2QTg5LjM0NCA4OS4zNDQgMCAwIDAgMTcwLjY2NjY2NyA0MjYuNjY2NjY3YzAgMzQuOTg2NjY3IDIxLjE2MjY2NyA2My4zMTczMzMgNDYuNTA2NjY2IDgzLjM3MDY2NiAxNS43ODY2NjcgMTIuMzczMzMzIDM0Ljk4NjY2NyAyMi41MjggNTUuNjM3MzM0IDMxLjgyOTMzNEMyNjIuNTcwNjY3IDU1OC4yNTA2NjcgMjU2IDU3Ni44NTMzMzMgMjU2IDU5Ny4zMzMzMzNjMCAzMS4xNDY2NjcgMTUuMjc0NjY3IDU4Ljk2NTMzMyAzNS44NCA3OS44NzIgMjAuNTY1MzMzIDIwLjgyMTMzMyA0Ni45MzMzMzMgMzYuNjkzMzMzIDc3LjQ4MjY2NyA0OS44MzQ2NjcgNC41MjI2NjcgMS44NzczMzMgOS44MTMzMzMgMy4zMjggMTQuNTA2NjY2IDUuMTJDMzU5LjUwOTMzMyA3NTEuNDQ1MzMzIDM0MS4zMzMzMzMgNzc3LjM4NjY2NyAzNDEuMzMzMzMzIDgxMC42NjY2NjdjMCAyOC41MDEzMzMgMTMuODI0IDUxLjIgMzIuODUzMzM0IDY5LjM3NkMzNTYuNTIyNjY3IDg5Mi41ODY2NjcgMzQxLjMzMzMzMyA5MTEuNzAxMzMzIDM0MS4zMzMzMzMgOTM4LjY2NjY2N2MwIDM0LjgxNiAyNS4yNTg2NjcgNTYuNzQ2NjY3IDQ4LjQ2OTMzNCA2OC4yNjY2NjYgMjMuMjEwNjY3IDExLjY5MDY2NyA1MC4xNzYgMTcuMDY2NjY3IDc5LjUzMDY2NiAxNy4wNjY2NjcgMjkuMzU0NjY3IDAgNTYuMzItNS4zNzYgNzkuNTMwNjY3LTE3LjA2NjY2NyAyMy4yMTA2NjctMTEuNTIgNDguNDY5MzMzLTMzLjQ1MDY2NyA0OC40NjkzMzMtNjguMjY2NjY2IDAtMS41MzYtMC43NjgtMi42NDUzMzMtMC44NTMzMzMtNC4xODEzMzQgMzYuMDk2LTQuMDEwNjY3IDcwLjE0NC0xMS4zNDkzMzMgOTguMDQ4LTI1LjM0NCAzOC42NTYtMTkuMjg1MzMzIDczLjQ3Mi01MS44ODI2NjcgNzMuNDcyLTk4LjQ3NDY2NmE0Mi42NjY2NjcgNDIuNjY2NjY3IDAgMSAwLTg1LjMzMzMzMyAwYzAgMC41MTItMy4yNDI2NjcgMTAuNTgxMzMzLTI2LjM2OCAyMi4xODY2NjYtMjMuMDQgMTEuNTItNjAuMjQ1MzMzIDIwLjQ4LTEwMS42MzIgMjAuNDgtNDEuMzg2NjY3IDAtNzguNTA2NjY3LTguOTYtMTAxLjYzMi0yMC40OEM0MjkuOTA5MzMzIDgyMS4yNDggNDI2LjY2NjY2NyA4MTEuMTc4NjY3IDQyNi42NjY2NjcgODEwLjY2NjY2N2MwLTAuNTEyIDMuMjQyNjY3LTEwLjU4MTMzMyAyNi4zNjgtMjIuMTg2NjY3IDIzLjA0LTExLjUyIDYwLjI0NTMzMy0yMC40OCAxMDEuNjMyLTIwLjQ4YTQyLjY2NjY2NyA0Mi42NjY2NjcgMCAwIDAgMTEuMDkzMzMzLTEuMzY1MzMzQzU3Ni40MjY2NjcgNzY3LjE0NjY2NyA1ODYuNjY2NjY3IDc2OCA1OTcuMzMzMzMzIDc2OGM3MS45MzYgMCAxNDEuMDU2LTcuOTM2IDE5OS4xNjgtMjMuODkzMzMzIDU4LjAyNjY2Ny0xNS44NzIgMTA3LjUyLTM3LjI5MDY2NyAxMzUuNTA5MzM0LTgxLjA2NjY2N2E0Mi42NjY2NjcgNDIuNjY2NjY3IDAgMCAwLTcyLjAyMTMzNC00Ni4wOGMtNi42NTYgMTAuNDEwNjY3LTM3LjgwMjY2NyAzMS42NTg2NjctODYuMDE2IDQ0Ljg4NTMzM0M3MjUuNzYgNjc0Ljk4NjY2NyA2NjIuODY5MzMzIDY4Mi42NjY2NjcgNTk3LjMzMzMzMyA2ODIuNjY2NjY3Yy03Ny41NjggMC0xNDcuNTQxMzMzLTEzLjkwOTMzMy0xOTQuMzA0LTMzLjk2MjY2N2ExNjQuNzc4NjY3IDE2NC43Nzg2NjcgMCAwIDEtNTAuNTE3MzMzLTMxLjU3MzMzM0MzNDIuODY5MzMzIDYwNy40MDI2NjcgMzQxLjMzMzMzMyA2MDEuNiAzNDEuMzMzMzMzIDU5Ny4zMzMzMzNjMC01LjAzNDY2NyA1LjEyLTE0LjQyMTMzMyAxOS42MjY2NjctMjYuMzY4QzQyOC43MTQ2NjcgNTg3LjYwNTMzMyA1MDkuNDQgNTk3LjMzMzMzMyA1OTcuMzMzMzMzIDU5Ny4zMzMzMzNjOTIuNzU3MzMzIDAgMTgwLjkwNjY2Ny03LjMzODY2NyAyNTMuMzU0NjY3LTIyLjUyOCA3Mi41MzMzMzMtMTUuMTg5MzMzIDEzMS4wNzItMzMuMzY1MzMzIDE2NS4xMi04MC40NjkzMzNhNDIuNjY2NjY3IDQyLjY2NjY2NyAwIDEgMC02OC45NDkzMzMtNTAuMDA1MzMzYy02LjU3MDY2NyA5LjA0NTMzMy00OS40OTMzMzMgMzMuNTM2LTExMy42NjQgNDYuOTMzMzMzQzc2OS4wMjQgNTA0LjgzMiA2ODUuNTY4IDUxMiA1OTcuMzMzMzMzIDUxMmMtMTAyLjE0NCAwLTE5NC42NDUzMzMtMTQuMjUwNjY3LTI1Ny45NjI2NjYtMzUuMzI4LTMxLjY1ODY2Ny0xMC41ODEzMzMtNTUuODkzMzMzLTIzLjA0LTY5LjM3Ni0zMy43MDY2NjdDMjU2LjUxMiA0MzIuMzg0IDI1NiA0MjcuMDA4IDI1NiA0MjYuNjY2NjY3YzAtMC41MTIgMi4zMDQtOS4xMzA2NjcgMjIuODY5MzMzLTIyLjUyOEMzNDkuMDEzMzMzIDQxOC4yMTg2NjcgNDI3LjQzNDY2NyA0MjYuNjY2NjY3IDUxMiA0MjYuNjY2NjY3YzEzMy44MDI2NjcgMCAyNTQuOTc2LTE5LjIgMzQ2LjQ1MzMzMy01Mi40OCA0NS44MjQtMTYuNjQgODQuMzA5MzMzLTM2LjYwOCAxMTMuOTItNjIuMDM3MzM0QzEwMDEuODEzMzMzIDI4Ni43MiAxMDI0IDI1Mi43NTczMzMgMTAyNCAyMTMuMzMzMzMzYzAtMzkuNDI0LTIyLjEwMTMzMy03My4zODY2NjctNTEuNjI2NjY3LTk4LjgxNi0yOS42MTA2NjctMjUuNDI5MzMzLTY4LjA5Ni00NS4zOTczMzMtMTEzLjgzNDY2Ni02Mi4wMzczMzNDNzY2Ljk3NiAxOS4yIDY0NS44MDI2NjcgMCA1MTIgMHogbTAgODUuMzMzMzMzYzEyNS40NCAwIDIzOC45MzMzMzMgMTkuMDI5MzMzIDMxNy4zNTQ2NjcgNDcuNTMwNjY3IDM5LjE2OCAxNC4yNTA2NjcgNjkuMzc2IDMxLjA2MTMzMyA4Ny4yOTYgNDYuNTA2NjY3IDE3LjkyIDE1LjM2IDIyLjAxNiAyNi4yODI2NjcgMjIuMDE2IDMzLjk2MjY2NiAwIDcuNjgtNC4wOTYgMTguNjAyNjY3LTIyLjAxNiAzMy45NjI2NjctMTcuOTIgMTUuNDQ1MzMzLTQ4LjEyOCAzMi4yNTYtODcuMjk2IDQ2LjUwNjY2N0M3NTAuOTMzMzMzIDMyMi4zMDQgNjM3LjM1NDY2NyAzNDEuMzMzMzMzIDUxMiAzNDEuMzMzMzMzYy0xMjUuNDQgMC0yMzguOTMzMzMzLTE5LjAyOTMzMy0zMTcuMzU0NjY3LTQ3LjUzMDY2Ni0zOS4xNjgtMTQuMjUwNjY3LTY5LjM3Ni0zMS4wNjEzMzMtODcuMjk2LTQ2LjUwNjY2N0M4OS40MjkzMzMgMjMxLjkzNiA4NS4zMzMzMzMgMjIxLjAxMzMzMyA4NS4zMzMzMzMgMjEzLjMzMzMzM2MwLTcuNjggNC4wOTYtMTguNjAyNjY3IDIyLjAxNi0zMy45NjI2NjYgMTcuOTItMTUuNDQ1MzMzIDQ4LjEyOC0zMi4yNTYgODcuMjk2LTQ2LjUwNjY2N0MyNzMuMDY2NjY3IDEwNC4zNjI2NjcgMzg2LjY0NTMzMyA4NS4zMzMzMzMgNTEyIDg1LjMzMzMzM3oiIGZpbGw9IiNmZmZmZmYiIHAtaWQ9IjE5NDMiPjwvcGF0aD48L3N2Zz4=';
        
        const svg = {
            background:{
                type:'background',
                stroke:{
                    size: 1,
                    color: '#b9c1d0'
                },
                fill:{
                    type:'tb-gradient',
                    color: '#2568ed',
                    color1: '#f1f1f1',
                    color2: '#d6d6d6',
                    r:0,
                    rx:68
                },
                path:{
                    show:false,
                    x:0,y:0,width:0,height:0,
                    color:'blue',
                    chooiceColor:'blue',
                    dragColor:'yellow',
                }
            },
            texts:[
                {
                    type:'text',
                    text:'FNPACKUP',x:80,y:208,font:'Arial',size:20,fill:'#949494',weight:'bold',
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
            ],
            images:[
                {
                    type:'image',
                    src:defaultImg,
                    x:0,y:0,width:0,height:0,
                    path:{
                        show:false,
                        x:0,y:0,width:0,height:0,
                    }
                }
            ]
        }

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

            element:null,
            types:['background','text','image'],
            fillTypes:[
                {name:'纯色',value:'fill'},
                {name:'左右渐变',value:'lr-gradient'},
                {name:'上下渐变',value:'tb-gradient'},
                {name:'对焦渐变',value:'ltrb-gradient'},
                {name:'径向渐变',value:'radial-gradient'},
            ],
            fonts:[
                {name:'Arial',value:'Arial'},
                {name:'cursive',value:'cursive'},
                {name:'emoji',value:'emoji'},
                {name:'fantasy',value:'fantasy'},
                {name:'fangsong',value:'fangsong'},
                {name:'math',value:'math'},
                {name:'sans-serif',value:'sans-serif'},
                {name:'serif',value:'serif'},
                {name:'黑体',value:'黑体'},
                {name:'楷体',value:'楷体'},
                {name:'微软雅黑',value:'微软雅黑'},
                {name:'宋体',value:'宋体'},
                {name:'新宋体',value:'新宋体'},
                {name:'仿宋',value:'仿宋'},
            ],

            svg: svg
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
            state.svg.background.path.show = false;
            element.path.show = true;

            state.element = element;

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
            strokeBorderPath(state.svg.background.path,document.querySelector(`#svg-bg`));
        }

        onMounted(()=>{
            nextTick(()=>{
                handleClick(state.svg.background);
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

    .svg-wrap{
        font-size:0;
        border-right:1px solid #ddd;
        padding:1rem;
    }
    .control-wrap{
        padding:1rem;
    }
}
</style>