target=$(cd $(dirname $0)/..; pwd)

rs=('x86_64' 'arm64' 'arm')
for r in ${rs[@]} 
do

    mkdir -p public/publish-fpk/docker/${r}
    cp -rf install-package/fpk/docker/* public/publish-fpk/docker/${r}

    sed -i "s|{version}|0.0.1|g" public/publish-fpk/docker/${r}/manifest
    sed -i "s|{arch}|${r}|g" public/publish-fpk/docker/${r}/manifest
    sed -i 's/\r$//' public/publish-fpk/docker/${r}/manifest
    sed -i 's/\r$//' public/publish-fpk/docker/${r}/cmd/main
    sed -i 's/\r$//' public/publish-fpk/docker/${r}/cmd/uninstall_callback

    cd public/publish-fpk/docker/${r}

    tar -czf app.tgz --transform='s,app/,,g' app/docker app/ui config
    tar -czf fnpackup.fpk --exclude='app' *
    mv fnpackup.fpk fnpackup-docker-${r}.fpk

    cd ../../../../
done
