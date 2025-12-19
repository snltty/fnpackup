target=$(cd $(dirname $0)/..; pwd)


cd fnpackup.web 
npm install &&
npm run build &&
cd ../

rs=('x64')
for r in ${rs[@]} 
do

    mkdir -p public/publish-fpk/docker/${r}
    cp -rf install-package/fpk/docker/* public/publish-fpk/docker/${r}

    sed -i "s|{version}|{{version}}|g" public/publish-fpk/docker/${r}/manifest
    sed -i 's/\r$//' public/publish-fpk/docker/${r}/manifest
    sed -i 's/\r$//' public/publish-fpk/docker/${r}/cmd/main
    sed -i 's/\r$//' public/publish-fpk/docker/${r}/cmd/uninstall_callback

    cd public/publish-fpk/docker/${r}

    tar -czf app.tgz --transform='s,app/,,g' app/docker app/ui config
    tar -czf fnpackup.fpk --exclude='app' *
    mv fnpackup.fpk fnpackup-docker-x64.fpk

    cd ../../../../
done
