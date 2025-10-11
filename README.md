# C# WinForms Chat Uygulaması

Socket tabanlı gerçek zamanlı chat uygulaması.

## Özellikler

- 🔍 **Otomatik Sunucu Keşfi**: UDP broadcast ile yakındaki sunucuları bulur
- 🔗 **Manuel Bağlantı**: IP adresi ile manuel bağlantı seçeneği
- 💬 **Çift Yönlü Mesajlaşma**: Sunucu ve istemci arasında tam mesajlaşma
- 🖥️ **Otomatik Sunucu Modu**: Sunucu bulunamazsa otomatik sunucu olur
- 🐛 **Debug Mesajları**: Detaylı log mesajları ile hata ayıklama
- 🌐 **IP Gösterimi**: Sunucu başlatıldığında local IP adresleri

## Kullanım

1. Uygulamayı çalıştırın
2. Otomatik olarak sunucu aranır veya sunucu moduna geçer
3. Başka bir bilgisayarda da çalıştırın
4. Otomatik keşif çalışmazsa "Manuel Bağlan" butonunu kullanın

## Teknik Detaylar

- **TCP Port**: 55555 (mesajlaşma)
- **UDP Port**: 49876 (sunucu keşfi)
- **Framework**: .NET Framework 4.7.2
- **UI**: Windows Forms

## Son Güncellemeler

- ✅ TCP istemci mesaj alma sorunu çözüldü
- ✅ Debug mesajları eklendi
- ✅ Manuel bağlantı seçeneği eklendi
- ✅ IP adresi gösterimi eklendi
- ✅ UDP broadcast iyileştirildi