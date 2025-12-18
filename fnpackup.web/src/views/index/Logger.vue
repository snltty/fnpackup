<template>
    <div class="logger-wrap scrollbar-4">
        <ul>
            <template v-for="item in logger.list">
                <li :class="`color-${item.type}`">
                    <pre>[{{dayjs(new Date()).format('YYYY-MM-DD HH:mm:ss')}}]:{{ item.msg }}</pre>
                </li>
            </template>
        </ul>
        
    </div>
</template>

<script>
import {useLogger} from './logger'
import dayjs from 'dayjs';
export default {
    setup () {
        const logger = useLogger()
        return {
            logger,dayjs
        }
    }
}
</script>

<style lang="stylus" scoped>
.logger-wrap{
    font-size:1.2rem;
    padding:1rem;

    .color-warn{color:orange;}
    .color-debug{color:blue;}
    .color-success{color:green;}
    .color-error{color:red;}
}
</style>