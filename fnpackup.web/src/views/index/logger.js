import { inject, provide, ref } from "vue";

const loggerSymbol = Symbol();
export const provideLogger = () => {
    const logger = ref({
        list:[{type:'info',msg:'操作日志显示'}],
        info(msg){
            this.list.push({type:'info',msg:msg});
        },
        warn(msg){
            this.list.push({type:'warn',msg:msg});
        },
        debug(msg){
            this.list.push({type:'debug',msg:msg});
        },
        error(msg){
            this.list.push({type:'error',msg:msg});
        },
        success(msg){
            this.list.push({type:'success',msg:msg});
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