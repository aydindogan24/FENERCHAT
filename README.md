# 💛💙 FenerChat - Fenerbahçe Temalı Chat Uygulaması 💙💛

**Geliştirici:** Berkay Sabuncu  
**Öğrenci No:** 240542029  
**Bölüm:** Teknoloji Fakültesi - Yazılım Mühendisliği  
**Sınıf:** 2/A  

---

## 📋 Proje Hakkında

**FenerChat**, **C# Windows Forms** teknolojisi kullanılarak geliştirilmiş gerçek zamanlı chat uygulamasıdır. Uygulama, **TCP/IP socket** protokolleri ile aynı ağdaki bilgisayarlar arasında mesajlaşma imkanı sağlar. 

### 🎨 Özel Özellikler
- **Fenerbahçe Teması**: Sarı-Lacivert renklerle özel tasarım
- **Renkli Mesajlaşma**: Mesaj tiplerine göre renklendirme
- **Modern UI**: Flat design butonlar ve hover efektleri

## 🛠️ Kullanılan Teknolojiler

### **Backend Teknolojileri**
- **C# (.NET Framework 4.7.2)**
- **TCP/IP Socket Programming**
- **UDP Broadcast Communication**
- **Asynchronous Programming (async/await)**
- **Multi-threading**

### **Frontend Teknolojileri**
- **Windows Forms (.NET Framework)**
- **RichTextBox** (mesaj görüntüleme)
- **TextBox** (mesaj girişi)
- **Button Controls** (etkileşim)
- **Label Controls** (durum gösterimi)

### **Ağ Protokolleri**
- **TCP (Port 55555)**: Ana mesajlaşma protokolü
- **UDP (Port 49876)**: Sunucu keşif protokolü

## 🏗️ Mimari ve Çalışma Prensibi

### **1. Otomatik Sunucu Keşfi**
```
UDP Broadcast → Sunucu Arama → TCP Bağlantı
```

### **2. Hibrit Sunucu/İstemci Modeli**
- Uygulama başlatıldığında önce sunucu arar
- Sunucu bulunamazsa otomatik sunucu moduna geçer
- Sunucu bulunursa istemci olarak bağlanır

### **3. Mesajlaşma Protokolü**
```
İstemci → TCP Socket → Sunucu → Broadcast → Tüm İstemciler
```

## 📁 Proje Yapısı

```
csharp-winforms-chat/
├── ChatApp/
│   ├── MainForm.cs              # Ana form ve UI mantığı
│   ├── MainForm.Designer.cs     # UI tasarımı
│   ├── Program.cs               # Uygulama giriş noktası
│   ├── Networking/
│   │   ├── TcpChatServer.cs     # TCP sunucu implementasyonu
│   │   ├── TcpChatClient.cs     # TCP istemci implementasyonu
│   │   └── DiscoveryService.cs  # UDP sunucu keşif servisi
│   └── Properties/
│       └── AssemblyInfo.cs       # Assembly bilgileri
├── ChatApp.csproj               # Proje dosyası
└── README.md                    # Bu dosya
```

## ⚡ Özellikler

### **🔍 Akıllı Sunucu Keşfi**
- UDP broadcast ile otomatik sunucu bulma
- Local subnet broadcast desteği
- Timeout korumalı keşif sistemi

### **🔗 Manuel Bağlantı**
- IP adresi ile manuel bağlantı seçeneği
- Otomatik keşif başarısız olduğunda alternatif
- Kullanıcı dostu IP girişi

### **💬 Çift Yönlü Mesajlaşma**
- Gerçek zamanlı mesaj gönderme/alma
- Çoklu istemci desteği
- Mesaj geçmişi korunması

### **🖥️ Otomatik Sunucu Modu**
- Sunucu bulunamazsa otomatik sunucu olma
- Dinamik sunucu/istemci geçişi
- IP adresi otomatik gösterimi

### **🐛 Gelişmiş Debug Sistemi**
- Console ve UI debug mesajları
- Detaylı hata yakalama
- Bağlantı durumu takibi

## 🚀 Kurulum ve Çalıştırma

### **Gereksinimler**
- Windows 7/8/10/11
- .NET Framework 4.7.2 veya üzeri
- Visual Studio 2017+ (geliştirme için)

### **Derleme**
```bash
# Visual Studio ile
1. csharp-winforms-chat.sln dosyasını açın
2. Build → Rebuild Solution

# Command Line ile
dotnet build ChatApp/ChatApp.csproj
```

### **Çalıştırma**
```bash
# Debug modunda
.\ChatApp\bin\Debug\FenerChat.exe

# Release modunda
.\ChatApp\bin\Release\FenerChat.exe
```

## 📖 Kullanım Kılavuzu

### **1. Sunucu Başlatma**
1. Uygulamayı çalıştırın
2. Otomatik olarak sunucu moduna geçer
3. IP adresleri ekranda görüntülenir
4. Durum: "Sunucu (dinlemede)" olur

### **2. İstemci Bağlantısı**
1. Başka bilgisayarda uygulamayı çalıştırın
2. Otomatik sunucu keşfi çalışır
3. Bağlantı başarılı olursa: "Bağlı (IP)" durumu
4. Başarısız olursa: "Manuel Bağlan" butonunu kullanın

### **3. Mesajlaşma**
- Mesaj yazın ve Enter'a basın veya "Gönder" butonuna tıklayın
- Mesajlar tüm bağlı kullanıcılara gönderilir
- Mesaj geçmişi RichTextBox'ta görüntülenir

## 🔧 Teknik Detaylar

### **TCP Sunucu (TcpChatServer.cs)**
- Çoklu istemci desteği
- ConcurrentDictionary ile thread-safe istemci yönetimi
- Async/await ile non-blocking I/O
- Otomatik istemci bağlantı/ayrılma yönetimi

### **TCP İstemci (TcpChatClient.cs)**
- Async bağlantı ve mesaj gönderme
- Satır bazlı mesaj okuma
- Otomatik yeniden bağlanma desteği
- Graceful disconnect handling

### **UDP Keşif Servisi (DiscoveryService.cs)**
- Broadcast ve unicast desteği
- Timeout korumalı keşif
- Otomatik sunucu yanıt sistemi
- Local subnet detection

## 🐛 Bilinen Sorunlar ve Çözümler

### **Firewall Sorunları**
- **Sorun**: Port 55555 ve 49876 engellenmiş
- **Çözüm**: Windows Firewall'da portları açın

### **Antivirus Engellemesi**
- **Sorun**: Ağ bağlantıları engellenmiş
- **Çözüm**: Antivirus ayarlarında uygulamaya izin verin

### **Router Ayarları**
- **Sorun**: UDP broadcast engellenmiş
- **Çözüm**: Router'da broadcast forwarding'i açın

## 📈 Gelecek Geliştirmeler

- [ ] Şifreli mesajlaşma
- [ ] Dosya transferi
- [ ] Grup sohbetleri
- [ ] Mesaj geçmişi kaydetme
- [ ] Kullanıcı avatarları
- [ ] Emoji desteği
- [ ] Ses/video arama

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir. Ticari kullanım için izin gereklidir.

## 👨‍💻 İletişim

**Geliştirici:** Berkay Sabuncu  
**E-posta:** berkaysbncc@gmail.com  
**GitHub:** [@brkysbnc](https://github.com/brkysbnc)  
**Proje:** [csharp-winforms-chat](https://github.com/brkysbnc/csharp-winforms-chat)

---

*Bu proje, Yazılım Mühendisliği 2/A sınıfı öğrencisi Berkay Sabuncu tarafından geliştirilmiştir.*
