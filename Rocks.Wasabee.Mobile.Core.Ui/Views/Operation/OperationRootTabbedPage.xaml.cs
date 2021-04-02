using MvvmCross;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(Position = MasterDetailPosition.Detail, NoHistory = true)]
    public partial class OperationRootTabbedPage : MvxTabbedPage<OperationRootTabbedViewModel>
    {
        private bool _firstTime = true;
        private MvxSubscriptionToken _tokenPortal;
        private MvxSubscriptionToken _tokenMarker;

        public OperationRootTabbedPage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, true);

            _tokenPortal = Mvx.IoCProvider.Resolve<IMvxMessenger>().SubscribeOnMainThread<ShowPortalOnMapMessage>(ShowPortalOnMapMessageReceived);
            _tokenMarker = Mvx.IoCProvider.Resolve<IMvxMessenger>().SubscribeOnMainThread<ShowMarkerOnMapMessage>(ShowMarkerOnMapMessageReceived);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_firstTime)
            {
                ViewModel.ShowInitialViewModelsCommand.ExecuteAsync();
                _firstTime = false;
            }
        }

        private void ShowPortalOnMapMessageReceived(ShowPortalOnMapMessage msg)
        {
            try
            {
                CurrentPage = Children[0];
                if (CurrentPage.BindingContext is MapViewModel mapViewModel)
                {
                    mapViewModel.SelectedWasabeePin = mapViewModel.Anchors.FirstOrDefault(x => x.Portal.Id.Equals(msg.Portal.Id));
                    if (mapViewModel.SelectedWasabeePin != null)
                        mapViewModel.MoveToPortalCommand.Execute();
                }
            }
            catch (Exception e)
            {
                Mvx.IoCProvider.Resolve<ILoggingService>().Error(e, "Error Executing OperationRootTabbedPage.ShowPortalOnMapMessageReceived");
            }
        }

        private void ShowMarkerOnMapMessageReceived(ShowMarkerOnMapMessage msg)
        {
            try
            {
                CurrentPage = Children[0];
                if (CurrentPage.BindingContext is MapViewModel mapViewModel)
                {
                    mapViewModel.SelectedWasabeePin = mapViewModel.Markers.FirstOrDefault(x => x.Marker.Id.Equals(msg.Marker.Id));
                    if (mapViewModel.SelectedWasabeePin != null)
                        mapViewModel.MoveToPortalCommand.Execute();
                }
            }
            catch (Exception e)
            {
                Mvx.IoCProvider.Resolve<ILoggingService>().Error(e, "Error Executing OperationRootTabbedPage.ShowMarkerOnMapMessageReceived");
            }
        }
    }
}