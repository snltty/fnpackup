import dayjs from 'dayjs';
import { inject, provide, ref } from "vue";

const loggerSymbol = Symbol();
export const provideLogger = () => {
    const logger = ref({
        version:0,
        list:[{type:'info',msg:'操作日志显示',time:dayjs(new Date()).format('YYYY-MM-DD HH:mm:ss')}],
        info(msg){
            this.list.push({type:'info',msg:msg,time:dayjs(new Date()).format('YYYY-MM-DD HH:mm:ss')});
            this.version++;
        },
        warn(msg){
            this.list.push({type:'warn',msg:msg,time:dayjs(new Date()).format('YYYY-MM-DD HH:mm:ss')});
            this.version++;
        },
        debug(msg){
            this.list.push({type:'debug',msg:msg,time:dayjs(new Date()).format('YYYY-MM-DD HH:mm:ss')});
            this.version++;
        },
        error(msg){
            this.list.push({type:'error',msg:msg,time:dayjs(new Date()).format('YYYY-MM-DD HH:mm:ss')});
            this.version++;
        },
        success(msg){
            this.list.push({type:'success',msg:msg,time:dayjs(new Date()).format('YYYY-MM-DD HH:mm:ss')});
            this.version++;
        },
        clear(){
            this.list = [];
        }
    });
    provide(loggerSymbol, logger);
    return {
        logger
    }
}
export const useLogger = () => {
    return inject(loggerSymbol);
}