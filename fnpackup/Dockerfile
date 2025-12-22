FROM alpine:latest

ENV TZ=Asia/Shanghai DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN echo "https://mirrors.ustc.edu.cn/alpine/latest-stable/main/" > /etc/apk/repositories \
    && apk add --no-cache libstdc++ libintl tzdata bash \
    && ln -snf /usr/share/zoneinfo/$clTZ /etc/localtime \
    && echo $TZ > /etc/timezone

WORKDIR /app
COPY . .

ENV DOTNET_ThreadPool_UnfairSemaphoreSpinLimit=0

ENTRYPOINT ["./fnpackup"]