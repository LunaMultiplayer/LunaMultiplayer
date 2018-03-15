FROM eclipse/dotnet_core
ENV DIRPATH $LUNA_HOME
ENV LUNA_HOME LMPServer
ENV LMP_URL https://github.com/LunaMultiplayer/LunaMultiplayerUpdater/releases/download/1.0.0/LunaMultiplayerUpdater-Release.zip
ENV LMP_EXE Server.exe
ENV LMP_UPDATER ServerUpdater.exe
WORKDIR $LUNA_HOME
RUN apt-get update && apt-get install zip
RUN sudo wget $LMP_URL \
&& sudo unzip LunaMultiplayerUpdater-Release.zip \
&& sudo touch Server.exe && sudo mkdir -p $LUNA_HOME/logs  
COPY . /${LUNA_HOME}
VOLUME /${LUNA_HOME}
EXPOSE 8800/udp
EXPOSE 8801/udp
RUN sudo mono $LMP_UPDATER
CMD ["mono", "${LMP_EXE}"]
