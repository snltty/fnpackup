<template>
    <div class="logger-wrap scrollbar h-100" ref="wrap">
        <ul class="ul" ref="ul">
            <template v-for="item in logger.list">
                <li :class="`color-${item.type}`">
                    <pre>[{{item.time}}]:{{ item.msg }}</pre>
                </li>
            </template>
        </ul>
    </div>
</template>

<script>
import { nextTick, ref, watch } from 'vue';
import {useLogger} from './logger'

export default {
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
.logger-wrap{
    font-size:1.2rem;
    padding:1rem;
    box-sizing: border-box;
    position:relative;

    .color-warn{color:orange;}
    .color-debug{color:blue;}
    .color-success{color:green;}
    .color-error{color:red;}
}
</style>