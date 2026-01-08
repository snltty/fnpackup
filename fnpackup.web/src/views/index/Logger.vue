<template>
    <div class="logger-wrap h-100" >
        <div class="split">
            <el-icon><Minus /></el-icon>
        </div>
        <div class="inner scrollbar h-100" ref="wrap">
            <ul class="ul" ref="ul">
                <template v-for="item in logger.list">
                    <li :class="`color-${item.type}`">
                        <pre>[{{item.time}}]:{{ item.msg }}</pre>
                    </li>
                </template>
            </ul>
        </div>
    </div>
</template>

<script>
import { nextTick, ref, watch } from 'vue';
import {useLogger} from './logger'
import { Minus } from '@element-plus/icons-vue';

export default {
    components: {Minus},
    setup () {
        const logger = useLogger();
        const wrap = ref(null);
        const ul = ref(null);

        watch(() => logger.value.version, (val) => {
           
            if(wrap.value){
                nextTick(()=>{
                    wrap.value.scrollTop =  (ul.value.offsetHeight + 20);
                })
            }
        });
       
        return {
            logger,wrap,ul
        }
    }
}
</script>

<style lang="stylus" scoped>
html.dark .logger-wrap{
    .inner{
        border-color:#39434c;
    }
}
.split{
    position:absolute;
    left:50%;
    top:-7px;
    color:#666;
}
.logger-wrap{
    font-size:1.2rem;
    padding:.5rem 1rem 1rem 1rem;
    box-sizing: border-box;
    position:relative;

    .color-warn{color:orange;}
    .color-debug{color:blue;}
    .color-success{color:green;}
    .color-error{color:red;}

    .inner{
        border:1px solid #e2e8f0e6;
        border-radius: 5px;
        padding:1rem;
        box-sizing: border-box;
    }
}
</style>