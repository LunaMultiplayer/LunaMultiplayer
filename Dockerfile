FROM alpine:3.9

ENV LMP_URL https://luna-endpoint.glitch.me/latest

RUN apk add --no-cache mono --repository http://dl-cdn.alpinelinux.org/alpine/edge/testing && \
    wget $LMP_URL && \
    unzip latest && \
    rm -rf latest LMPClient LMP\ Readme.txt

WORKDIR LMPServer

EXPOSE 8800/udp
EXPOSE 8801/udp

VOLUME ["Universe", "Config", "Plugins"]

CMD ["mono", "Server.exe"]