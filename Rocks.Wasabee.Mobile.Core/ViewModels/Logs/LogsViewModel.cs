using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Messages;
using System;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Logs
{
    public class LogsViewModel : BaseViewModel
    {
        private MvxSubscriptionToken _token;

        public LogsViewModel(IMvxMessenger mvxMessenger)
        {
            _token = mvxMessenger.Subscribe<NotificationMessage>(msg =>
            {
                LogsCollection.Add($"{DateTime.Now:T}: {msg.Message}");
                RaisePropertyChanged(() => LogsCollection);
            });
        }

        public MvxObservableCollection<string> LogsCollection { get; set; } = new MvxObservableCollection<string>();
    }
}