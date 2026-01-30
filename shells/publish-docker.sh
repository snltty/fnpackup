target=$(cd $(dirname $0)/..; pwd)
image="snltty/fnpackup"

fs=('fnpackup')
rs=('x64' 'arm64' 'arm')

cd fnpackup.web 
npm install &&
npm run build &&
cd ../

for f in ${fs[@]} 
do
    for r in ${rs[@]} 
    do
        rr=linux-${r}
        dotnet publish ${f} -c release -f net8.0 -o public/publish/docker/linux-${r}/${f} -r ${rr}  -p:PublishSingleFile=true  --self-contained true  -p:TrimMode=partial -p:TieredPGO=true  -p:DebugType=full -p:EventSourceSupport=false -p:DebugSymbols=true -p:EnableCompressionInSingleFile=true -p:DebuggerSupport=false -p:EnableUnsafeBinaryFormatterSerialization=false -p:EnableUnsafeUTF7Encoding=false -p:HttpActivityPropagationSupport=false -p:InvariantGlobalization=true  -p:MetadataUpdaterSupport=false  -p:UseSystemResourceKeys=true -p:MetricsSupport=false -p:StackTraceSupport=false -p:XmlResolverIsNetworkingEnabledByDefault=false
        cp -rf ${f}/Dockerfile public/publish/docker/linux-${r}/${f}/Dockerfile
        cp -rf public/extends/any/web public/publish/docker/linux-${r}/${f}/web
        mkdir -p public/publish/docker/linux-${r}/${f}/projects
    done
    cd public/publish/docker/linux-x64/${f}
    docker buildx build -f ${target}/public/publish/docker/linux-x64/${f}/Dockerfile --platform="linux/x86_64"  --force-rm -t "${image}-x64:latest" -t "${image}-x64:v1.0.5" . --push
    cd ../../../../../

    cd public/publish/docker/linux-arm64/${f}
    docker buildx build -f ${target}/public/publish/docker/linux-arm64/${f}/Dockerfile --platform="linux/arm64"  --force-rm -t "${image}-arm64:latest" -t "${image}-arm64:v1.0.5" . --push
    cd ../../../../../

    cd public/publish/docker/linux-arm/${f}
    docker buildx build -f ${target}/public/publish/docker/linux-arm/${f}/Dockerfile --platform="linux/arm/v7"  --force-rm -t "${image}-arm:latest" -t "${image}-arm:v1.0.5" . --push
    cd ../../../../../
	
done