FROM eclipse/dotnet_core
ENV DIRPATH $LUNA_HOME
ENV LUNA_HOME LMPServer
ENV LMP_URL https://github.com/4669842/Docker-LunaMultiplayer/releases/download/0.0.0.1/Updater.tar.gz
ENV LMP_EXE Server.exe
ENV LMP_UPDATER ServerUpdater.exe
WORKDIR $LUNA_HOME
RUN sudo wget $LMP_URL \
&& sudo tar -xvzf Updater.tar.gz \
&& sudo touch Server.exe && sudo mkdir -p $LUNA_HOME/logs  
COPY . /${LUNA_HOME}
VOLUME /${LUNA_HOME}
EXPOSE 8800/udp
EXPOSE 8801/udp
RUN sudo mono $LMP_UPDATER
CMD ["mono", "${LMP_EXE}"]
