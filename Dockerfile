# Use the ASP.NET 8.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the already-published output
COPY publish/CICD/ ./

# Bind Kestrel to port 80
ENV ASPNETCORE_URLS=http://+:9000
EXPOSE 9000

ENTRYPOINT ["dotnet", "ManagementSystem.WSAPI.dll"]