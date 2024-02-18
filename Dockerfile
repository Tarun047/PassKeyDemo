FROM node:latest AS frotend
WORKDIR /frontend
COPY ./PassKeys.WebApp/Frontend .
RUN npm install
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY PassKeys.sln .
COPY ./PassKeys.Business/PassKeys.Business.csproj ./PassKeys.Business/PassKeys.Business.csproj
COPY ./PassKeys.WebApp/PassKeys.WebApp.csproj ./PassKeys.WebApp/PassKeys.WebApp.csproj
RUN dotnet restore ./PassKeys.Business/PassKeys.Business.csproj
RUN dotnet restore ./PassKeys.WebApp/PassKeys.WebApp.csproj

COPY ./PassKeys.Business ./PassKeys.Business
COPY ./PassKeys.WebApp ./PassKeys.WebApp
WORKDIR /source/PassKeys.WebApp
RUN dotnet publish -c release -o /publish
COPY --from=frotend /frontend/dist /publish/wwwroot

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /publish ./
ENV ASPNETCORE_ENVIRONMENT=DockerDev
ENV ASPNETCORE_URLS=http://+:5001
EXPOSE 5001
ENTRYPOINT ["dotnet", "PassKeys.WebApp.dll"]


