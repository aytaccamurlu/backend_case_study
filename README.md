# 🎟️ İki Aşamalı Rezervasyon ve Kapasite Yönetim Sistemi

Bu proje; etkinlik platformları için kritik öneme sahip olan **"Hold & Confirm"** (Ön Rezervasyon ve Onay) akışını yöneten, yüksek eşzamanlılık (concurrency) altında veri tutarlılığını garanti eden bir backend sistemidir.

## 🚀 Mimari ve Teknolojiler

- **Backend:** .NET 8 / 9
- **Database:** MongoDB (Atlas) - *Atomic Operations & High Availability*
- **Background Jobs:** Hangfire - *Otomatik Kapasite İadesi & Süre Yönetimi*
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, API)
- **Eşzamanlılık Yönetimi:** MongoDB `FindOneAndUpdate` (Atomic Update) ile Race Condition önlenmiştir.

## 🛠️ İşleyiş Mantığı

### 1. Aşama: HOLD (Geçici Ayırma)
- Kullanıcı rezervasyon isteği gönderdiğinde sistem **atomik** bir sorgu ile kapasiteyi kontrol eder.
- Kapasite uygunsa, tek bir işlemde kapasite `-1` düşürülür ve kullanıcıya 5 dakikalık bir "HOLD" kaydı oluşturulur.
- Eşzamanlı gelen 1000 istek olsa bile, veritabanı seviyesindeki kilit mekanizması sayesinde kapasite asla eksiye düşmez.

### 2. Aşama: CONFIRM (Onaylama)
- Kullanıcı 5 dakika içerisinde onay verirse, rezervasyon durumu `Confirmed` olarak güncellenir.
- Kapasite zaten HOLD aşamasında düşürüldüğü için ek bir işlem gerekmez.

### 3. Aşama: Otomatik İptal (Hangfire)
- Rezervasyon oluşturulduğu an, arka planda 5 dakika sonrasına bir **Background Job** planlanır.
- 5 dakika sonunda rezervasyon hala `Hold` durumundaysa; sistem rezervasyonu `Expired` yapar ve ilgili etkinliğin kapasitesini `+1` artırarak geri iade eder.



## 📂 Proje Yapısı

- **Domain:** Entity'ler, Enum'lar ve temel iş kuralları.
- **Application:** DTO'lar ve servis arayüzleri.
- **Infrastructure:** MongoDB Context, Hangfire servisleri ve veritabanı implementasyonları.
- **API:** Controller'lar, Swagger ve JWT entegrasyonu.

## 🚦 Başlangıç

1. **Bağımlılıkları Yükleyin:**
   ```bash
   dotnet restore
