FROM eclipse/dotnet_core

ENV LMP_URL https://luna-endpoint.glitch.me/latest
ENV LMP_REPO_UPDATE https://luna-endpoint.glitch.me/update

RUN set -ex \
&& sudo apt-get update \
&& sudo apt-get install -y \
    tzdata \
    mono-devel \
    zip \
    curl \
&& sudo curl -Ss $LMP_REPO_UPDATE \
&& sudo wget $LMP_URL \
&& sudo unzip latest \
&& sudo rm -rf /var/lib/apt/lists/*

WORKDIR LMPServer

EXPOSE 8800/udp
EXPOSE 8801/udp

VOLUME ["Universe", "Config", "Plugins"]

CMD ["sudo" ,"mono", "Server.exe"]
