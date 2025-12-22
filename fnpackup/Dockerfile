FROM debian:latest

RUN ln -snf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime && echo Asia/Shanghai > /etc/timezone

EXPOSE 1069/tcp

WORKDIR /app
COPY . .

ENV DOTNET_ThreadPool_UnfairSemaphoreSpinLimit=0

ENTRYPOINT ["./fnpackup"]