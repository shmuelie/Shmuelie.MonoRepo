// <copyright file="LimitedTextBoxAutomationPeer.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;

namespace Shmuelie.UWP.Controls
{
    /// <summary>
    /// Exposes <see cref="LimitedTextBox"/> types to Microsoft UI Automation.
    /// </summary>
    public class LimitedTextBoxAutomationPeer : TextBoxAutomationPeer, ITextEditProvider, ITextProvider2, IValueProvider
    {
        private readonly LimitedTextBox owner;

        /// <summary>
        /// Creates a new instance of the <see cref="LimitedTextBoxAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The <see cref="LimitedTextBox"/> to create a peer for.</param>
        public LimitedTextBoxAutomationPeer(LimitedTextBox owner)
            : base(owner)
        {
            this.owner = owner;
        }

        /// <inheritdoc/>
        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            return base.GetPatternCore(patternInterface);
        }

        /// <inheritdoc/>
        public void SetValue(string value) => owner.Text = value;

        /// <inheritdoc/>
        public bool IsReadOnly => owner.IsReadOnly;

        /// <inheritdoc/>
        public string Value => owner.Text;

        /// <inheritdoc/>
        public ITextRangeProvider RangeFromAnnotation(IRawElementProviderSimple annotationElement)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITextRangeProvider GetCaretRange(out bool isActive)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITextRangeProvider[] GetSelection()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITextRangeProvider[] GetVisibleRanges()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITextRangeProvider RangeFromPoint(Point screenLocation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITextRangeProvider DocumentRange { get; }

        /// <inheritdoc/>
        public SupportedTextSelection SupportedTextSelection { get; }

        /// <inheritdoc/>
        public ITextRangeProvider GetActiveComposition()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ITextRangeProvider GetConversionTarget()
        {
            throw new NotImplementedException();
        }
    }
}
