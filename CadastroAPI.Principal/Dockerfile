# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CadastroAPI.csproj", "CadastroAPI/"]
RUN dotnet restore "CadastroAPI/CadastroAPI.csproj"

WORKDIR "/src/CadastroAPI"
COPY . .

RUN dotnet build "CadastroAPI.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "CadastroAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CadastroAPI.dll"]
