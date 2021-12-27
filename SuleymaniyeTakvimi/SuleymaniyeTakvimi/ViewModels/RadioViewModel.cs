using System;
using System.Diagnostics;
using System.Windows.Input;
using Acr.UserDialogs;
using MediaManager;
using MediaManager.Media;
using MediaManager.Player;
using SuleymaniyeTakvimi.Localization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class RadioViewModel:MvvmHelpers.BaseViewModel
    {
        //HtmlWebViewSource htmlSource;
        public Command PlayCommand { get; }
        // Launcher.OpenAsync is provided by Xamarin.Essentials.
        public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));

        //private ISimpleAudioPlayer _player;

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }
        //public HtmlWebViewSource HtmlSource
        //{
        //    get => htmlSource;
        //    set => SetProperty(ref htmlSource, value);
        //}

        public RadioViewModel()
        {
            IsBusy = true;
            Title = AppResources.IcerikYukleniyor;
            //HtmlSource = new HtmlWebViewSource();
            //htmlSource.Html = //@"<html><body><h1>Hello World!</h1></body></html>";
            //@"<html><body><script type='text/javascript'>var cstrFreePlayerUid = 376684; var cstrFreePlayerTheme = 'blue'; var cstrFreePlayerColor = '';</script>
            //<script type = 'text/javascript' src = '//corscdn.caster.fm/freeplayer/FreePlanPlayerEmbed.js' ></script>
            //<!--DO NOT REMOVE THE LINKS BELOW, THEY WILL BE HIDDEN(AND WILL HELP US A LOT) -->
            //<a id = 'cstrFreePlayerBL1' href = '//www.caster.fm/'> Free Shoutcast Hosting</a><a id = 'cstrFreePlayerBL2' href = '//www.caster.fm/' > Radio Stream Hosting</a>
            //<div id = 'cstrFreePlayerDiv' ></div></body></html>";
            //@"<html xmlns='http://www.w3.org/1999/xhtml'><head> <script type='text/javascript' src='https://corscdn.caster.fm/advplayer/js/jquery.free.min.js'></script><style class='keyframe-style' id='boost-keyframe' type='text/css'> .boostKeyframe{transform:scale3d(1,1,1);}</style> <script type='text/javascript' src='https://polyfill.io/v3/polyfill.min.js?features=Promise%2CPromise.prototype.finally%2Cfetch%2Cdefault'></script> <script type='text/javascript' src='https://corscdn.caster.fm/advplayer/js/howler.core.min.js'></script> <script type='text/javascript' src='https://corscdn.caster.fm/advplayer/js/last.fm.js'></script> <style media='' data-href='https://corscdn.caster.fm/advplayer/css/style.css'>@charset 'utf-8'; body{ width:100%; height:100%; padding:0px; margin:0px; color:#b2b2b2; font-family:Myriad Pro; font-size:12px; text-shadow: 1px 1px 0px #000; overflow:hidden; } #container{ border:4px solid #c5d845; border-radius: 4px; width:auto; margin:0px; background-color:#4b4b4b; min-width:269px; } #container-inner{ background-color:#232323; margin:5px 8px 5px 5px; width:auto; } #content{ padding:5px; width:auto; text-overflow:ellipsis; white-space:nowrap; max-height:100px; overflow:hidden; } .content{ padding:5px 0px 0px 10px; width:auto; text-overflow:ellipsis; white-space:nowrap; max-height:100px; } .content2, .content3, .content4 { position:relative; height:90px; width:calc(100%-10px); color:#bebebe; overflow:hidden; } .content1{ position:relative; top:0px; } .content2 { top:0px; } .content3 { text-align:center; margin:auto; z-index:103; top:0px; } .content4 { z-index:104; } #album{ border:4px solid #0f0f0f; float:left; margin-right:10px; margin-top:-7px; width:80px; height:80px; margin-left:-1px; border-radius:2px; } #albumbox1{ position:absolute; z-index:1; background-color:#1f1f1f; line-height:80px; text-align:center; height:80px; width:80px; font-size:14px; color:#666; } #albumbox2{ position:absolute; z-index:2; display:block; } #albumbox3{ position:absolute; z-index:3; } #content-live{ overflow:hidden; text-overflow: ellipsis; line-height:18px; padding-top:7px; } #content-border{ border:5px solid #262626; width:auto; } #content-holder{ margin-bottom:3px; border-left:1px solid #0b0b0b; border-right:1px solid #0b0b0b; height:99px; background-repeat:repeat-x; background-image:url(https://corscdn.caster.fm/advplayer/images/background.png); box-shadow: 1px 2px 2px #000; border-top:1px solid #0b0b0b; overflow:hidden; } #playerbar{ background-image:url(https://corscdn.caster.fm/advplayer/images/background-lower.png); border:1px solid #0b0b0b; border-top:1px solid #232323; border-bottom:0px; height:34px; box-shadow: 1px 2px 2px #000; } .clear{ clear:both; } .break{ width:2px; height:34px; background-image:url(https://corscdn.caster.fm/advplayer/images/break.png); background-repeat:no-repeat; } .right{ float:right; } .left{ float:left; } .title{ color:#eaff00; padding:0px; margin:0px; font-size:14px; } #livebtn{ float:left; padding:0px 0px; height:100%; line-height:33px; font-size:11px; text-align:center; width:50px; } .onair { color:#eaff00; } .offair { color:#fff; } #kbs92 { cursor:pointer; } #kbs128{ cursor:pointer; } #volumebar{ margin:0 auto; float:left; padding:0px; height:100%; width:113px; } #btn1{ height:34px; float:right; width:39px; padding:0px; margin:0px; background-repeat:no-repeat; background-position:center; cursor:pointer; } #btn2{ height:34px; float:right; width:36px; padding:0px; margin:0px; background-repeat:no-repeat; background-position:center; cursor:pointer; } #btn3{ height:34px; float:right; width:32px; padding:0px; margin:0px; background-repeat:no-repeat; background-position:center; cursor:pointer; } .btn1hover, .btn2hover, .btn3hover{ cursor:pointer; } #embedtitle{ float:left; } #embed{ float:left; -moz-box-shadow:inset 2px 2px 2px 1px #080808; -webkit-box-shadow:inset 2px 2px 2px 1px #080808; box-shadow:inset 2px 2px 2px 1px #080808; padding:0px; background-color:#1a1a1a; border-radius:20px; margin:-7px 0px 0px 9px; height:28px; width:259px; border-bottom:1px solid #484848; border-right:1px solid #484848; } #embedtext{ background-image:none; background-color:transparent; border:0px; padding:0px; margin:6px 11px 0px 11px; color:#bebebe; font-size:12px; width:calc(100% - 22px); color:#b2b2b2; } .iconbox{ margin-bottom:5px; font-size:11px; float:left; width:25%; white-space:nowrap; text-align:center; min-width:170px; } .icontext{ margin-left:5px; float:left; margin-top:6px; color:#b2b2b2; } #ico1{ } #ico2{ } #ico3{ } #ico4{ } @media (min-width:10px) and (max-width:395px){ #volumebarbreak{ display:none; } } @media (min-width:10px) and (max-width:279px){ #tuneinbreak{ display:none; } } @media (min-width:10px) and (max-width:391px){ #volumebar{ display:none; } #embed{ width:calc(100% - 95px); } .iconbox{ margin:5px 0px 0px 0px; font-size:11px; float:left; width:25%; white-space:nowrap; text-align:center; min-width:25%; } .icontext{ display:none; } } #btnplay-holder{ float:right; width:82px; height:34px; display:block; z-index:1000; position:relative; } .jp-play{ width:82px; height:68px; float:left; position:absolute; background-image:url(https://corscdn.caster.fm/advplayer/images/playbutton.png); margin-top:-17px; cursor:pointer; } .jp-pause{ width:82px; height:68px; float:left; position:absolute; background-image:url(https://corscdn.caster.fm/advplayer/images/pausebutton.png); margin-top:-17px; cursor:pointer; } .jp-loading{ width:82px; height:68px; float:left; position:absolute; background-image:url(https://corscdn.caster.fm/advplayer/images/playbutton_blank.png); margin-top:-17px; cursor:pointer; } .jp-no-solution { color: #FF6363; margin-left: 5px; } .stream { cursor: pointer; } section { width: 118px; height: auto; position: relative; margin:8px 0px 0px 0px; } #slider{ border-width: 1px; border-style: solid; border-color: #333 #333 #484848 #333; border-radius: 25px; width: 75px; position: absolute; height: 8px; margin-top:4px; background-color: #F00; background: -webkit-gradient(linear, 0% 0%, 0% 100%, from(#080808), to(#1a1a1a)); background: -webkit-linear-gradient(top, #080808, #1a1a1a); background: -moz-linear-gradient(top, #080808, #1a1a1a); background: -ms-linear-gradient(top, #080808, #1a1a1a); background: -o-linear-gradient(top, #080808, #1a1a1a); left: 25px; } .tooltip { position: absolute; display: none; top: -25px; width: 35px; height: 20px; color: #fff; text-align: center; font: 10pt Tahoma, Arial, sans-serif ; border-radius: 3px; border: 1px solid #333; -webkit-box-shadow: 1px 1px 2px 0px rgba(0, 0, 0, .3); box-shadow: 1px 1px 2px 0px rgba(0, 0, 0, .3); -moz-box-sizing: border-box; -webkit-box-sizing: border-box; box-sizing: border-box; background: -moz-linear-gradient(top, rgba(69,72,77,0.5) 0%, rgba(0,0,0,0.5) 100%); background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,rgba(69,72,77,0.5)), color-stop(100%,rgba(0,0,0,0.5))); background: -webkit-linear-gradient(top, rgba(69,72,77,0.5) 0%,rgba(0,0,0,0.5) 100%); background: -o-linear-gradient(top, rgba(69,72,77,0.5) 0%,rgba(0,0,0,0.5) 100%); background: -ms-linear-gradient(top, rgba(69,72,77,0.5) 0%,rgba(0,0,0,0.5) 100%); background: linear-gradient(top, rgba(69,72,77,0.5) 0%,rgba(0,0,0,0.5) 100%); filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#8045484d', endColorstr='#80000000',GradientType=0 ); } .jp-mute { content: ''; display: inline-block; width: 25px; height: 25px; right: -5px; background: url('https://corscdn.caster.fm/advplayer/images/volume.png') no-repeat 0 -75px; margin-top: -5px; float:left; margin-right:15px; cursor:pointer; } .ui-slider-handle { position: absolute; z-index: 2; width: 25px; height: 25px; cursor: pointer; background: url('https://corscdn.caster.fm/advplayer/images/handle.png') no-repeat 50% 50%; font-weight: bold; color: #1C94C4; outline: none; top: -7px; margin-left: -12px; margin-top:0px; } .ui-slider-range { background: #f7fd05; background: -moz-linear-gradient(top, #f7fd05 0%, #d4e626 100%); background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#f7fd05), color-stop(100%,#d4e626)); background: -webkit-linear-gradient(top, #f7fd05 0%,#d4e626 100%); background: -o-linear-gradient(top, #f7fd05 0%,#d4e626 100%); background: -ms-linear-gradient(top, #f7fd05 0%,#d4e626 100%); background: linear-gradient(top, #f7fd05 0%,#d4e626 100%); filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#f7fd05', endColorstr='#d4e626',GradientType=0 ); position: absolute; border: 0; top: 0; height: 100%; border-radius: 25px; } .slider { width: 300px; } .slider > .dragger { background: #8DCA09; background: -webkit-linear-gradient(top, #8DCA09, #72A307); background: -moz-linear-gradient(top, #8DCA09, #72A307); background: linear-gradient(top, #8DCA09, #72A307); -webkit-box-shadow: inset 0 2px 2px rgba(255,255,255,0.5), 0 2px 8px rgba(0,0,0,0.2); -moz-box-shadow: inset 0 2px 2px rgba(255,255,255,0.5), 0 2px 8px rgba(0,0,0,0.2); box-shadow: inset 0 2px 2px rgba(255,255,255,0.5), 0 2px 8px rgba(0,0,0,0.2); -webkit-border-radius: 10px; -moz-border-radius: 10px; border-radius: 10px; border: 1px solid #496805; width: 16px; height: 16px; } .slider > .dragger:hover { background: -webkit-linear-gradient(top, #8DCA09, #8DCA09); } .slider > .track, .slider > .highlight-track { background: #ccc; background: -webkit-linear-gradient(top, #bbb, #ddd); background: -moz-linear-gradient(top, #bbb, #ddd); background: linear-gradient(top, #bbb, #ddd); -webkit-box-shadow: inset 0 2px 4px rgba(0,0,0,0.1); -moz-box-shadow: inset 0 2px 4px rgba(0,0,0,0.1); box-shadow: inset 0 2px 4px rgba(0,0,0,0.1); -webkit-border-radius: 8px; -moz-border-radius: 8px; border-radius: 8px; border: 1px solid #aaa; height: 4px; } .slider > .highlight-track { background-color: #8DCA09; background: -webkit-linear-gradient(top, #8DCA09, #72A307); background: -moz-linear-gradient(top, #8DCA09, #72A307); background: linear-gradient(top, #8DCA09, #72A307); border-color: #496805; } .output{ padding:3px; position: absolute; display: none !important; top: -25px; color: #fff; text-align: center; font: 10pt Tahoma, Arial, sans-serif ; border-radius: 3px; border: 1px solid #333; -webkit-box-shadow: 1px 1px 2px 0px rgba(0, 0, 0, .3); box-shadow: 1px 1px 2px 0px rgba(0, 0, 0, .3); -moz-box-sizing: border-box; -webkit-box-sizing: border-box; box-sizing: border-box; background: -moz-linear-gradient(top, rgba(69,72,77,0.5) 0%, rgba(0,0,0,0.5) 100%); background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,rgba(69,72,77,0.5)), color-stop(100%,rgba(0,0,0,0.5))); background: -webkit-linear-gradient(top, rgba(69,72,77,0.5) 0%,rgba(0,0,0,0.5) 100%); background: -o-linear-gradient(top, rgba(69,72,77,0.5) 0%,rgba(0,0,0,0.5) 100%); background: -ms-linear-gradient(top, rgba(69,72,77,0.5) 0%,rgba(0,0,0,0.5) 100%); background: linear-gradient(top, rgba(69,72,77,0.5) 0%,rgba(0,0,0,0.5) 100%); filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#8045484d', endColorstr='#80000000',GradientType=0 ); } .slider-volume { width: 65px; margin-left:0px !important; float:left; margin-top:8px; position:relative !important; padding:0px; !important; } .slider-volume > .dragger { background: url('https://corscdn.caster.fm/advplayer/images/handle.png') no-repeat 50% 50%; width: 23px; height: 22px; margin: 0 auto; position:relative !important; background-repeat:no-repeat !important; margin-top:-13px !important; } .slider-volume > .track, .slider-volume > .highlight-track { height: 8px; background: #151515; background: -moz-linear-gradient(top, #080808, #1a1a1a); background: -webkit-linear-gradient(top, #080808, #1a1a1a); background: linear-gradient(top, #080808, #1a1a1a); border-bottom:1px solid #484848; border-right:1px solid #484848; -moz-border-radius: 5px; -webkit-border-radius: 5px; border-radius: 5px; position:relative !important; } .slider-volume > .highlight-track { background-color: #eaf514; position:relative !important; margin-top:-9px !important; border-top-right-radius: 0px; border-bottom-right-radius: 0px; margin-left:-10px !important; } .tooltipster-default { border-radius: 5px; border: 2px solid #000; background: #4c4c4c; color: #fff; } .tooltipster-default .tooltipster-content { font-family: Arial, sans-serif; font-size: 14px; line-height: 16px; padding: 8px 10px; overflow: hidden; } .tooltipster-default .tooltipster-arrow .tooltipster-arrow-border { } .tooltipster-icon { cursor: help; margin-left: 4px; } .tooltipster-base { padding: 0; font-size: 0; line-height: 0; position: absolute; left: 0; top: 0; z-index: 9999999; pointer-events: none; width: auto; overflow: visible; } .tooltipster-base .tooltipster-content { overflow: hidden; } .tooltipster-arrow { display: block; text-align: center; width: 100%; height: 100%; position: absolute; top: 0; left: 0; z-index: -1; } .tooltipster-arrow span, .tooltipster-arrow-border { display: block; width: 0; height: 0; position: absolute; } .tooltipster-arrow-top span, .tooltipster-arrow-top-right span, .tooltipster-arrow-top-left span { border-left: 8px solid transparent !important; border-right: 8px solid transparent !important; border-top: 8px solid; bottom: -7px; } .tooltipster-arrow-top .tooltipster-arrow-border, .tooltipster-arrow-top-right .tooltipster-arrow-border, .tooltipster-arrow-top-left .tooltipster-arrow-border { border-left: 9px solid transparent !important; border-right: 9px solid transparent !important; border-top: 9px solid; bottom: -7px; } .tooltipster-arrow-bottom span, .tooltipster-arrow-bottom-right span, .tooltipster-arrow-bottom-left span { border-left: 8px solid transparent !important; border-right: 8px solid transparent !important; border-bottom: 8px solid; top: -7px; } .tooltipster-arrow-bottom .tooltipster-arrow-border, .tooltipster-arrow-bottom-right .tooltipster-arrow-border, .tooltipster-arrow-bottom-left .tooltipster-arrow-border { border-left: 9px solid transparent !important; border-right: 9px solid transparent !important; border-bottom: 9px solid; top: -7px; } .tooltipster-arrow-top span, .tooltipster-arrow-top .tooltipster-arrow-border, .tooltipster-arrow-bottom span, .tooltipster-arrow-bottom .tooltipster-arrow-border { left: 0; right: 0; margin: 0 auto; } .tooltipster-arrow-top-left span, .tooltipster-arrow-bottom-left span { left: 6px; } .tooltipster-arrow-top-left .tooltipster-arrow-border, .tooltipster-arrow-bottom-left .tooltipster-arrow-border { left: 5px; } .tooltipster-arrow-top-right span, .tooltipster-arrow-bottom-right span { right: 6px; } .tooltipster-arrow-top-right .tooltipster-arrow-border, .tooltipster-arrow-bottom-right .tooltipster-arrow-border { right: 5px; } .tooltipster-arrow-left span, .tooltipster-arrow-left .tooltipster-arrow-border { border-top: 8px solid transparent !important; border-bottom: 8px solid transparent !important; border-left: 8px solid; top: 50%; margin-top: -7px; right: -7px; } .tooltipster-arrow-left .tooltipster-arrow-border { border-top: 9px solid transparent !important; border-bottom: 9px solid transparent !important; border-left: 9px solid; margin-top: -8px; } .tooltipster-arrow-right span, .tooltipster-arrow-right .tooltipster-arrow-border { border-top: 8px solid transparent !important; border-bottom: 8px solid transparent !important; border-right: 8px solid; top: 50%; margin-top: -7px; left: -7px; } .tooltipster-arrow-right .tooltipster-arrow-border { border-top: 9px solid transparent !important; border-bottom: 9px solid transparent !important; border-right: 9px solid; margin-top: -8px; } .tooltipster-fade { opacity: 0; -webkit-transition-property: opacity; -moz-transition-property: opacity; -o-transition-property: opacity; -ms-transition-property: opacity; transition-property: opacity; } .tooltipster-fade-show { opacity: 1; } .tooltipster-grow { -webkit-transform: scale(0,0); -moz-transform: scale(0,0); -o-transform: scale(0,0); -ms-transform: scale(0,0); transform: scale(0,0); -webkit-transition-property: -webkit-transform; -moz-transition-property: -moz-transform; -o-transition-property: -o-transform; -ms-transition-property: -ms-transform; transition-property: transform; -webkit-backface-visibility: hidden; } .tooltipster-grow-show { -webkit-transform: scale(1,1); -moz-transform: scale(1,1); -o-transform: scale(1,1); -ms-transform: scale(1,1); transform: scale(1,1); -webkit-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1); -webkit-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -moz-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -ms-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -o-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); } .tooltipster-swing { opacity: 0; -webkit-transform: rotateZ(4deg); -moz-transform: rotateZ(4deg); -o-transform: rotateZ(4deg); -ms-transform: rotateZ(4deg); transform: rotateZ(4deg); -webkit-transition-property: -webkit-transform, opacity; -moz-transition-property: -moz-transform; -o-transition-property: -o-transform; -ms-transition-property: -ms-transform; transition-property: transform; } .tooltipster-swing-show { opacity: 1; -webkit-transform: rotateZ(0deg); -moz-transform: rotateZ(0deg); -o-transform: rotateZ(0deg); -ms-transform: rotateZ(0deg); transform: rotateZ(0deg); -webkit-transition-timing-function: cubic-bezier(0.230, 0.635, 0.495, 1); -webkit-transition-timing-function: cubic-bezier(0.230, 0.635, 0.495, 2.4); -moz-transition-timing-function: cubic-bezier(0.230, 0.635, 0.495, 2.4); -ms-transition-timing-function: cubic-bezier(0.230, 0.635, 0.495, 2.4); -o-transition-timing-function: cubic-bezier(0.230, 0.635, 0.495, 2.4); transition-timing-function: cubic-bezier(0.230, 0.635, 0.495, 2.4); } .tooltipster-fall { top: 0; -webkit-transition-property: top; -moz-transition-property: top; -o-transition-property: top; -ms-transition-property: top; transition-property: top; -webkit-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1); -webkit-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -moz-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -ms-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -o-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); } .tooltipster-fall-show { } .tooltipster-fall.tooltipster-dying { -webkit-transition-property: all; -moz-transition-property: all; -o-transition-property: all; -ms-transition-property: all; transition-property: all; top: 0px !important; opacity: 0; } .tooltipster-slide { left: -40px; -webkit-transition-property: left; -moz-transition-property: left; -o-transition-property: left; -ms-transition-property: left; transition-property: left; -webkit-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1); -webkit-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -moz-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -ms-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); -o-transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); transition-timing-function: cubic-bezier(0.175, 0.885, 0.320, 1.15); } .tooltipster-slide.tooltipster-slide-show { } .tooltipster-slide.tooltipster-dying { -webkit-transition-property: all; -moz-transition-property: all; -o-transition-property: all; -ms-transition-property: all; transition-property: all; left: 0px !important; opacity: 0; } .tooltipster-content-changing { opacity: 0.5; -webkit-transform: scale(1.1, 1.1); -moz-transform: scale(1.1, 1.1); -o-transform: scale(1.1, 1.1); -ms-transform: scale(1.1, 1.1); transform: scale(1.1, 1.1); } .tooltipster-punk { border-radius: 5px; border-bottom: 3px solid #f71169; color: #fff; } .tooltipster-punk .tooltipster-content { font-size: 12px; line-height: 16px; padding: 8px 10px; } @font-face { font-family: 'icomoon'; src:url('https://corscdn.caster.fm/advplayer/icomoon.eot?-l3v4h0'); src:url('https://corscdn.caster.fm/advplayer/icomoon.eot?#iefix-l3v4h0') format('embedded-opentype'), url('https://corscdn.caster.fm/advplayer/icomoon.ttf?-l3v4h0') format('truetype'), url('https://corscdn.caster.fm/advplayer/icomoon.woff?-l3v4h0') format('woff'), url('https://corscdn.caster.fm/advplayer/icomoon.svg?-l3v4h0#icomoon') format('svg'); font-weight: normal; font-style: normal; } [class^='icon-'], [class*=' icon-'] { font-family: 'icomoon'; speak: none; font-style: normal; font-weight: normal; font-variant: normal; text-transform: none; line-height: 1; -webkit-font-smoothing: antialiased; -moz-osx-font-smoothing: grayscale; } .icon-equalizer:before { content: '\eb58'; } .icon-link:before { content: '\ec96'; } .icon-embed2:before { content: '\eeca'; } .icon-equalizer:before { content: '\eb58'; } .icon-link:before { content: '\ec96'; } .icon-embed2:before { content: '\eeca'; } .icon-feed:before { content: '\e986'; } .icon-square-up-right:before { content: '\ee02'; } .icon-new-tab:before { content: '\eec6'; } .icon-new-tab2:before { content: '\eec7'; } .iconn { font-size: 15px; line-height: 34px; width: 39px; text-align: center; color: #909090; display: inline-block; margin: 0 auto; } .iconn:hover, .newwindow:hover span { color: #DDD; } #btn2 .iconn { width: 36px; } #btn3 .iconn { width: 32px; } .newwindow { margin-right: 5px;float: right;text-align: right; } .newwindow span { cursor:pointer; color: #909090; font-size: 15px; bottom: 4px; position: absolute; right: 6px; } #volumecount { display:none; } .spinner { width: 40px; height: 40px; position: absolute; z-index: 999999; right: 21px; bottom: -3px; } .double-bounce1, .double-bounce2 { width: 100%; height: 100%; border-radius: 50%; background-color: #fff; opacity: 0.6; position: absolute; top: 0; left: 0; -webkit-animation: sk-bounce 2.0s infinite ease-in-out; animation: sk-bounce 2.0s infinite ease-in-out; } .double-bounce2 { -webkit-animation-delay: -1.0s; animation-delay: -1.0s; } @-webkit-keyframes sk-bounce { 0%, 100% { -webkit-transform: scale(0.0) } 50% { -webkit-transform: scale(1.0) } } @keyframes sk-bounce { 0%, 100% { transform: scale(0.0); -webkit-transform: scale(0.0); } 50% { transform: scale(1.0); -webkit-transform: scale(1.0); } } </style> <style media='' data-href='https://corscdn.caster.fm/advplayer/css/blue.css'>#container{ border: 4px solid #4599d8; } .title{ color:#0090ff; } .onair { color:#0090ff; } .offair { color:#fff; } .active_stream { color:#0090ff !important; } .slider-volume > .highlight-track{ background-color: #0090ff; background: -moz-linear-gradient(top, #05befd, #2f9eda); background: -webkit-linear-gradient(top, #05befd, #2f9eda); background: linear-gradient(top, #05befd, #2f9eda); } .btn1hover span, .btn2hover span, .btn3hover span{ color:#0090ff!important; } .tooltipster-punk { border-radius: 5px; border-bottom: 3px solid #0090ff; background: #2a2a2a; color: #fff; } @keyframes neonspark { 0% { color: #0090FF;font-weight:bold;} 100% { color: #0068B8;font-weight:bold;} }</style> <style> body { font-family: Arial, Helvetica, sans-serif; } .onair { color: #ff4f4f; } #errorContainer { display: block; background: #ff656529; border: #ff5a5a4f 2px solid; color: #ec8282; font-size: 14px; cursor: pointer; text-shadow: 1px 1px 1px #000; margin-bottom: 5px; padding: 5px; white-space: normal; display:none; } .error_icon { float: right; font-size: 16px; margin-right: 5px; position: absolute; right: 0; } </style> <script async='' src='https://www.googletagmanager.com/gtag/js?id=UA-17379854-1'></script> <script> window.dataLayer = window.dataLayer || []; function gtag(){dataLayer.push(arguments);} gtag('js', new Date()); gtag('config', 'UA-17379854-1'); </script> </head> <body> <div id='jquery_jplayer_1'></div> <div id='container'> <div id='container-inner'> <div id='content-border'> <div id='content-holder'> <div id='errorContainer' class='content content1'> <div style='float:left;margin-right: 20px;'>Error: cannot play the stream. Either the radio is currently off-air or your browser doesn't support the stream format. </div> <div class='error_icon'><i class='fas fa-window-close' aria-hidden='true'></i></div> <div class='clear'></div> </div> <div id='content' class='content1'> <div id='content-live'> <div id='album'> <div id='albumbox1'>Radyo Fıtrat</div> <div id='albumbox2'><img style='height:80px;width:100px;' id='albumart' src='http://radyofitrat.com/img/fitratlogoRadyoDik.png' width='100px' height='80px'></div> <div id='albumbox3'><img src='https://corscdn.caster.fm/advplayer/images/gloss.png' style='opacity:0.4;'></div> </div> <span id='titletxt' class='title neontitle boostKeyframe' style='animation: 1s ease 0s infinite normal forwards running neonspark;'> Radyo Fıtrat</span><br> <span id='artisttxt' style='color:#b2b2b2; font-weight:bold; font-size:12px;'>2021 </span> <br> <span id='albumtxt' style='color:#b2b2b2; font-weight:bold; font-size:12px;'></span> <br> <span id='streamspec' style='color:#fefefe; font-size:11px;'><a href='http://www.radyofitrat.com/' target='_blank' style='color:#fefefe; font-size:11px;'>Radyo Fıtrat</a></span> <span class='jp-no-solution' style='display:none;'>Error: Your Browser doesn't support the stream codec/s.</span> </div> <div class='clear'></div> </div> </div> <div id='playerbar'> <div id='jp_container_1'> <div id='livebtn' class='neontitle onair boostKeyframe' style='animation: 1s ease 0s infinite normal forwards running neonspark;'>ON-AIR</div> <div class='break left'></div> <div id='volumebar'> <section> <span class='tooltip'></span> <span class='jp-mute'></span> <input id='volumecount' type='text' data-slider='true' value='1.0' data-slider-highlight='true' data-slider-theme='volume' style='display: none;'><span class='output'></span> </section> </div> <div class='break left' id='volumebarbreak'></div> <div id='btnplay-holder'> <div class='spinner' style='display:none;'> <div class='double-bounce1'></div> <div class='double-bounce2'></div> </div> <div id='playbtn' class='jp-play'></div> </div> <div class='break right' id='tuneinbreak'></div> </div> </div> </div> </div> </div> <audio id='audioElement' preload='none'> </audio><div id='op-ads' tabindex='-1'></div> <img src='https://corscdn.caster.fm/advplayer/images/pausebutton.png' style='height:0.1px;visibility: hidden;'> <img src='https://corscdn.caster.fm/advplayer/images/playbutton_blank.png' style='height:0.1px;visibility: hidden;'> <script type='text/javascript'> var streamhost = 'shaincast.caster.fm'; var streamport = '22344'; var vautoplay = 'false'; var iscolor = 'false'; var detectmobile = 'false'; var isdbg = 0; var ispop = 0; var statusUrl = '//shaincast.caster.fm:22344/status-json.xsl'; var streamUrl = 'http://shaincast.caster.fm:22344/listen.mp3?authn48ce3f86b671412d6e253ef8d3c63a07'; var radioName = 'Radyo Fıtrat'; var radioCategory = 'Religion and Spirituality'; var country = 'TR'; var ads = false; </script> <script type='text/javascript' src='https://corscdn.caster.fm/advplayer/js/freeplayer.js'></script> </body></html>";
            //ReloadCommand=new Command();
            PlayCommand = new Command(Play);
            Title = AppResources.FitratinSesi;
            CheckInternet();
            //_player = CrossSimpleAudioPlayer.Current;
            IsBusy = false;
            if (CrossMediaManager.Current.IsPlaying()) IsPlaying = true;
        }

        private async void Play()
        {
            IsBusy = true;
            //if (!DependencyService.Get<IAudioService>().IsPlaying())
            //{
            //    DependencyService.Get<IAudioService>().Play("http://shaincast.caster.fm:22344/listen.mp3");
            //}
            //else
            //{
            //    DependencyService.Get<IAudioService>().Stop();
            //}
            //if (CrossSimpleAudioPlayer.Current.IsPlaying)
            //{
            //    _player.Stop();
            //    IsPlaying = false;
            //}
            //else
            //{
            //    DataService data = new DataService();
            //    if (data.CheckInternet())
            //    {
            //        Title = AppResources.IcerikYukleniyor;
            //        await Task.Run(async () =>
            //        {
            //            await PlayRadio().ConfigureAwait(false);
            //        }).ConfigureAwait(false);
            //        IsPlaying = true;
            //        Title = AppResources.FitratinSesi;
            //    }
            //}
            if (CrossMediaManager.Current.IsPlaying())
            {
                await CrossMediaManager.Current.Stop().ConfigureAwait(true);
                //CrossMediaManager.Current.Notification.Enabled = false;
                //CrossMediaManager.Current.Notification.UpdateNotification();
                IsPlaying = false;
            }
            else
            {
                Title = AppResources.IcerikYukleniyor;
                CheckInternet();
                var mediaItem = await CrossMediaManager.Current.Play("http://shaincast.caster.fm:22344/listen.mp3").ConfigureAwait(true);
                mediaItem.Title = AppResources.FitratinSesi;
                CrossMediaManager.Current.Notification.Enabled = false;
                //CrossMediaManager.Current.Notification.ShowNavigationControls = false;
                //CrossMediaManager.Current.Notification.ShowPlayPauseControls = false;
                mediaItem.MetadataUpdated += OnMediaItemOnMetadataUpdated;
                CrossMediaManager.Current.StateChanged += Current_StateChanged;
            }

            Title = AppResources.FitratinSesi;
            IsBusy = false;
        }

        //private async Task PlayRadio()
        //{
        //    string url = "http://shaincast.caster.fm:22344/listen.mp3";
        //    using var httpClient = new HttpClient();
        //    var fileStream = await httpClient.GetStreamAsync(url).ConfigureAwait(false);
        //    _player.Load(fileStream);
        //    _player.Play();

        //}

        private static void CheckInternet()
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                UserDialogs.Instance.Toast(AppResources.RadyoIcinInternet, TimeSpan.FromSeconds(7));
            }
        }

        private void Current_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            if (CrossMediaManager.Current.State == MediaPlayerState.Loading ||
                CrossMediaManager.Current.State == MediaPlayerState.Buffering)
            {
                Debug.WriteLine($"[Radio Player Buffering] {DateTime.Now.ToString("HH:m:s.f")}");
                Title = AppResources.IcerikYukleniyor;
                IsBusy = true;
                IsPlaying = false;
                return;
            }

            if (CrossMediaManager.Current.IsPlaying())
            {
                Debug.WriteLine($"[Radio Player Playing] {DateTime.Now.ToString("HH:m:s.f")}");
                IsPlaying = true;
                Title = AppResources.FitratinSesi;
                IsBusy = false;
                return;
            }

            if (CrossMediaManager.Current.State == MediaPlayerState.Stopped ||
                CrossMediaManager.Current.State == MediaPlayerState.Paused)
            {
                IsPlaying = false;
                Title = AppResources.FitratinSesi;
                IsBusy = false;
                return;
            }

            //if (CrossMediaManager.Current.State == MediaPlayerState.Failed)
            //{
            IsPlaying = false;
            Title = AppResources.FitratinSesi;
            UserDialogs.Instance.Toast(AppResources.RadyoIcinInternet, TimeSpan.FromSeconds(7));
                IsBusy = false;
            //}
        }

        private void OnMediaItemOnMetadataUpdated(object sender, MetadataChangedEventArgs args)
        {
            
        }
    }
}
