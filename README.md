
<div align="center">
<p><img src="./fnpackup.web/public/logo.png" height="240"></p> 

# Fnpackup
 

[![Stars](https://img.shields.io/github/stars/snltty/fnpackup?style=for-the-badge)](https://github.com/snltty/fnpackup)
[![Forks](https://img.shields.io/github/forks/snltty/fnpackup?style=for-the-badge)](https://github.com/snltty/fnpackup)
[![Docker Pulls](https://img.shields.io/docker/pulls/snltty/fnpackup?style=for-the-badge)](https://hub.docker.com/r/snltty/fnpackup)

[![Release](https://img.shields.io/github/v/release/snltty/fnpackup?sort=semver&style=for-the-badge)](https://github.com/snltty/fnpackup/releases)
[![License](https://img.shields.io/github/license/snltty/fnpackup?style=for-the-badge)](https://mit-license.org/)
[![Language](https://img.shields.io/github/languages/top/snltty/fnpackup?style=for-the-badge)](https://github.com/snltty/fnpackup)
[![GitHub Downloads](https://img.shields.io/github/downloads/snltty/fnpackup/total?style=for-the-badge)](https://github.com/snltty/fnpackup)


<a href="https://jq.qq.com/?_wv=1027&k=ucoIVfz4" target="_blank">加入组织：1121552990</a>

</div>

## [🪂]Fnpackup
fnpack二次包装UI，在线编辑和打包fpk，和帮助fpk自动托管静态资源

## [😂]安装方法

1. 在飞牛系统`应用中心`找到应用安装
2. 下载`fnpackup-docker-x64.fpk`到飞牛系统安装
3. 或使用`snltty/fnpackup`Dcker镜像运行，依赖fnpack，监听1069/tcp端口
```
docker run -it -d --name fnpackup \
--network host \
-v /usr/local/fnpackup-docker/projects:/app/projects \
-v /usr/local/fnpackup-docker/statics:/app/statics \
-v /usr/local/bin/appcenter-cli:/app/appcenter-cli:ro \
-v /usr/local/bin/fnpack:/app/fnpack:ro \
-v /var/apps:/app/apps:ro \
--restart=always \
--privileged=true \
snltty/fnpackup
```

## [📦]使用方法

### [💼]打包fpk
1. 打开`在线fpk`选择`打包fpk`菜单，可以创建项目或导入已有的.fpk文件
2. 双击项目文件夹进入项目
3. 如果是原生app，可以先上传你的程序到`app/server`中，docker项目直接编辑`app/docker/docker-compose.yaml`文件即可
4. 可以使用`快速编辑`，包含了所需编辑的内容，一一编辑即可，也可以右键文件进行源码编辑，也可以右键上传所需文件
5. 编辑好全部内容后打包fpk，在项目文件夹下查看生成的fpk文件

### [🌍]静态托管

如果你有一些静态网页，比如纯静态内容的fpk，啊！专门写一个http服务，或者写个cgi实在太麻烦了，那么可以使用本程序进行托管

两种配置方式

1. 是fpk里的静态内容，打包fpk时，manifest里填一个`fnpackup={目录}`，表示托管`app/{目录}`，也可以是`fnpackup={目录}/{下级目录}`，表示托管`app/{目录}/{下级目录}`
2. 就纯静态网页，安装fnpackup后，在`文件管理/应用文件/fnpackup-docker/statics/`下新建一个文件夹，将静态内容放入，文件夹名称就是`{appname}`

然后使用`http://{appname}.domain.com:1069`或`http://ip:1069/{appname}`访问


## [🖼️]预览效果

![pay](./fnpackup.web/public/fnpackup.png)

## [🎁]为爱发电

若此项目对您有用，可以考虑对作者稍加支持，让作者更有动力，在项目上投入更多时间和精力


![pay](./fnpackup.web/public/pay.png)

## [👏]特别说明

[![Contributors](https://contrib.rocks/image?repo=snltty/fnpackup&columns=8)](https://github.com/snltty/fnpackup/graphs/contributors)

[![Star History Chart](https://api.star-history.com/svg?repos=snltty/fnpackup&type=Date)](https://www.star-history.com/#snltty/fnpackup&Date)


