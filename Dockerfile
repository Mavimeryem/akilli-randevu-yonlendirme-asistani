# Akıllı Randevu – üretim imajı (çok aşamalı derleme)
# Derleme: docker build -t akilli-randevu .
# Çalıştırma: docker run -p 8080:8080 -e DemoModu=true akilli-randevu

# 1) Derleme aşaması: SDK ile yayınla
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS derleme
WORKDIR /kaynak

# Önce yalnızca proje dosyaları: bağımlılık katmanı önbellekte kalır, kod değişince tekrar indirilmez.
COPY AkilliRandevu.sln ./
COPY src/AkilliRandevu.Entities/AkilliRandevu.Entities.csproj src/AkilliRandevu.Entities/
COPY src/AkilliRandevu.DataAccess/AkilliRandevu.DataAccess.csproj src/AkilliRandevu.DataAccess/
COPY src/AkilliRandevu.Business/AkilliRandevu.Business.csproj src/AkilliRandevu.Business/
COPY src/AkilliRandevu.Web/AkilliRandevu.Web.csproj src/AkilliRandevu.Web/
RUN dotnet restore src/AkilliRandevu.Web/AkilliRandevu.Web.csproj

COPY src/ src/
RUN dotnet publish src/AkilliRandevu.Web/AkilliRandevu.Web.csproj -c Release -o /yayin --no-restore

# 2) Çalışma aşaması: yalnızca ASP.NET çalışma ortamı (SDK yok, imaj küçük)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS calisma
WORKDIR /app

# Saat dilimi veritabanı: poliklinik saati (Europe/Istanbul) sunucunun UTC saatinden bağımsız hesaplanır.
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*

# Kalıcı veriler (SQLite veritabanı, oturum anahtarları) tek klasörde; root olmayan "app" kullanıcısına ait.
RUN mkdir -p /app/veri/anahtarlar && chown -R app:app /app/veri

COPY --from=derleme --chown=app:app /yayin .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Varsayilan="Data Source=/app/veri/akilli_randevu.db" \
    VeriKoruma__AnahtarKlasoru=/app/veri/anahtarlar \
    ProxyArkasinda=true

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "AkilliRandevu.Web.dll"]
