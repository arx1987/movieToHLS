FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /source
COPY . .
RUN dotnet publish -c Release -o /app


FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 80
ENTRYPOINT [ "dotnet", "MovieToHLS.dll" ]