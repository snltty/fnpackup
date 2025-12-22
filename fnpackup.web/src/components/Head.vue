<template>
    <div class="head-wrap">
        <div class="flex">
            <div class="logo">
                <img src="../assets/logo.png" alt="">
                <span>在线打包FPK</span>
            </div>
            <span class="flex-1"></span>
            <span class="github">
                <a href="https://github.com/snltty/fnpackup" target="_blank">
                    <img src="../assets/github.svg" alt="" height="20">
                </a>
            </span>
        </div>
    </div>
</template>

<script>
export default {
    setup () {
        

        return {}
    }
}
</script>

<style lang="stylus" scoped>
.head-wrap{
    background-color: #f6f8fa;
    border-bottom: 1px solid #d0d7de;
    height: 5rem;
    line-height: 5rem;

    .logo{
        padding-left:1rem;
        font-size: 0;
        img{
            height:4rem;
            vertical-align: middle;
            margin-right: .5rem;
            border:1px solid #45627b;
            border-radius: 10px;
        }
        span{font-size:2rem;vertical-align: middle;color:#45627b;}
    }
    .github{
        padding-right:1rem;
        img{
            vertical-align: middle;
        }
    }
}
</style>