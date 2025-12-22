
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
fnpack二次包装UI，在线编辑和打包fpk

## [😂]使用方法

1. 在飞牛系统`应用中心`找到应用安装，所有创建的项目文件都在`应用文件夹->fnpackup-docker->projects`下
2. 下载`fnpackup-docker-x64.fpk`到飞牛系统安装，目录与应用中心安装的一样
3. 或使用`snltty/fnpackup`Dcker镜像运行

```
docker run -it -d --name fnpackup \
-p 1069:1069 \
-v /usr/local/fnpackup-docker/projects:/app/projects \
--restart=always \
--privileged=true \
snltty/fnpackup
```
```
docker run -it -d --name fnpackup \
--network host \
-v /usr/local/fnpackup-docker/projects:/app/projects \
--restart=always \
--privileged=true \
snltty/fnpackup
```

## [🎁]为爱发电

若此项目对您有用，可以考虑对作者稍加支持，让作者更有动力，在项目上投入更多时间和精力


![pay](./fnpackup.web/public/pay.png)

## [👏]特别说明

[![Contributors](https://contrib.rocks/image?repo=snltty/fnpackup&columns=8)](https://github.com/snltty/fnpackup/graphs/contributors)

[![Star History Chart](https://api.star-history.com/svg?repos=snltty/fnpackup&type=Date)](https://www.star-history.com/#snltty/fnpackup&Date)


