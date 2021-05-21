# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app

# Copy build files and set entry point
COPY /out .
ENTRYPOINT ["dotnet", "./GV.SCS.Store.FridgeStore.dll"]
