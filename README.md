# Suleymaniye Takvimi
Suleymaniyetakvimi.com Xamarin mobil uygulaması

**Uygulama çalışma şekli:**
* Android'te Özel bir 'Activity'i en öne getirp integre alarm uygulaması gibi gösterilebidiği için, İOS teki gibi bildirim göstermek yerine özel 'Activity' ile ses çalma, bidirim ve titremeler çalıştırıldı.
* Android'te [arka planda çalışma kısıtlandığı](https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/#background-execution-limits-in-android-80) ve [Doze mode](https://devblogs.microsoft.com/xamarin/understanding-androids-doze-functionality/) uygulandığı için ['Foreground Service'](https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/foreground-services) kullanarak uygulamanın sürekli çalışması sağlandı. Ama 'Foreground Service'in sürekli kalan bildirimi çinli şirketlerin telefonlarında RAM temizleyince kapanıyor aynı zamanda kullanıcının uygulama için pil kıstlamaması ve arka planda çalışmasına izin vermesi gerekiyor.
* Alarm (ses çalma, titreme, bildirim) zamanlaması haftalık olarak (aylık olunca cihazı zorlıyor) ayarlandı ve bu bir haftalık zamanlamanın son 2 günü kala, yine bir haftalık zamanlanacak şekilde ayarlandı. Daha hızlı olması için vakitleri aylık takvim dosyasından okuyarak zamanlıyor.
* Bazı cihazlarda (Xiaomi gibi) 'Foreground Service' kapanıp açılıyor ve zamanlamalar kaybolıyor (uygulama kapnıp açılıyor gibi), o yüzden servis başlarken ve uygulama ana sayfası açılırken her ikisinde zamanlama çalıştırıldı ve uygulamayı yavaşlatmaması için zamanlama geciktirelrek çalıştırıldı.
* Aylık takvimi cihazın konumu ve zamanına(Tarih ve zaman dilimine) göre [suleymaniyetakvimi.com](https://www.suleymaniyetakvimi.com) sitesinden o günden başlayarak bir aylık gelecek namaz vakitlerini alarak kaydediyor. İndirdiği namaz vakitlerine göre o günlük namaz vakitlerini uygulamada gösteriyor ve kullanıcının seçimlerine uygun şekilde, seçilen namaz vakitleri için alarm, titreme ve bilidirimleri bir haftalık zamanlıyor.
* İOS için sadece bildiri göstererek kullanıcının seçtiği sesle hatırlatma yapar.
* Konumu bir kere aldıktan sonra kaydederek, sonrasında konum kapalı olsa bile yine de namaz vakitlerini indirip göstermeye devam edebiliyor. Ana sayfadaki yenile tuşuna tılayarak konumu ve namaz vakitlerini yenileyebiliyor.
* Şehir adına tıklayınca konumu haritadan gösterebilir, aylık takvime tıklayınca bir aylık namaz vaiktleri takvimini gösterebiliyor.

**Mevcut Özellikler:**

* Cihazın konumunu alabilir ve ona göre günlük veya aylık namaz vakitlerini görüntüleyebilir.
* Kullanıcı seçtiği ayarlara göre bildirim, titreşim ve sesli hatırlatma yapabilir.
* Arayüzdeki şehir adına tıklayarak haritadaki konumu, aylık takvime tıklayarak aylık namaz vakitlerini gösteriyor, aylık takvim yerel dosyaya kaydediliyor.
* Kıble yönünü belirtmek için bir pusula eklendi.
* [Radyo Fıtrat](https://www.radyofitrat.com)'tan çevrimiçi radyo çalabiliyor, radyo web sayfsı ve radyo akışının linklerini gösteriyor.
* Sitelerimiz ve sosyal medya bağlantıları, Namaz vakitleri hakkındakı yazıyı gösteriyor
* Cihaz dil ayarlarına göre Türkçe ve İnglizce ara yüzü destekler.
* Koyu tema ve açık tema modları var


**Mevcut sorunlar:**
* Alarm(bildirim) özelliği ios te seçilen sesi çalamayabılıyor.


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
* Support Turkish and English UI based on the device language setting.
* Support dark and light UI mode.


**Existing problems:**
* Alarm(notification) feature on IOS can not play selected sound most of the time.
