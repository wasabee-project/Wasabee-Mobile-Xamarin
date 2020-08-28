using System;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Controls
{
    public class LinkLabel : Label
    {
        private event EventHandler Click;

        public string Name
        {
            get; set;
        }

        public void DoClick()
        {
            Click?.Invoke(this, null);
        }

        public event EventHandler Clicked
        {
            add
            {
                lock (this)
                {
                    Click += value;

                    var g = new TapGestureRecognizer();

                    g.Tapped += (s, e) => Click?.Invoke(s, e);

                    GestureRecognizers.Add(g);
                }
            }
            remove
            {
                lock (this)
                {
                    Click -= value;

                    GestureRecognizers.Clear();
                }
            }
        }
    }
}