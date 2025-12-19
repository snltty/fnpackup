target=$(cd $(dirname $0)/..; pwd)
image="snltty/fnpackup"

fs=('fnpackup')
ps=('musl')
rs=('x64' 'arm64' 'arm')

cd fnpackup.web 
npm install &&
npm run build &&
cd ../

for f in ${fs[@]} 
do
	for p in ${ps[@]} 
	do
		for r in ${rs[@]} 
		do
            rr=linux-${p}-${r}
			dotnet publish ${f} -c release -f net8.0 -o public/publish/docker/linux-${p}-${r}/${f} -r ${rr}  -p:PublishSingleFile=true  --self-contained true  -p:TrimMode=partial -p:TieredPGO=true  -p:DebugType=full -p:EventSourceSupport=false -p:DebugSymbols=true -p:EnableCompressionInSingleFile=true -p:DebuggerSupport=false -p:EnableUnsafeBinaryFormatterSerialization=false -p:EnableUnsafeUTF7Encoding=false -p:HttpActivityPropagationSupport=false -p:InvariantGlobalization=true  -p:MetadataUpdaterSupport=false  -p:UseSystemResourceKeys=true -p:MetricsSupport=false -p:StackTraceSupport=false -p:XmlResolverIsNetworkingEnabledByDefault=false
			cp -rf ${f}/Dockerfile-${p} public/publish/docker/linux-${p}-${r}/${f}/Dockerfile-${p}
			cp -rf public/extends/any/web public/publish/docker/linux-${p}-${r}/${f}/web
            mkdir -p public/publish/docker/linux-${p}-${r}/${f}/projects
		done
		cd public/publish/docker/linux-${p}-x64/${f}
		docker buildx build -f ${target}/public/publish/docker/linux-${p}-x64/${f}/Dockerfile-${p} --platform="linux/x86_64"  --force-rm -t "${image}-${p}-x64:latest" -t "${image}-${p}-x64:v0.0.1" . --push
		cd ../../../../../

		cd public/publish/docker/linux-${p}-arm64/${f}
		docker buildx build -f ${target}/public/publish/docker/linux-${p}-arm64/${f}/Dockerfile-${p} --platform="linux/arm64"  --force-rm -t "${image}-${p}-arm64:latest" -t "${image}-${p}-arm64:v0.0.1" . --push
		cd ../../../../../

        cd public/publish/docker/linux-${p}-arm/${f}
		docker buildx build -f ${target}/public/publish/docker/linux-${p}-arm/${f}/Dockerfile-${p} --platform="linux/arm/v7"  --force-rm -t "${image}-${p}-arm:latest" -t "${image}-${p}-arm:v0.0.1" . --push
		cd ../../../../../
	done
done