using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Logs
{
    public class LogsViewModel : BaseViewModel
    {
        private MvxSubscriptionToken _token;

        public LogsViewModel(IMvxMessenger mvxMessenger)
        {
            _token = mvxMessenger.Subscribe<NotificationMessage>(msg =>
            {
                LogsCollection.Add(new LogLine($"{DateTime.Now:T}: {msg.Message}", msg.Data));
                RaisePropertyChanged(() => LogsCollection);
            });
        }

        public override Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);

            return base.Initialize();
        }

        public MvxObservableCollection<LogLine> LogsCollection { get; set; } = new MvxObservableCollection<LogLine>();
    }

    public class LogLine : MvxViewModel
    {
        private IUserDialogs? _userDialogs;

        public LogLine(string text, IDictionary<string, string> data)
        {
            Text = text;
            Data = data;
        }
        
        public string Text { get; set; }
        public IDictionary<string, string> Data { get; }


        public IMvxCommand ShowDetailsCommand => new MvxCommand(ShowDetailsExecuted);
        private async void ShowDetailsExecuted()
        {
            _userDialogs ??= Mvx.IoCProvider.Resolve<IUserDialogs>();

            var text = Data.Aggregate(string.Empty, (current, kvp) => current + $"{kvp.Key} : {kvp.Value}\r\n");
            await _userDialogs.AlertAsync(text);
        }

    }
}