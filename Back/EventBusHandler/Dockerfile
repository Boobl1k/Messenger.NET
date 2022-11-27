#FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#
#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
#WORKDIR /src
#COPY ["EventBusHandler/EventBusHandler.csproj", "EventBusHandler/"]
#COPY ["Back/Domain/Domain.csproj", "Domain/"]
#RUN dotnet restore "EventBusHandler/EventBusHandler.csproj"
#COPY . .
#WORKDIR "/src/EventBusHandler"
#RUN dotnet build "EventBusHandler.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "EventBusHandler.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "EventBusHandler.dll"]

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS dev
WORKDIR /app
RUN dotnet dev-certs https
ENTRYPOINT ["dotnet", "watch", "run", "--urls=http://+:82;https://+:5003"]
