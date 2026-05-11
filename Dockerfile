FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /package

COPY ./src/DbEye/DbEye.csproj ./src/DbEye/DbEye.csproj
COPY ./src/DbEye.Demo/DbEye.Demo.csproj ./src/DbEye.Demo/DbEye.Demo.csproj

RUN dotnet restore ./src/DbEye.Demo/DbEye.Demo.csproj
COPY . .

RUN dotnet publish ./src/DbEye.Demo/DbEye.Demo.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /package
COPY --from=build /app .

ARG UID=10001
RUN useradd --uid ${UID} --no-create-home appuser
USER appuser

ENTRYPOINT ["dotnet", "DbEye.Demo.dll"]