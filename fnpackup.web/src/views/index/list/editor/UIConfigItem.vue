<template>
     <div class="item">
        <el-form label-width="80">
            <el-form-item label="入口标识">
                <el-input v-model="item._key" ></el-input>
            </el-form-item>
            <el-form-item label="显示标题" prop="title">
                <el-input v-model="item.title" />
            </el-form-item>
            <el-form-item label="显示图标" prop="icon">
                <el-input v-model="item.icon" />
            </el-form-item>
            <el-form-item label="" label-width="0" class="mgb-0">
                <el-row>
                    <el-col :span="9">
                        <el-form-item label="打开协议" prop="protocol">
                            <el-select v-model="item.protocol">
                                <el-option key="http" label="http" value="http"></el-option>
                                <el-option key="https" label="https" value="https"></el-option>
                            </el-select>
                        </el-form-item>
                    </el-col>
                    <el-col :span="8">
                        <el-form-item label="端口" prop="port" label-width="40">
                            <el-input v-model="item.port" />
                        </el-form-item>
                    </el-col>
                    <el-col :span="7">
                        <el-form-item label="路径" prop="url" label-width="40">
                            <el-input v-model="item.url" />
                        </el-form-item>
                    </el-col>
                </el-row>
            </el-form-item>
            <el-form-item label="">
                <div class="flex w-100">
                    <span>{{ item.protocol }}://fn_ip:{{ item.port }}{{ item.url }}</span>
                    <span class="flex-1"></span>
                    <span>
                        <el-radio-group v-model="item.type">
                            <el-radio value="url" class="mgr-1">新标签打开</el-radio>
                            <el-radio value="iframe">内嵌打开</el-radio>
                        </el-radio-group>
                    </span>
                </div>
            </el-form-item>
            <el-form-item label="访问权限" prop="allUsers">
                <el-radio-group v-model="item.allUsers">
                    <el-radio :value="true" class="mgr-1">所有用户可访问</el-radio>
                    <el-radio :value="false">仅管理员可访问</el-radio>
                </el-radio-group>
            </el-form-item>
            
            <template v-if="item.allUsers==false">
                <el-form-item label="" label-width="0">
                    <el-row class="w-100">
                        <el-col :span="10">
                            <el-form-item label="访问权限" prop="accessPerm">
                                <el-select v-model="item.control.accessPerm">
                                    <el-option key="editable" label="可编辑" value="editable"></el-option>
                                    <el-option key="readonly" label="只读" value="readonly"></el-option>
                                    <el-option key="hidden" label="隐藏" value="hidden"></el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>
                        <el-col :span="7">
                            <el-form-item label="端口" prop="portPerm" label-width="40">
                                <el-select v-model="item.control.portPerm">
                                    <el-option key="editable" label="可编辑" value="editable"></el-option>
                                    <el-option key="readonly" label="只读" value="readonly"></el-option>
                                    <el-option key="hidden" label="隐藏" value="hidden"></el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>
                        <el-col :span="7">
                            <el-form-item label="路径" prop="pathPerm" label-width="40">
                                <el-select v-model="item.control.pathPerm">
                                    <el-option key="editable" label="可编辑" value="editable"></el-option>
                                    <el-option key="readonly" label="只读" value="readonly"></el-option>
                                    <el-option key="hidden" label="隐藏" value="hidden"></el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>
                    </el-row>
                </el-form-item>
            </template>
            
            <el-form-item label="文件右键" prop="noDisplay">
                <el-radio-group v-model="item.noDisplay">
                    <el-radio :value="true" class="mgr-1">仅右键菜单显示</el-radio>
                    <el-radio :value="false">桌面和右键菜单显示</el-radio>
                </el-radio-group>
            </el-form-item>
            <el-form-item label="关联类型" prop="fileTypes">
                <el-select v-model="item.fileTypes" filterable clearable allow-create 
                multiple collapse-tags collapse-tags-tooltip :max-collapse-tags="19">
                    <el-option v-for="item in state.fileTypes" :key="item" :label="item" :value="item"></el-option>
                </el-select>
            </el-form-item>
        </el-form>
    </div>
</template>

<script>
import { reactive } from 'vue';
export default {
    props: ['item'],
    setup () {
        const state = reactive({
            fileTypes:[
                "txt", "md", "doc", "docx", "pdf", "rtf", "odt",
                "json", "xml", "csv", "yml", "yaml", "ini", "toml",
                "html", "htm", "css", "js", "py", "java", "cpp", "c", "h", "php",'cs',
                "jpg", "jpeg", "png", "gif", "svg", "bmp", "webp",
                "zip", "rar", "7z", "tar", "gz",
                "mp3", "mp4", "avi", "mov", "wav",
                "exe", "dmg", "iso", "log", "db", "sqlite", "ppt", "pptx", "xls", "xlsx"
            ],
        });
        return {
            state
        }
    }
}
</script>

<style lang="stylus" scoped>
.item{
    padding:1rem;
    border:1px solid #eee;
}
</style>