﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using SuleymaniyeTakvimi.Models;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class OnBoardingViewModel : MvvmHelpers.BaseViewModel
    {
        private ObservableCollection<OnBoarding> items;
        private int position;
        private string nextButtonText;
        private string skipButtonText;

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
                    Content = "Süleymaniye Vakfi Takvim uygulamasına Hoş geldiniz.",
                    ImageUrl = "welcome"
                },
                new OnBoarding
                {
                    Title = "Özellikler",
                    Content = "Namaz vakitlerini bulunduğunuz konuma göre gösterir.\nNamaz vakti geldiğinde isterseniz bildiri ile uyarabilir.",
                    ImageUrl = "show"
                },
                new OnBoarding
                {
                    Title = "Yenilikler",
                    Content = "Uygulama siz değerli kullanıcılarımızın Önerileri ile yeniden tasarlandi.",
                    ImageUrl = "notification"
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
            SkipCommand = new Command(() =>
            {
                ExitOnBoarding();

            });
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
            get => items;
            set => SetProperty(ref items, value);
        }

        public string NextButtonText
        {
            get => nextButtonText;
            set => SetProperty(ref nextButtonText, value);
        }
        public string SkipButtonText
        {
            get => skipButtonText;
            set => SetProperty(ref skipButtonText, value);
        }

        public int Position
        {
            get => position;
            set
            {
                if (SetProperty(ref position, value))
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

        public ICommand NextCommand { get; private set; }
        public ICommand SkipCommand { get; private set; }
    }
}
