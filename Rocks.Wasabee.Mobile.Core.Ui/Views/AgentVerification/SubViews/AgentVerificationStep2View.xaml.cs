using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Text.RegularExpressions;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.AgentVerification.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgentVerificationStep2View : BaseContentView<AgentVerificationStep2SubViewModel>
    {
        public AgentVerificationStep2View()
        {
            InitializeComponent();

            AgentNameEntry.EntryField.TextChanged += EntryField_TextChanged;
        }

        private void EntryField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                var text = e.NewTextValue;
                if (text.Length > 16)
                    text = text.Substring(0, 16);

                var regex = new Regex("[a-zA-Z0-9]{0,16}");
                var match = regex.Match(text);
                if (match.Success)
                    entry.Text = match.Value;
            }
        }
    }
}