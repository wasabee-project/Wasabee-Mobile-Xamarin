using MvvmCross.Binding.BindingContext;
using MvvmCross.Forms.Views;
using MvvmCross.ViewModels;
using Rg.Plugins.Popup.Pages;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views
{
    public abstract class BaseDialogPage : PopupPage, IMvxPage
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(IMvxViewModel), typeof(IMvxElement), default(MvxViewModel), BindingMode.Default, null, ViewModelChanged, null, null);

        protected BaseDialogPage() : base()
        {

        }

        internal static void ViewModelChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (newvalue != null)
            {
                if (bindable is IMvxElement element)
                    element.DataContext = newvalue;
                else
                    bindable.BindingContext = newvalue;
            }
        }

        public IMvxViewModel ViewModel
        {
            get => BindingContext as IMvxViewModel;
            set
            {
                DataContext = value;
                SetValue(ViewModelProperty, value);
                OnViewModelSet();
            }
        }

        public object DataContext
        {
            get => BindingContext.DataContext;
            set
            {
                if (value != null && !(_bindingContext != null && ReferenceEquals(DataContext, value)))
                    BindingContext = new MvxBindingContext(value);
            }
        }

        private IMvxBindingContext _bindingContext;
        public new IMvxBindingContext BindingContext
        {
            get
            {
                if (_bindingContext == null)
                    BindingContext = new MvxBindingContext(base.BindingContext);
                return _bindingContext;
            }
            set
            {
                _bindingContext = value;
                base.BindingContext = _bindingContext.DataContext;
            }
        }

        protected virtual void OnViewModelSet()
        {
            ViewModel?.ViewCreated();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel?.ViewAppearing();
            ViewModel?.ViewAppeared();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel?.ViewDisappearing();
            ViewModel?.ViewDisappeared();
            ViewModel?.ViewDestroy();
        }
    }


    public class BaseDialogPage<TViewModel> : BaseDialogPage, IMvxPage<TViewModel> where TViewModel : BaseDialogViewModel
    {
        public new static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(TViewModel), typeof(IMvxElement<TViewModel>), default(TViewModel), BindingMode.Default, null, ViewModelChanged, null, null);

        public new TViewModel ViewModel
        {
            get => (TViewModel)base.ViewModel;
            set => base.ViewModel = value;
        }

        public MvxFluentBindingDescriptionSet<IMvxElement<TViewModel>, TViewModel> CreateBindingSet()
        {
            return this.CreateBindingSet<IMvxElement<TViewModel>, TViewModel>();
        }
    }
}