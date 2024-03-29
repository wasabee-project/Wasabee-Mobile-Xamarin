﻿using Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Dialogs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkAssignmentDialog : BaseDialogPage<LinkAssignmentDialogViewModel>
    {
        public LinkAssignmentDialog()
        {
            InitializeComponent();
        }
    }
}