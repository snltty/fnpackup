target=$(cd $(dirname $0)/..; pwd)

mkdir -p public/publish-fpk/docker
cp -rf install-package/fpk/docker/* public/publish-fpk/docker

sed -i "s|{version}|1.0.0|g" public/publish-fpk/docker/manifest
sed -i "s|{changelog}|1. 一些累计更新，写了一些新bug、2. 一些bug优化、3. 颜色主题适应、4. 依赖选择，查询应用中心应用列表|g" public/publish-fpk/docker/manifest
sed -i "s|{version}|v1.0.0|g" public/publish-fpk/docker/app/docker/docker-compose.yaml
sed -i 's/\r$//' public/publish-fpk/docker/manifest
sed -i 's/\r$//' public/publish-fpk/docker/cmd/main
sed -i 's/\r$//' public/publish-fpk/docker/cmd/uninstall_callback

cd public/publish-fpk/docker

tar -czf app.tgz --transform='s,app/,,g' app/docker app/www app/ui config
tar -czf fnpackup.fpk --exclude='app' *
mv fnpackup.fpk fnpackup-docker.fpk
