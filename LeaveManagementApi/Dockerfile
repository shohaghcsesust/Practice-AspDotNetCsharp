FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["LeaveManagementApi.csproj", "./"]
RUN dotnet restore "LeaveManagementApi.csproj"
COPY . .
RUN dotnet build "LeaveManagementApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LeaveManagementApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory for file attachments
RUN mkdir -p /app/uploads/attachments

ENTRYPOINT ["dotnet", "LeaveManagementApi.dll"]
