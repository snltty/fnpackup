<template>
    <el-dialog v-model="state.show" title="图标设计器"  width="510" top="1vh"
    :close-on-click-modal="false" :close-on-press-escape="false" draggable class="paint-dialog">
        <div class="paint-wrap flex flex-nowrap">       
            <div v-if="state.svg" class="svg-wrap"  ref="wrap" @click="handleWrapClick"
            :class="{line:state.svg.setting.cline.show}" :style="{'--line-color':state.svg.setting.cline.color}">
                <svg id="svg" width="256" height="256" viewBox="0 0 256 256" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="1685" xmlns:xlink="http://www.w3.org/1999/xlink">
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
                            <radialGradient :id="state.svg.background.fill.type" 
                            :cx="`${state.svg.background.fill.cx}%`" :cy="`${state.svg.background.fill.cy}%`" 
                            :r="`${state.svg.background.fill.r}%`">
                                <stop offset="0%" :stop-color="state.svg.background.fill.color1"/>
                                <stop offset="100%" :stop-color="state.svg.background.fill.color2"/>
                            </radialGradient>
                        </defs>
                        <rect 
                        :x="state.svg.background.stroke.size/2" :y="state.svg.background.stroke.size/2" 
                        :stroke-width="state.svg.background.stroke.size" :stroke="state.svg.background.stroke.color" 
                        :fill="state.svg.background.fill.type =='fill' ? state.svg.background.fill.color : `url(#${state.svg.background.fill.type})`"
                        :width="256-state.svg.background.stroke.size" :height="256-state.svg.background.stroke.size" :rx="state.svg.background.fill.rx" 
                        @click.stop="handleClick(state.svg.background,-1)" id="svg-bg"></rect>
                    </g>
                    <template v-for="(image,index) in state.svg.images">
                        <g>
                            <image :id="`image-${index}`" :class="{move:image.path.show}" :href="image.src" 
                            :x="image.x" :y="image.y" :width="image.width" :height="image.height" preserveAspectRatio="xMidYMid slice"
                            @mousedown.stop="handleDown($event,image,index)" @click.stop="handleClick(image,index)"/>
                        </g>
                    </template>
                    <template v-for="(text,index) in state.svg.texts">
                        <g>
                            <text :id="`text-${index}`" :class="{move:text.path.show}"
                            @mousedown.stop="handleDown($event,text,index)" @click.stop="handleClick(text,index)"
                            :x="text.x" :y="text.y"
                            :font-family="text.font" :font-size="text.size" :font-weight="text.weight" :fill="text.fill"
                            :stroke-width="text.stroke.size" :stroke="text.stroke.color">{{text.text}}</text>
                        </g>
                    </template>
                    <template v-if="state.element && state.element.path.show">
                        <g class="sub-line"> 
                            <rect :x="state.element.path.x" :y="state.element.path.y" :width="state.element.path.width" :height="state.element.path.height" 
                            :stroke="state.element.path.color" stroke-width="2" stroke-dasharray="5,3" fill="none"></rect>
                            <rect @click.stop :fill="state.element.path.color" :x="state.element.path.x-4" :y="state.element.path.y+state.element.path.height/2-4" width="8" height="8" class="resize-l"></rect>
                            <rect @click.stop :fill="state.element.path.color" :x="state.element.path.x+state.element.path.width-4" :y="state.element.path.y+state.element.path.height/2-4" width="8" height="8" class="resize-r"></rect>
                            <rect @click.stop :fill="state.element.path.color" :x="state.element.path.x+state.element.path.width/2-4" :y="state.element.path.y-4" width="8" height="8" class="resize-t"></rect>
                            <rect @click.stop :fill="state.element.path.color" :x="state.element.path.x+state.element.path.width/2-4" :y="state.element.path.y+state.element.path.height-4" width="8" height="8" class="resize-b"></rect>
                        </g>
                    </template>
                </svg>
                <div class="setting-wrap mgt-1 flex">
                    <el-color-picker v-model="state.svg.setting.cline.color" size="small" show-alpha />
                    <el-checkbox v-model="state.svg.setting.cline.show" size="small" class="mgl-1">中心线</el-checkbox>
                    <span class="flex-1"></span>
                    <el-button plain size="small" @click="handleAddText">
                        +<img src="paint-text.svg" height="14">
                    </el-button>
                    <el-button plain size="small" @click="handleAddImage">
                        +<img src="paint-image.svg" height="14">
                    </el-button>
                </div>
                <div class="btns t-c mgt-1">
                    <el-button plain :loading="state.loading" @click="handleSaveSvg" size="small">保存设计</el-button>
                    <el-button plain type="primary" class="mgl-1" :loading="state.loading" @click="handleSaveIcon" size="small">保存图标</el-button>
                    <el-dropdown class="mgl-1">
                        <el-button plain type="info" size="small">
                            下载<el-icon><ArrowDown /></el-icon>
                        </el-button>
                        <template #dropdown>
                            <el-dropdown-menu>
                                <el-dropdown-item @click="handleDownloadIcon('json')">下载json</el-dropdown-item>
                                <el-dropdown-item @click="handleDownloadIcon('svg')">下载svg</el-dropdown-item>
                                <el-dropdown-item @click="handleDownloadIcon('png')">下载png</el-dropdown-item>
                            </el-dropdown-menu>
                        </template>
                    </el-dropdown>
                </div>
            </div>
            <div class="control-wrap flex-1" v-if="state.element">
                <template v-if="state.element.type == 'background'">
                    <el-descriptions title="设置背景" :column="1" size="small" border class="w-100" :label-width="70">
                        <el-descriptions-item label="圆角半径">
                            <div class="mgl-1">
                                <el-slider v-model="state.element.fill.rx" size="small" :min="0" :max="256" />
                            </div>
                        </el-descriptions-item>
                        <el-descriptions-item label="填充类型">
                            <el-select v-model="state.element.fill.type" class="flex-1" size="small">
                                <template v-for="(item,index) in state.fillTypes">
                                    <el-option :value="item.value" :label="item.name"></el-option>
                                </template>
                            </el-select>
                        </el-descriptions-item>
                        <el-descriptions-item label="填充颜色">
                            <template v-if="state.element.fill.type == 'fill'">
                                <el-color-picker v-model="state.element.fill.color" size="small" show-alpha />
                            </template>
                            <template v-else>
                                <el-color-picker v-model="state.element.fill.color1" size="small" show-alpha /> -> 
                                <el-color-picker v-model="state.element.fill.color2" size="small" show-alpha />
                            </template>
                        </el-descriptions-item>
                        <el-descriptions-item label="径向半径" v-if="state.element.fill.type == 'radial-gradient'">
                            <div class="mgl-1">
                                <el-slider v-model="state.element.fill.r" size="small" :min="0" :max="500" />
                            </div>
                        </el-descriptions-item>
                        <el-descriptions-item label="径向x" v-if="state.element.fill.type == 'radial-gradient'">
                            <div class="mgl-1">
                                <el-slider v-model="state.element.fill.cx" size="small" :min="0" :max="100" />
                            </div>
                        </el-descriptions-item>
                        <el-descriptions-item label="径向y" v-if="state.element.fill.type == 'radial-gradient'">
                            <div class="mgl-1">
                                <el-slider v-model="state.element.fill.cy" size="small" :min="0" :max="100" />
                            </div>
                        </el-descriptions-item>
                        <el-descriptions-item label="描边">
                            <el-input-number v-model="state.element.stroke.size" size="small" />
                        </el-descriptions-item>
                        <el-descriptions-item label="描边颜色">
                            <el-color-picker v-model="state.element.stroke.color" size="small" show-alpha />
                        </el-descriptions-item>
                    </el-descriptions>
                </template>
                <template v-if="state.element.type == 'text'">
                    <el-descriptions title="设置文本" :column="1" size="small" border class="w-100" :label-width="70">
                        <el-descriptions-item label="文本">
                            <el-input v-model="state.element.text" size="small" @change="handleStrokeBorder"/>
                        </el-descriptions-item>
                        <el-descriptions-item label="字体">
                            <el-select v-model="state.element.font" class="flex-1" size="small" @change="handleStrokeBorder">
                                <template v-for="(item,index) in state.fonts">
                                    <el-option :value="item.value" :label="item.name"></el-option>
                                </template>
                            </el-select>
                        </el-descriptions-item>
                        <el-descriptions-item label="大小">
                            <el-input-number v-model="state.element.size" size="small" @change="handleStrokeBorder"/>
                        </el-descriptions-item>
                        <el-descriptions-item label="字重">
                            <el-select v-model="state.element.weight" class="flex-1" size="small" @change="handleStrokeBorder">
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
                            <el-color-picker v-model="state.element.fill" size="small" show-alpha />
                        </el-descriptions-item>
                        <el-descriptions-item label="描边">
                            <el-input-number v-model="state.element.stroke.size" size="small" />
                        </el-descriptions-item>
                        <el-descriptions-item label="描边颜色">
                            <el-color-picker v-model="state.element.stroke.color" size="small" show-alpha />
                        </el-descriptions-item>
                        <el-descriptions-item label="x">
                            <el-input-number v-model="state.element.x" size="small" @change="handleStrokeBorder"/>
                        </el-descriptions-item>
                        <el-descriptions-item label="y">
                            <el-input-number v-model="state.element.y" size="small" @change="handleStrokeBorder"/>
                        </el-descriptions-item>
                        <el-descriptions-item label="操作">
                            <el-popconfirm title="确定删除吗?" confirm-button-text="确认" cancel-button-text="取消" @confirm="handleDelete(state.svg.texts)">
                                <template #reference>
                                <el-button type="danger" size="small"><el-icon><DeleteFilled></DeleteFilled></el-icon></el-button>
                                </template>
                            </el-popconfirm>
                        </el-descriptions-item>
                    </el-descriptions>
                </template>
                <template v-if="state.element.type == 'image'">
                    <el-descriptions title="设置图像" :column="1" size="small" border class="w-100" :label-width="70">
                        <el-descriptions-item label="锁定比例">
                            <el-checkbox v-model="state.element.lock">锁定比例</el-checkbox>
                        </el-descriptions-item>
                        <el-descriptions-item label="宽">
                            <el-input-number v-model="state.element.width" size="small" @change="handleImageWidthChange" />
                        </el-descriptions-item>
                        <el-descriptions-item label="高">
                            <el-input-number v-model="state.element.height" size="small" @change="handleImageHeightChange" />
                        </el-descriptions-item>
                        <el-descriptions-item label="x">
                            <el-input-number v-model="state.element.x" size="small" @change="handleStrokeBorder" />
                        </el-descriptions-item>
                        <el-descriptions-item label="y">
                            <el-input-number v-model="state.element.y" size="small" @change="handleStrokeBorder"/>
                        </el-descriptions-item>
                        <el-descriptions-item label="操作">
                            <el-popconfirm title="确定删除吗?" confirm-button-text="确认" cancel-button-text="取消" @confirm="handleDelete(state.svg.images)">
                                <template #reference>
                                <el-button plain type="danger" size="small"><el-icon><DeleteFilled></DeleteFilled></el-icon></el-button>
                                </template>
                            </el-popconfirm>
                        </el-descriptions-item>
                    </el-descriptions>
                </template>
            </div>
        </div>
        <input multiple type="file" ref="input" accept="image/*" @change="handleFileChange"></input>
    </el-dialog>
    <el-dialog v-model="state.showSave" width="320" top="1vh"
    :close-on-click-modal="false" :close-on-press-escape="false" draggable>
        <el-descriptions title="选择保存项" :column="1" size="small" border class="w-100" :label-width="80">
            <template v-for="icon in state.icons">
                <el-descriptions-item :label="icon.name">
                    <div class="flex">
                        <div style="width:50%;" class="mgr-1"><el-input-number v-model="icon.size" size="small" /></div>
                        <el-checkbox v-model="icon.use" size="small">保存此项</el-checkbox>
                    </div>
                </el-descriptions-item>
            </template>
            <el-descriptions-item>
                <el-button plain type="primary" :loading="state.loading" @click="handleConfirmIcon">
                    <template v-if="state.loading">
                        {{ state.process.progress }}
                    </template>
                    <template v-else>确定保存到所选项</template>
                </el-button>
            </el-descriptions-item>
        </el-descriptions>
    </el-dialog>

</template>

<script>
import { nextTick, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { useProjects } from '../list';
import { ArrowDown, DeleteFilled } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox, ElNotification } from 'element-plus';
import { fetchFileRead, fetchFileUpload, fetchFileWrite, xhrApi } from '@/api/api';
import { useLogger } from '../../logger';
export default {
    components: {DeleteFilled,ArrowDown},
    props: ['modelValue'],
    setup (props,{emit}) {
        const logger = useLogger();
        const projects = useProjects();

        const root = projects.value.page.root.join('/');
        const path = `${root}/building/icon_design.json`;
        const appname = projects.value.page.path.split('/')[1];
        const defaultImg = 'data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBzdGFuZGFsb25lPSJubyI/PjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+PHN2ZyB0PSIxNzY3MjA4MDEwMjg5IiBjbGFzcz0iaWNvbiIgdmlld0JveD0iMCAwIDEwMjUgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHAtaWQ9IjE5NDIiIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIiB3aWR0aD0iNTEyLjUiIGhlaWdodD0iNTEyIj48cGF0aCBkPSJNNTEyIDBDMzc4LjE5NzMzMyAwIDI1Ny4wMjQgMTkuMiAxNjUuNTQ2NjY3IDUyLjQ4Yy00NS44MjQgMTYuNjQtODQuMzA5MzMzIDM2LjYwOC0xMTMuOTIgNjIuMDM3MzMzQzIyLjE4NjY2NyAxMzkuOTQ2NjY3IDAgMTczLjkwOTMzMyAwIDIxMy4zMzMzMzNjMCAzOS40MjQgMjIuMTAxMzMzIDczLjM4NjY2NyA1MS42MjY2NjcgOTguODE2IDI5LjYxMDY2NyAyNS40MjkzMzMgNjguMDk2IDQ1LjM5NzMzMyAxMTMuODM0NjY2IDYyLjAzNzMzNCA1Ljk3MzMzMyAyLjEzMzMzMyAxMy4xNDEzMzMgMy43NTQ2NjcgMTkuMzcwNjY3IDUuODAyNjY2QTg5LjM0NCA4OS4zNDQgMCAwIDAgMTcwLjY2NjY2NyA0MjYuNjY2NjY3YzAgMzQuOTg2NjY3IDIxLjE2MjY2NyA2My4zMTczMzMgNDYuNTA2NjY2IDgzLjM3MDY2NiAxNS43ODY2NjcgMTIuMzczMzMzIDM0Ljk4NjY2NyAyMi41MjggNTUuNjM3MzM0IDMxLjgyOTMzNEMyNjIuNTcwNjY3IDU1OC4yNTA2NjcgMjU2IDU3Ni44NTMzMzMgMjU2IDU5Ny4zMzMzMzNjMCAzMS4xNDY2NjcgMTUuMjc0NjY3IDU4Ljk2NTMzMyAzNS44NCA3OS44NzIgMjAuNTY1MzMzIDIwLjgyMTMzMyA0Ni45MzMzMzMgMzYuNjkzMzMzIDc3LjQ4MjY2NyA0OS44MzQ2NjcgNC41MjI2NjcgMS44NzczMzMgOS44MTMzMzMgMy4zMjggMTQuNTA2NjY2IDUuMTJDMzU5LjUwOTMzMyA3NTEuNDQ1MzMzIDM0MS4zMzMzMzMgNzc3LjM4NjY2NyAzNDEuMzMzMzMzIDgxMC42NjY2NjdjMCAyOC41MDEzMzMgMTMuODI0IDUxLjIgMzIuODUzMzM0IDY5LjM3NkMzNTYuNTIyNjY3IDg5Mi41ODY2NjcgMzQxLjMzMzMzMyA5MTEuNzAxMzMzIDM0MS4zMzMzMzMgOTM4LjY2NjY2N2MwIDM0LjgxNiAyNS4yNTg2NjcgNTYuNzQ2NjY3IDQ4LjQ2OTMzNCA2OC4yNjY2NjYgMjMuMjEwNjY3IDExLjY5MDY2NyA1MC4xNzYgMTcuMDY2NjY3IDc5LjUzMDY2NiAxNy4wNjY2NjcgMjkuMzU0NjY3IDAgNTYuMzItNS4zNzYgNzkuNTMwNjY3LTE3LjA2NjY2NyAyMy4yMTA2NjctMTEuNTIgNDguNDY5MzMzLTMzLjQ1MDY2NyA0OC40NjkzMzMtNjguMjY2NjY2IDAtMS41MzYtMC43NjgtMi42NDUzMzMtMC44NTMzMzMtNC4xODEzMzQgMzYuMDk2LTQuMDEwNjY3IDcwLjE0NC0xMS4zNDkzMzMgOTguMDQ4LTI1LjM0NCAzOC42NTYtMTkuMjg1MzMzIDczLjQ3Mi01MS44ODI2NjcgNzMuNDcyLTk4LjQ3NDY2NmE0Mi42NjY2NjcgNDIuNjY2NjY3IDAgMSAwLTg1LjMzMzMzMyAwYzAgMC41MTItMy4yNDI2NjcgMTAuNTgxMzMzLTI2LjM2OCAyMi4xODY2NjYtMjMuMDQgMTEuNTItNjAuMjQ1MzMzIDIwLjQ4LTEwMS42MzIgMjAuNDgtNDEuMzg2NjY3IDAtNzguNTA2NjY3LTguOTYtMTAxLjYzMi0yMC40OEM0MjkuOTA5MzMzIDgyMS4yNDggNDI2LjY2NjY2NyA4MTEuMTc4NjY3IDQyNi42NjY2NjcgODEwLjY2NjY2N2MwLTAuNTEyIDMuMjQyNjY3LTEwLjU4MTMzMyAyNi4zNjgtMjIuMTg2NjY3IDIzLjA0LTExLjUyIDYwLjI0NTMzMy0yMC40OCAxMDEuNjMyLTIwLjQ4YTQyLjY2NjY2NyA0Mi42NjY2NjcgMCAwIDAgMTEuMDkzMzMzLTEuMzY1MzMzQzU3Ni40MjY2NjcgNzY3LjE0NjY2NyA1ODYuNjY2NjY3IDc2OCA1OTcuMzMzMzMzIDc2OGM3MS45MzYgMCAxNDEuMDU2LTcuOTM2IDE5OS4xNjgtMjMuODkzMzMzIDU4LjAyNjY2Ny0xNS44NzIgMTA3LjUyLTM3LjI5MDY2NyAxMzUuNTA5MzM0LTgxLjA2NjY2N2E0Mi42NjY2NjcgNDIuNjY2NjY3IDAgMCAwLTcyLjAyMTMzNC00Ni4wOGMtNi42NTYgMTAuNDEwNjY3LTM3LjgwMjY2NyAzMS42NTg2NjctODYuMDE2IDQ0Ljg4NTMzM0M3MjUuNzYgNjc0Ljk4NjY2NyA2NjIuODY5MzMzIDY4Mi42NjY2NjcgNTk3LjMzMzMzMyA2ODIuNjY2NjY3Yy03Ny41NjggMC0xNDcuNTQxMzMzLTEzLjkwOTMzMy0xOTQuMzA0LTMzLjk2MjY2N2ExNjQuNzc4NjY3IDE2NC43Nzg2NjcgMCAwIDEtNTAuNTE3MzMzLTMxLjU3MzMzM0MzNDIuODY5MzMzIDYwNy40MDI2NjcgMzQxLjMzMzMzMyA2MDEuNiAzNDEuMzMzMzMzIDU5Ny4zMzMzMzNjMC01LjAzNDY2NyA1LjEyLTE0LjQyMTMzMyAxOS42MjY2NjctMjYuMzY4QzQyOC43MTQ2NjcgNTg3LjYwNTMzMyA1MDkuNDQgNTk3LjMzMzMzMyA1OTcuMzMzMzMzIDU5Ny4zMzMzMzNjOTIuNzU3MzMzIDAgMTgwLjkwNjY2Ny03LjMzODY2NyAyNTMuMzU0NjY3LTIyLjUyOCA3Mi41MzMzMzMtMTUuMTg5MzMzIDEzMS4wNzItMzMuMzY1MzMzIDE2NS4xMi04MC40NjkzMzNhNDIuNjY2NjY3IDQyLjY2NjY2NyAwIDEgMC02OC45NDkzMzMtNTAuMDA1MzMzYy02LjU3MDY2NyA5LjA0NTMzMy00OS40OTMzMzMgMzMuNTM2LTExMy42NjQgNDYuOTMzMzMzQzc2OS4wMjQgNTA0LjgzMiA2ODUuNTY4IDUxMiA1OTcuMzMzMzMzIDUxMmMtMTAyLjE0NCAwLTE5NC42NDUzMzMtMTQuMjUwNjY3LTI1Ny45NjI2NjYtMzUuMzI4LTMxLjY1ODY2Ny0xMC41ODEzMzMtNTUuODkzMzMzLTIzLjA0LTY5LjM3Ni0zMy43MDY2NjdDMjU2LjUxMiA0MzIuMzg0IDI1NiA0MjcuMDA4IDI1NiA0MjYuNjY2NjY3YzAtMC41MTIgMi4zMDQtOS4xMzA2NjcgMjIuODY5MzMzLTIyLjUyOEMzNDkuMDEzMzMzIDQxOC4yMTg2NjcgNDI3LjQzNDY2NyA0MjYuNjY2NjY3IDUxMiA0MjYuNjY2NjY3YzEzMy44MDI2NjcgMCAyNTQuOTc2LTE5LjIgMzQ2LjQ1MzMzMy01Mi40OCA0NS44MjQtMTYuNjQgODQuMzA5MzMzLTM2LjYwOCAxMTMuOTItNjIuMDM3MzM0QzEwMDEuODEzMzMzIDI4Ni43MiAxMDI0IDI1Mi43NTczMzMgMTAyNCAyMTMuMzMzMzMzYzAtMzkuNDI0LTIyLjEwMTMzMy03My4zODY2NjctNTEuNjI2NjY3LTk4LjgxNi0yOS42MTA2NjctMjUuNDI5MzMzLTY4LjA5Ni00NS4zOTczMzMtMTEzLjgzNDY2Ni02Mi4wMzczMzNDNzY2Ljk3NiAxOS4yIDY0NS44MDI2NjcgMCA1MTIgMHogbTAgODUuMzMzMzMzYzEyNS40NCAwIDIzOC45MzMzMzMgMTkuMDI5MzMzIDMxNy4zNTQ2NjcgNDcuNTMwNjY3IDM5LjE2OCAxNC4yNTA2NjcgNjkuMzc2IDMxLjA2MTMzMyA4Ny4yOTYgNDYuNTA2NjY3IDE3LjkyIDE1LjM2IDIyLjAxNiAyNi4yODI2NjcgMjIuMDE2IDMzLjk2MjY2NiAwIDcuNjgtNC4wOTYgMTguNjAyNjY3LTIyLjAxNiAzMy45NjI2NjctMTcuOTIgMTUuNDQ1MzMzLTQ4LjEyOCAzMi4yNTYtODcuMjk2IDQ2LjUwNjY2N0M3NTAuOTMzMzMzIDMyMi4zMDQgNjM3LjM1NDY2NyAzNDEuMzMzMzMzIDUxMiAzNDEuMzMzMzMzYy0xMjUuNDQgMC0yMzguOTMzMzMzLTE5LjAyOTMzMy0zMTcuMzU0NjY3LTQ3LjUzMDY2Ni0zOS4xNjgtMTQuMjUwNjY3LTY5LjM3Ni0zMS4wNjEzMzMtODcuMjk2LTQ2LjUwNjY2N0M4OS40MjkzMzMgMjMxLjkzNiA4NS4zMzMzMzMgMjIxLjAxMzMzMyA4NS4zMzMzMzMgMjEzLjMzMzMzM2MwLTcuNjggNC4wOTYtMTguNjAyNjY3IDIyLjAxNi0zMy45NjI2NjYgMTcuOTItMTUuNDQ1MzMzIDQ4LjEyOC0zMi4yNTYgODcuMjk2LTQ2LjUwNjY2N0MyNzMuMDY2NjY3IDEwNC4zNjI2NjcgMzg2LjY0NTMzMyA4NS4zMzMzMzMgNTEyIDg1LjMzMzMzM3oiIGZpbGw9IiNmZmZmZmYiIHAtaWQ9IjE5NDMiPjwvcGF0aD48L3N2Zz4=';
        const defaultSvg = {
            setting:{
                cline:{
                    show:true,
                    color:'#ddd',
                }
            },
            background:{
                type:'background',
                stroke:{
                    size: 1,
                    color: '#3b6fd5'
                },
                fill:{
                    type:'radial-gradient',
                    color: '#2568ed',
                    color1: '#5c91fb',
                    color2: '#2568ed',
                    r:96,
                    cx:50,
                    cy:50,
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
                    text:'FNPACKUP',x:73,y:208,font:'Arial',size:20,fill:'#ffffff',weight:'bold',
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
                    x:78,y:78,width:100,height:100,
                    lock:true,
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

        const state = reactive({
            show:true,
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

            loading:false,
            element:null,
            elementIndex:-1,
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

            icons:[
                {name:'应用大图标',path:`${root}/ICON_256.PNG`,use:true,size:256},
                {name:'应用小图标',path:`${root}/ICON.PNG`,use:true,size:256},
                {name:'入口大图标',path:`${root}/app/ui/images/icon_256.png`,use:true,size:256},
                {name:'入口小图标',path:`${root}/app/ui/images/icon_64.png`,use:true,size:256},
            ],
            showSave:false,
            process:{
                total:0,
                current:0,
                progress:'0%'
            },

            svg: null
        });
        watch(()=>state.show,(val)=>{ 
            if(!val) { 
                setTimeout(()=>{ 
                    emit('update:modelValue', val);
                },300);
            }
        });


        const wrap = ref(null);
        const handleWrapClick = (e)=>{ 
            state.svg.texts.forEach((text,index)=>{ 
                text.path.show = false;
            });
        }
        const clearPath = ()=>{
            state.svg.texts.forEach((text,index)=>{ 
                text.path.show = false;
            });
            state.svg.images.forEach((image,index)=>{ 
                image.path.show = false;
            });
            state.svg.background.path.show = false; 
        }
        const handleClick = (element,index)=>{ 
            clearPath();
            element.path.show = true;

            state.element = element;
            state.elementIndex = index;

            nextTick(()=>{
                strokeBorder();
            });
        }
        const handleDelete = (list)=>{ 
            list.splice(state.elementIndex,1);
            state.element = null;
            state.elementIndex = -1;
        }
        const handleAddText = ()=>{ 
            ElMessageBox.prompt('请输入文字', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
            }).then(({ value }) => {
                if(!value) return;
                state.svg.texts.push({
                    type:'text',
                    text:value,x:87,y:40,font:'Arial',size:20,fill:'#ffffff',weight:'bold',
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
                });
                handleClick(state.svg.texts[state.svg.texts.length-1],state.svg.texts.length-1);
            }).catch(() => { 
            });
        }

        const input = ref(null);
        const handleAddImage = ()=>{ 
            input.value.click();
        }
        const handleFileChange = (e)=>{ 
            const files = Array.from(e.target.files);
            input.value.value = '';

            const image = new Image();
            image.crossOrigin = 'anonymous';
            image.src = URL.createObjectURL(files[0]);
            image.onload = async () => { 
                const reader = new FileReader();
                reader.onload = function(event) {

                    const {width,height} = image.width > image.height 
                    ? {width:100, height:Math.round((image.height / image.width) * 100)} 
                    : {width:Math.round((image.width / image.height) * 100), height:100};
                    let base64 = reader.result;
                    state.svg.images.push({
                        type:'image',
                        src:base64,
                        x:78,y:78,width:width,height,
                        lock:true,
                        path:{
                            show:false,
                            x:0,y:0,width:0,height:0,
                            color:'blue',
                            chooiceColor:'blue',
                            dragColor:'yellow',
                        }
                    });
                };
                reader.readAsDataURL(files[0]);
            };

            

            
        }
        const handleImageWidthChange = (newValue,oldValue)=>{
            if(state.element.lock)
            state.element.height = Math.round((newValue/oldValue) * state.element.height); 
            handleStrokeBorder();
        }
        const handleImageHeightChange = (newValue,oldValue)=>{
            if(state.element.lock)
            state.element.width = Math.round((newValue/oldValue) * state.element.width); 
            handleStrokeBorder();
        }
        const handleStrokeBorder = ()=>{ 
            nextTick(()=>{
                strokeBorder();
            });
        }

        const handleDown = (event,element,index)=>{
            clearTimeout(state.timer);
            
            state.timer = setTimeout(()=>{
                state.element = element;
                state.elementIndex = index;
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
            state.svg.images.forEach((image,index)=>{ 
                strokeBorderPath(image.path,document.querySelector(`#image-${index}`));
            });
            strokeBorderPath(state.svg.background.path,document.querySelector(`#svg-bg`));
        }

        const loadSvg = ()=>{ 
            state.loading = true;
            fetchFileRead(path)
            .then((res)=>{
                state.svg = JSON.parse(res || JSON.stringify(defaultSvg)); 
            }).catch((e)=>{
                logger.value.error(`${e}`);
                state.svg = defaultSvg;
            }).finally(()=>{
                state.loading = false;
                if(state.svg){
                    handleClick(state.svg.background);
                }
            });
        }
        const handleSaveSvg = ()=>{ 
            state.loading = true;
            fetchFileWrite(path,JSON.stringify(state.svg,null,2))
            .then(res => {
                if(res){
                    logger.value.error(res);
                }else{
                    ElMessage.success('保存成功');
                    logger.value.success(`保存成功`);
                    projects.value.load();
                }
            }).catch((e)=>{
                logger.value.error(`${e}`);
            }).finally(()=>{
                state.loading = false;
            });
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
        const handleSaveIcon = ()=>{
            state.showSave = true;
        }
        const handleConfirmIcon = ()=>{ 
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
                const svgElement = document.querySelector('#svg');
                const svgEl = svgElement.cloneNode(true);
                svgEl.setAttribute('width', fileObj.size*4);
                svgEl.setAttribute('height',fileObj.size*4);
                svgEl.querySelectorAll('.sub-line').forEach(item=>{
                    svgEl.removeChild(item);
                });
                const svgBlob = new Blob([svgEl.outerHTML], { type: 'image/svg+xml;charset=utf-8' });
                const url = URL.createObjectURL(svgBlob);
                const file = await toFile(url,fileObj.size,fileObj.path.split('/').pop());
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
        const handleDownloadIcon = (type)=>{ 
            const svgElement = document.querySelector('#svg');
            const svgEl = svgElement.cloneNode(true);
            svgEl.querySelectorAll('.sub-line').forEach(item=>{
                svgEl.removeChild(item);
            });

            const download = (file) => {
                const a = document.createElement('a');
                a.target='_blank';
                a.href = URL.createObjectURL(file);
                a.download = file.name;
                a.click(); 
            };
            switch(type){
                case 'svg':
                    download(new File([new Blob([
                        `<?xml version="1.0" standalone="no"?><!DOCTYPE svg PUBLIC "-//W3C//DTD SVG 1.1//EN" "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd">${svgEl.outerHTML}`
                        ], { type: 'image/svg+xml;charset=utf-8' })], `${appname}.svg`, {
                        type: 'image/svg+xml',
                        lastModified: Date.now()
                    }));
                    break;
                case 'json':
                    download(new File([new Blob([JSON.stringify(state.svg,null,2)], { type: 'application/json;charset=utf-8' })], `${appname}.json`, {
                        type: 'application/json',
                        lastModified: Date.now()
                    }));
                    break;
                case 'png':
                    {
                        ElMessageBox.prompt('请输入下载尺寸', '提示', {
                            confirmButtonText: '确定',
                            cancelButtonText: '取消',
                            inputPattern: /^[0-9]+$/,
                            inputValue:256
                        }).then(async ({ value }) => {
                            if(!value) return;
                            download(await toFile(URL.createObjectURL(new Blob([svgEl.outerHTML], { type: 'image/svg+xml;charset=utf-8' })),+value,`${appname}.png`));
                        }).catch(() => { 
                        })
                    }
                break;
            }
        }

        onMounted(()=>{
            document.addEventListener('mouseup',handleUp);
            document.addEventListener('mousemove',handleMove);

            loadSvg();
        });
        onUnmounted(()=>{
            document.removeEventListener('mouseup',handleUp);
            document.removeEventListener('mousemove',handleMove);
        });

        return {projects,state,wrap,input,handleWrapClick,handleClick,handleDown,
            handleAddText,handleAddImage,handleFileChange,handleStrokeBorder,
            handleImageWidthChange,handleImageHeightChange,handleDelete,
            handleSaveSvg,handleSaveIcon,handleConfirmIcon,handleDownloadIcon}
    }
}
</script>

<style lang="stylus" scoped>
input[type=file]
    opacity: 0;
    z-index: -1;
    position: absolute;
text,image{
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
    
    border:1px solid var(--main-border-color);
    border-radius: 0.5rem;
    

    .svg-wrap{
        font-size:0;
        border-right:1px solid var(--main-border-color);
        padding:1rem;
        
        position: relative;
        &.line:before{
            content:'';
            display:block;
            width:256px;
            border-bottom:1px dashed var(--line-color);
            position:absolute;
            top:137px;
            left:1rem;
        }
        &.line:after{
            content:'';
            display:block;
            height:256px;
            border-right:1px dashed var(--line-color);
            position:absolute;
            left:137px;
            top:1rem;
        }

        
    }
    .control-wrap{
        padding:1rem;
    }
}
</style>