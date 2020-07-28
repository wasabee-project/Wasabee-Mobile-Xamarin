using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Messages;
using System;
using System.Threading.Tasks;

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

        public override Task Initialize()
        {
            LogsCollection = new MvxObservableCollection<string>();

            return base.Initialize();
        }

        public MvxObservableCollection<string> LogsCollection { get; set; }
    }
}