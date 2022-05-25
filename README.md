# Suleymaniye Takvimi [![Build status](https://build.appcenter.ms/v0.1/apps/f227d4b3-61fd-4f14-a8c3-df6b2a0b45bc/branches/master/badge)](https://appcenter.ms)
Suleymaniyetakvimi.com Xamarin mobil uygulaması

**Uygulama çalışma şekli:**
* Android'te Özel bir 'Activity'i en öne getirp integre alarm uygulaması gibi gösterilebidiği için, İOS teki gibi bildirim göstermek yerine özel 'Activity' ile ses çalma, bidirim ve titremeler çalıştırıldı.
* Android'te [arka planda çalışma kısıtlandığı](https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/#background-execution-limits-in-android-80) ve [Doze mode](https://devblogs.microsoft.com/xamarin/understanding-androids-doze-functionality/) uygulandığı için ['Foreground Service'](https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/foreground-services) kullanarak uygulamanın sürekli çalışması sağlandı. Ama 'Foreground Service'in sürekli kalan bildirimi çinli şirketlerin telefonlarında RAM temizleyince kapanıyor aynı zamanda kullanıcının uygulama için pil kıstlamaması ve arka planda çalışmasına izin vermesi gerekiyor.
* Alarm (ses çalma, titreme, bildirim) zamanlaması haftalık olarak (aylık olunca cihazı zorlıyor) ayarlandı ve bu bir haftalık zamanlamanın son 2 günü kala, yine bir haftalık zamanlanacak şekilde ayarlandı. Daha hızlı olması için vakitleri aylık takvim dosyasından okuyarak zamanlıyor.
* Bazı cihazlarda (Xiaomi gibi) 'Foreground Service' kapanıp açılıyor ve zamanlamalar kaybolıyor (uygulama kapnıp açılıyor gibi), o yüzden servis başlarken ve uygulama ana sayfası açılırken her ikisinde zamanlama çalıştırıldı ve uygulamayı yavaşlatmaması için zamanlama geciktirelrek çalıştırıldı.
* Aylık takvimi cihazın konumu ve zamanına(Tarih ve zaman dilimine) göre [suleymaniyetakvimi.com](https://www.suleymaniyetakvimi.com) sitesinden o günden başlayarak bir aylık gelecek namaz vakitlerini alarak kaydediyor. İndirdiği namaz vakitlerine göre o günlük namaz vakitlerini uygulamada gösteriyor ve kullanıcının seçimlerine uygun şekilde, seçilen namaz vakitleri için alarm, titreme ve bilidirimleri bir haftalık zamanlıyor.
* İOS için sadece bildiri göstererek kullanıcının seçtiği sesle hatırlatma yapar.
* Konumu bir kere aldıktan sonra kaydederek, sonrasında konum kapalı olsa bile yine de namaz vakitlerini indirip göstermeye devam edebiliyor. Ana sayfadaki yenile tuşuna tıklayarak konumu ve namaz vakitlerini yenileyebiliyor.
* Şehir adına tıklayınca konumu haritadan gösterebilir, aylık takvime tıklayınca bir aylık namaz vaiktleri takvimini gösterebiliyor.

**Mevcut Özellikler:**

* Cihazın konumunu alabilir ve ona göre günlük veya aylık namaz vakitlerini görüntüleyebilir.
* Kullanıcı seçtiği ayarlara göre bildirim, titreşim ve sesli hatırlatma yapabilir.
* Arayüzdeki şehir adına tıklayarak haritadaki konumu, aylık takvime tıklayarak aylık namaz vakitlerini gösteriyor, aylık takvim yerel dosyaya kaydediliyor.
* Kıble yönünü belirtmek için bir pusula eklendi (pusula özelliğini desteklemeyen cihazlarda çalışmıyor).
* [Radyo Fıtrat](https://www.radyofitrat.com)'tan çevrimiçi radyo çalabiliyor, radyo web sayfsı ve radyo akışının linklerini gösteriyor.
* Sitelerimiz ve sosyal medya bağlantıları, Namaz vakitleri hakkındakı yazıyı gösteriyor.
* Kullanıcı seçimine göre Türkçe, İnglizce, Almanca, Arapça, Azerice, Farsça, Fransızca, Rusça, Çince ve Uygurca ara yüzü destekler.
* Koyu tema ve açık tema modları var.
* Android için widget özelliği eklendi.


**Yayin notları:**

*2.0.8 sürümündeki yenilikler:*
* Arapça, Azerice, Almanca, Farsça, Fransızca ve Rusça dil destekleri eklendi.
* Android 10 ve sonraki sürümlerde sistem kısıtlamalarından dolayı alarm çalma penceresi bildirim gösterme ve bildirim sesiyle değiştirildi.
* Artık yapışkan bildirimde tüm namaz vakitleri gösteriliyor.
* Widget özelliği eklendi.

*2.0.7 sürümündeki yenilikler:*
* Kullanıcılar arayüz yazı büyüklüğünü ayarlardan değiştirebilirler.
* Alarm başladıktan sonra keniliğinden kapanma süresini ayarlardan değiştirebilir.
* Ayarlara sürekli kalan yapışkan bildirimi kapatma seçeneği eklendi.
* Ana ekrana kalan süreyi gösteren bir özellik eklendi.

*2.0.6 sürümündeki yenilikler:*
* Cihaz pusulayı desteklemediğinde oluşan kilitlenme düzeltildi.
* Tekrar tekrar alarm sesi seçildiğinde oluşan donma sorunu düzeltildi.
* Konum davranışı iyileştirildi: Yenile düğmesiyle yenilenmedikçe, artık ilk kez alındıktan sonra konum sormuyor.
* Ayarlar sayfasında uygulama dilini seç seçeneği eklendi.
* Tema değişikliği ayarlar sayfasına taşındı.
* Yazı boyutları küçültüldü.

*2.0.5 sürümündeki yenilikler:*
* geçersiz konumdan dolayı kapanış düzeltildi.

*2.0.4 sürümündeki yenilikler:*
* Bazı nadir hatalar düzeltildi, konum ve namaz vakitlerini gösterme iyileştirildi.

*2.0.3 sürümündeki yenilikler:*
* Hakkında sayfası yeniden tasarlandı, renkler daha iyileştirildi, konum izni alma özelliği iyileştiridi.

*2.0.2 sürümündeki yenilikler:*
* Şimdilik Türkçe, İngilizce ve Çince dil tercümeleri mevcuttur, yakında başka dillerde eklenecek.

*2.0.1 sürümündeki yenilikler:*
* Alarm kurarken bazen oluşan ani kapanma olayı düzeltildi.

*2.0 sürümündeki yenilikler:*
* Uygulama yeniden tasarlandı ve yeni Özellikler eklendi.


**Ekran Görüntüleri:**

Namaz Vakitleri:    ![Namaz Vakitleri](Images/NamazVakitleri.png "Namaz Vakitleri")
Alarm Ayarları:     ![Alarm Ayarları](Images/AlarmAyarlari.png "Alarm Ayarları")
Kıble Gösterici:    ![Kıble Gösterici](Images/KibleGosterici.png "Kıble Gösterici")
Radyo Sayfası:      ![Radyo Sayfası](Images/Radyo.png "Radyo Sayfası")
Hakkında sayfası:   ![Hakkında sayfası](Images/Hakkinda.png "Hakkında sayfası")
Alarm sayfası:      ![Alarm sayfası](Images/Alarm.png "Alarm sayfası")


# Suleymaniye Calendar
Suleymaniyetakvimi.com Xamarin mobile application

**Current Features:**
* Get device current location and display corresponding daily or monthly prayer times.
* Reminding with notification, vibration and sound based on user preferences.
* When touch the Monthly time button show up monthly prayer times, monthly prayer times were saved in local file.
* Implementd a compass for indicate qibla direction.
* Online radio from [Radyo Fitrat](https://www.radyofitrat.com) and links for the site and radio schedules.
* Links for our sites and social media, An article about prayer times.
* Support Turkish, English, Arabic, Azerbaijani, Chinese, French, German, Persian, Russian and Uyghur language UI based on the user choice.
* Support dark and light UI mode.
* Android widget support for daily prayer times.


**Release notes:**

*What's new in version 2.0.8:*
* Added Arabic, Azerbaijani, German, Persian, French, Russian language support.
* Alarm window replaced with notification and notification ringtone in Android 10 and later version, because the system restriction.
* Steaky notification now include all prayer times.
* Added Widget support.

*What's new in version 2.0.7:*
* Users can change the interface font size from the settings.
* After the alarm starts, you can change the automatic shutdown time from the settings.
* Added option to turn off sticky notification in settings.
* Added a feature that displays the remaining time to the main screen.

*What's new in 2.0.6:*
* Fixed crashing when device not support compass.
* Fixed freezing when repeatedly choose alarm sound.
* Location behavior improved: now never asking location after get it first time, unless refresh it with refresh button.
* Added choose app language option in settings page.
* Theme changing moved to settings page.
* Font sizes decreased.

*What's new in version 2.0.5:*
* Fixed crashing over invalid location.

*What's new in version 2.0.4:*
* Fixed some rare errors, improved location and display prayer times related features.

*What's new in version 2.0.3:*
* Redesigned About page, improved colors and location permission request process.

*What's new in version 2.0.2:*
* Changed Default language for non translated languages to English.
* Currently support English, Turkish, Chinese and more will come.

*What's new in version 2.0.1:*
* Fixed an issue that crashing when setup alarms sometimes.

*What's new in version 2.0:*
* Completely redesigned the application and add new features.
