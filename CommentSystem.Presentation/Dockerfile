FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
 
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
 
COPY . .
 
RUN dotnet restore CommentSystem.Presentation/CommentSystem.Presentation.csproj
 
WORKDIR /src/CommentSystem.Presentation
RUN dotnet publish CommentSystem.Presentation.csproj -c Release -o /app/publish --no-restore
 
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommentSystem.Presentation.dll"]