﻿using System.Windows;

namespace Panuon.UI.Core
{
    public class ItemRemovingEventArgs : CancelRoutedEventArgs
    {
        #region Ctor
        public ItemRemovingEventArgs(RoutedEvent routedEvent, object item)
            : base(routedEvent)
        {
            Item = item;
        }
        #endregion

        #region Properties
        public object Item { get; set; }
        #endregion
    }
}
