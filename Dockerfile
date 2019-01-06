FROM microsoft/dotnet:2.2-sdk
COPY ./Service/bin/Release/netcoreapp2.2/publish/ /app
EXPOSE 80
ENTRYPOINT ["dotnet", "/app/Service.dll"]