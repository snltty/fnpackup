target=$(cd $(dirname $0)/..; pwd)

mkdir -p public/publish-fpk/docker
cp -rf install-package/fpk/docker/* public/publish-fpk/docker

sed -i "s|{version}|0.0.8|g" public/publish-fpk/docker/manifest
sed -i "s|{changelog}|1. 一些累计更新，一些BUG修复、2. 一些bug修复，目录跳转什么的、3. 一键打包多平台|g" public/publish-fpk/docker/manifest
sed -i "s|{version}|v0.0.8|g" public/publish-fpk/docker/app/docker/docker-compose.yaml
sed -i 's/\r$//' public/publish-fpk/docker/manifest
sed -i 's/\r$//' public/publish-fpk/docker/cmd/main
sed -i 's/\r$//' public/publish-fpk/docker/cmd/uninstall_callback

cd public/publish-fpk/docker

tar -czf app.tgz --transform='s,app/,,g' app/docker app/ui config
tar -czf fnpackup.fpk --exclude='app' *
mv fnpackup.fpk fnpackup-docker.fpk
