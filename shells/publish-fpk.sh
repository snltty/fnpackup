target=$(cd $(dirname $0)/..; pwd)

mkdir -p public/publish-fpk/docker
cp -rf install-package/fpk/docker/* public/publish-fpk/docker

sed -i "s|{version}|1.0.4|g" public/publish-fpk/docker/manifest
sed -i "s|{changelog}|1. 一些累计更新，写了一些新bug、2. 日志收集，重定向到管道，可在在线fpk查看输出、3. 向导新增飞牛端口填写，以适应自定义端口|g" public/publish-fpk/docker/manifest
sed -i "s|{version}|v1.0.4|g" public/publish-fpk/docker/app/docker/docker-compose.yaml
sed -i 's/\r$//' public/publish-fpk/docker/manifest
sed -i 's/\r$//' public/publish-fpk/docker/cmd/main
sed -i 's/\r$//' public/publish-fpk/docker/cmd/uninstall_callback

cd public/publish-fpk/docker

tar -czf app.tgz --transform='s,app/,,g' app/docker app/www app/ui config
tar -czf fnpackup.fpk --exclude='app' *
mv fnpackup.fpk fnpackup-docker.fpk
