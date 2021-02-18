using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using FFImageLoading.Forms;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    class CalendarViewModel:BaseViewModel
    {
        public ObservableCollection<Schedule> schedule { get; set; }
        String today;

        public string Today
        {
            get { return DateTime.Today.ToString("yyyy MMMM dd"); }
            set { SetProperty(ref today, value); }
        }


        public CalendarViewModel()
        {
            Title = "Süleymaniye Vakfı Takvimi";
            var data=new TakvimData();
            var takvim = data.takvim;
            schedule = new ObservableCollection<Schedule>
            {
                new Schedule
                {
                    Title = "Fecri Kazip",
                    Hour = takvim.FecriKazip,
                    State = CheckState(DateTime.Parse(takvim.FecriKazip),DateTime.Parse(takvim.FecriSadik)),
                    //Passed = DateTime.Now > DateTime.Parse(takvim.FecriSadik),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.FecriKazip) &&
                    //            DateTime.Now < DateTime.Parse(takvim.FecriSadik),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.FecriKazip)
                },
                new Schedule
                {
                    Title = "Fecri Sadik",
                    Hour = takvim.FecriSadik,
                    State = CheckState(DateTime.Parse(takvim.SabahSonu), DateTime.Parse(takvim.FecriSadik)),
                    //Passed = DateTime.Now > DateTime.Parse(takvim.SabahSonu),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.FecriSadik) &&
                    //            DateTime.Now < DateTime.Parse(takvim.SabahSonu),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.FecriSadik)
                },
                new Schedule
                {
                    Title = "Sabah Sonu",
                    Hour = takvim.SabahSonu,
                    State = CheckState(DateTime.Parse(takvim.Ogle), DateTime.Parse(takvim.SabahSonu)),
                    //Passed = DateTime.Now > DateTime.Parse(takvim.Ogle),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.SabahSonu) &&
                    //            DateTime.Now < DateTime.Parse(takvim.Ogle),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.SabahSonu)
                },
                new Schedule
                {
                    Title = "Öğle",
                    Hour = takvim.Ogle,
                    State = CheckState(DateTime.Parse(takvim.Ikindi), DateTime.Parse(takvim.Ogle)),
                    //Passed = DateTime.Now > DateTime.Parse(takvim.Ikindi),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.Ogle) &&
                    //            DateTime.Now < DateTime.Parse(takvim.Ikindi),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.Ogle)
                },
                new Schedule
                {
                    Title = "İkindi",
                    Hour = takvim.Ikindi,
                    State = CheckState(DateTime.Parse(takvim.Aksam), DateTime.Parse(takvim.Ikindi)),
                    //Passed = DateTime.Now > DateTime.Parse(takvim.Aksam),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.Ikindi) &&
                    //            DateTime.Now < DateTime.Parse(takvim.Aksam),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.Ikindi)
                },
                new Schedule
                {
                    Title = "Akşam",
                    Hour = takvim.Aksam,
                    State = CheckState(DateTime.Parse(takvim.Yatsi), DateTime.Parse(takvim.Aksam)),
                    //Passed = DateTime.Now > DateTime.Parse(takvim.Yatsi),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.Aksam) &&
                    //            DateTime.Now < DateTime.Parse(takvim.Yatsi),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.Aksam)
                },
                new Schedule
                {
                    Title = "Yatsı",
                    Hour = takvim.Yatsi,
                    State = CheckState(DateTime.Parse(takvim.YatsiSonu), DateTime.Parse(takvim.Yatsi)),
                    //Passed = DateTime.Now > DateTime.Parse(takvim.YatsiSonu),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.Yatsi) &&
                    //            DateTime.Now < DateTime.Parse(takvim.YatsiSonu),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.Yatsi)
                },
                new Schedule
                {
                    Title = "Yatsı Sonu",
                    Hour = takvim.YatsiSonu,
                    State = CheckState(DateTime.Parse(takvim.FecriKazip), DateTime.Parse(takvim.YatsiSonu)),
                    //Passed = DateTime.Now > DateTime.Parse(takvim.FecriKazip),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.YatsiSonu) &&
                    //            DateTime.Now < DateTime.Parse(takvim.FecriKazip),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.YatsiSonu)
                }
            };
        }

        private string CheckState(DateTime next, DateTime current)
        {
            var state = "";
            if (DateTime.Now > next) state = "Passed";
            if (DateTime.Now > current && DateTime.Now < next) state = "Happening";
            if (DateTime.Now < current) state = "Waiting";
            return state;
        }
    }
}
