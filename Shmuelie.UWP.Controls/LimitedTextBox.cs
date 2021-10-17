// <copyright file="LimitedTextBox.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

namespace Shmuelie.UWP.Controls
{
    public class LimitedTextBox : TextBox
    {
        /// <summary>
        /// Creates a new instance of the <see cref="LimitedTextBox"/> class.
        /// </summary>
        public LimitedTextBox()
        {
            DefaultStyleKey = typeof(LimitedTextBox);
        }

        /// <inheritdoc/>
        protected override AutomationPeer OnCreateAutomationPeer() => new LimitedTextBoxAutomationPeer(this);
    }
}
