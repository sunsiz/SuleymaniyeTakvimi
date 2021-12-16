# Suleymaniye Takvimi
Suleymaniyetakvimi.com Xamarin mobil uygulaması

**Uygulama çalışma şekli:**
* İlk açıldığında cihazın konumunu ve zamanına göre [suleymaniyetakvimi.com](https://www.suleymaniyetakvimi.com) sitesinden bir aylık gelecek namaz vakitlerini alıyor. İndirdiği namaz vakitlerine göre o günlük namaz vakitlerini uygulamada gösteriyor ve kullanıcının seçimlerine uygun şekilde, seçilen namaz vakitleri için alarm, titreme ve bilidirileri bir haftalık zamanlıyor.
* Alarmların düzgün çalışması için Android'te Foreground Service kullanarak sürekli kalan bilidir göstererek uygulamanın arka planda çalışmasını sağlıyor. Android'te Alarm ve titreme için özel activity gösterek kullanıcının seçtiği sesi çalabiliyor ve titremeyi yapabiliyor. Eğer zamanlamanın son iki gününe geldiyse alarmı kapatırken tekrardan uygulamayı açarak yeniden bir haftalık zamanlama yapılmasını sağlıyor. İOS için sadece bildiri göstererek kullanıcının seçtiği sesle hatırlatma yapar.
* Konumu bir kere aldıktan sonra kaydederek, sonrasında konum kapalı olsa bile yine de namaz vakitlerini indirip göstermeye devam edebiliyor.
* Şehir adına tıklayınca konumu haritadan gösterebilir, Bugünun tarihine tıklayınca bir aylık namaz vaiktler takvimini gösteriyor.

**Mevcut Özellikler:**

* Cihazın konumunu alabilir ve ona göre günlük veya aylık namaz vakitlerini görüntüleyebilir.
* Kullanıcı seçtiği ayarlara göre bildirim, titreşim ve sesli hatırlatma yapabilir.
* Arayüzdeki tarihe tıklayarak aylık namaz vakitlerini gösteriyor, aylık takvim yerel dosyaya kaydediliyor.
* Kıble yönünü belirtmek için bir pusula eklendi.
* [Radyo Fıtrat](https://www.radyofitrat.com)'tan çevrimiçi radyo çalabiliyor, radyo web sayfsı ve radyo akışının linklerini gösteriyor.
* Sitelerimiz ve sosyal medya bağlantıları, Namaz vakitleri hakkındakı yazıyı gösteriyor
* Cihaz dil ayarlarına göre Türkçe ve İnglizce ara yüzü destekler.


**Mevcut sorunlar:**
* Alarm(bildiri) özelliği ios te sesi doğru çalamıyor.

# Suleymaniye Calendar
Suleymaniyetakvimi.com Xamarin mobile application

**Current Features:**
* Get device current location and display corresponding daily or monthly prayer times.
* Reminding with notification, vibration and sound based on user settings.
* When touch the date show monthly prayer times, monthly prayer times were saved in local file.
* Implementd a compass for indicate qibla direction.
* Online radio from [Radyo Fitrat](https://www.radyofitrat.com) and links for the site and radio schedules.
* Links for our sites and social media, An article about prayer times.
* Based on the device language settings support Turkish and English UI.


**Existing problems:**
* Alarm(notification) feature on IOS can not play expected sound most of the time.
