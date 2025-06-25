# ── 1) Build stage ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY TaskManagement.sln .
COPY ManagementSystem.WSAPI/*.csproj ManagementSystem.WSAPI/
# (repeat for any other projects WSAPI depends on)

# Restore and publish only WSAPI
RUN dotnet restore "ManagementSystem.WSAPI/ManagementSystem.WSAPI.csproj"
COPY . .
RUN dotnet publish "ManagementSystem.WSAPI/ManagementSystem.WSAPI.csproj" \
    -c Release \
    -o /app/publish

# ── 2) Runtime stage ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port 80 and start the app
EXPOSE 9000
ENTRYPOINT ["dotnet", "ManagementSystem.WSAPI.dll"]
