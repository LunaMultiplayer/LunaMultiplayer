FROM eclipse/dotnet_core
ENV LMP_URL https://luna-endpoint.glitch.me/latest
ENV LMP_REPO_UPDATE https://luna-endpoint.glitch.me/update
RUN sudo apt-get update
RUN sudo apt-get install -y tzdata mono-devel zip curl
RUN sudo curl -Ss $LMP_REPO_UPDATE
RUN sudo wget $LMP_URL
RUN sudo unzip latest
WORKDIR LMPServer
RUN sudo mkdir logs
EXPOSE 8800/udp
EXPOSE 8801/udp
CMD ["sudo","mono", "Server.exe"]
