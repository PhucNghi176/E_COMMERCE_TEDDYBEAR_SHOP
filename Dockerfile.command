FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env

# Set environment variables to handle timeouts and improve performance
ENV DOTNET_CLI_TIMEOUT=300000
ENV NUGET_XMLDOC_MODE=skip
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV NUGET_PACKAGES=/root/.nuget/packages

WORKDIR /src

# Copy nuget.config if you have one (recommended to create)
# COPY nuget.config ./

# Copy solution file
COPY *.sln ./

# Copy COMMAND project files
COPY COMMAND/COMMAND.API/*.csproj COMMAND/COMMAND.API/
COPY COMMAND/COMMAND.APPLICATION/*.csproj COMMAND/COMMAND.APPLICATION/
COPY COMMAND/COMMAND.APPLICATION.TEST/*.csproj COMMAND/COMMAND.APPLICATION.TEST/
COPY COMMAND/COMMAND.INFRASTRUCTURE/*.csproj COMMAND/COMMAND.INFRASTRUCTURE/
COPY COMMAND/COMMAND.PERSISTENCE/*.csproj COMMAND/COMMAND.PERSISTENCE/
COPY COMMAND/COMMAND.PRESENTATION/*.csproj COMMAND/COMMAND.PRESENTATION/
COPY COMMAND/COMMAND.CONTRACT/*.csproj COMMAND/COMMAND.CONTRACT/

# Copy the shared CONTRACT project (now accessible)
COPY CONTRACT/CONTRACT/*.csproj CONTRACT/CONTRACT/

# Restore packages with retry logic and timeout settings
RUN for i in 1 2 3; do \
    dotnet restore COMMAND/COMMAND.API/COMMAND.API.csproj \
        --disable-parallel \
        --no-cache \
        --force \
        --verbosity normal && break || \
    (echo "Restore attempt $i failed, retrying in 10 seconds..." && sleep 10); \
    done && \
    if [ $? -ne 0 ]; then \
        echo "All restore attempts failed" && exit 1; \
    fi

# Copy all source code
COPY . ./

# Build and publish the COMMAND API
WORKDIR /src/COMMAND/COMMAND.API
RUN dotnet publish -c Release -o /app/out --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "COMMAND.API.dll"]
