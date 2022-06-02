namespace SuleymaniyeTakvimi.Models
{
    public class Sound
    {
        public Sound()
        {
        }

        public Sound(string fileName, string name)
        {
            FileName = fileName;
            Name = name;
        }

        //public int Index { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
    }
}
