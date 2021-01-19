FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
LABEL stage=intermediate-rknlist
WORKDIR /build
COPY . .
RUN dotnet publish RknList/RknList.csproj -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app .

#Application logs
VOLUME /app/log

#Webface port number
EXPOSE 8080

ENTRYPOINT ["dotnet", "RknList.dll", "8080"]