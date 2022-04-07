using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Models
{
    public static class Theme
    {
        //0 is dark, 1 is light
        private const int _tema = 1;
        public static int Tema
        {
            get => Preferences.Get(nameof(Tema), _tema);
            set => Preferences.Set(nameof(Tema), value);
        }
    }
}