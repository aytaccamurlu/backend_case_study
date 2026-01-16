# 1. Build Aşaması (SDK imajı kullanılır)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala (Katmanlı mimari olduğu için hepsini kopyalıyoruz)
COPY ["ReservationSystem.API/ReservationSystem.API.csproj", "ReservationSystem.API/"]
COPY ["ReservationSystem.Domain/ReservationSystem.Domain.csproj", "ReservationSystem.Domain/"]
COPY ["ReservationSystem.Application/ReservationSystem.Application.csproj", "ReservationSystem.Application/"]
COPY ["ReservationSystem.Infrastructure/ReservationSystem.Infrastructure.csproj", "ReservationSystem.Infrastructure/"]

# Bağımlılıkları geri yükle
RUN dotnet restore "ReservationSystem.API/ReservationSystem.API.csproj"

# Tüm kaynak kodları kopyala
COPY . .

# Projeyi derle ve yayınla (Publish)
WORKDIR "/src/ReservationSystem.API"
RUN dotnet publish "ReservationSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Çalıştırma Aşaması (Runtime imajı kullanılır)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "ReservationSystem.API.dll"]
