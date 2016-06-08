using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace VectorClock.Commander.View
{
    class AutoScrollTextBox : TextBox
    {
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            this.ScrollToEnd();
        }
    }
}
