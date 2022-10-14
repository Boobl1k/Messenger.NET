FROM mcr.microsoft.com/dotnet/sdk:6.0 AS dev
EXPOSE 5001
EXPOSE 5002
WORKDIR /app
RUN dotnet dev-certs https
ENTRYPOINT ["dotnet", "watch", "run", "--urls=http://+:81;https://+:5002"]
