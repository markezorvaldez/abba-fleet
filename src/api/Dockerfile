FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app
COPY . .
RUN dotnet restore
EXPOSE 8080
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://+:8080", "--no-launch-profile"]
