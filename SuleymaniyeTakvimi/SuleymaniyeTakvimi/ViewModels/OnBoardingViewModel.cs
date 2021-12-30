using System.Collections.ObjectModel;
using System.Windows.Input;
using SuleymaniyeTakvimi.Models;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class OnBoardingViewModel : MvvmHelpers.BaseViewModel
    {
        private ObservableCollection<OnBoarding> _items;
        private int _position;
        private string _nextButtonText;
        private string _skipButtonText;

        public ICommand NextCommand { get; private set; }
        public ICommand SkipCommand { get; private set; }

        public OnBoardingViewModel()
        {
            SetNextButtonText("SONRA");
            SetSkipButtonText("ATLA");
            OnBoarding();
            LaunchNextCommand();
            LaunchSkipCommand();
        }

        private void SetNextButtonText(string nextButtonText) => NextButtonText = nextButtonText;
        private void SetSkipButtonText(string skipButtonText) => SkipButtonText = skipButtonText;

        private void OnBoarding()
        {
            Items = new ObservableCollection<OnBoarding>
            {
                new OnBoarding
                {
                    Title = "Merhaba!",
                    Content = "Süleymaniye Vakfi Takvim uygulamasına Hoş geldiniz.\nTarih tuşuna tıklayarak (Örnek: 14 EKİM) aylık takvimi görebilirsiniz.\nŞehir tuşuna tıklayarak(Örnek: İSTANBUL) konumunuzu haritadan görebilirsiniz.",
                    ImageUrl = "takvim"
                },
                new OnBoarding
                {
                    Title = "Namaz vakti ayarları",
                    Content = "Namaz vakitlerini bulunduğunuz konuma göre gösterir.\nNamaz vakti geldiğinde isterseniz alarm, bildiri veya titreşim ile uyarabilir.",
                    ImageUrl = "ayar"
                },
                new OnBoarding
                {
                    Title = "Kıble göstergesi",
                    Content = "Kıble göstergesi ile, kolayca kıble yönünü belirleyebilirsiniz.",
                    ImageUrl = "kible"
                },
                new OnBoarding
                {
                    Title = "Radyo Fıtrat",
                    Content = "Radyo Fıtrat radyomuzu uygulamadan ve isterseniz web sitemizden dinleyebilir, Yayın akışını instagramdan takip edebilirsiniz.",
                    ImageUrl = "radyo"
                }
            };
        }

        private void LaunchNextCommand()
        {

            NextCommand = new Command(() =>
            {
                if (LastPositionReached())
                {
                    ExitOnBoarding();
                }
                else
                {
                    MoveToNextPosition();
                }
            });
        }
        private void LaunchSkipCommand()
        {
            SkipCommand = new Command(ExitOnBoarding);
        }

        private static void ExitOnBoarding()
            => Application.Current.MainPage.Navigation.PopModalAsync();

        private void MoveToNextPosition()
        {
            var nextPosition = ++Position;
            Position = nextPosition;
        }

        private bool LastPositionReached()
            => Position == Items.Count - 1;

        public ObservableCollection<OnBoarding> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public string NextButtonText
        {
            get => _nextButtonText;
            set => SetProperty(ref _nextButtonText, value);
        }
        public string SkipButtonText
        {
            get => _skipButtonText;
            set => SetProperty(ref _skipButtonText, value);
        }

        public int Position
        {
            get => _position;
            set
            {
                if (SetProperty(ref _position, value))
                {
                    UpdateNextButtonText();
                }
            }
        }

        private void UpdateNextButtonText()
        {
            if (LastPositionReached())
            {
                SetNextButtonText("TAMAM");
            }
            else
            {
                SetNextButtonText("SONRA");
            }
        }
    }
}
