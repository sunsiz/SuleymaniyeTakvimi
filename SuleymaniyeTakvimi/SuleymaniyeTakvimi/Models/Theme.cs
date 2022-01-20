using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Models
{
    public static class Theme
    {
        //0 is dark, 1 is light
        private const int tema = 1;
        public static int Tema
        {
            get => Preferences.Get(nameof(Tema), tema);
            set => Preferences.Set(nameof(Tema), value);
        }
    }
}