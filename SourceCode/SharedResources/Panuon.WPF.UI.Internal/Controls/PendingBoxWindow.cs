﻿using Panuon.WPF.UI.Internal.Implements;
using Panuon.WPF.UI.Internal.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Panuon.WPF.UI.Internal.Controls
{
    class PendingBoxWindow
        : Window
    {
        #region Fields
        private const string CancelButtonTemplateName = "PART_CancelButton";

        private const string MessageTextBlockTemplateName = "PART_MessageTextBlock";

        private const string CaptionTextBlockTemplateName = "PART_CaptionTextBlock";

        private const string SpinTemplateName = "PART_Spin";

        private Button _cancelButton;

        private object _cancelButtonContent;

        private PendingHandlerImpl _handler;

        private WindowX _owner;

        private string _messageText;

        private string _captionText;

        private TextBlock _messageTextBlock;

        private bool _canCancel;

        private bool _canClose = false;

        private Style _cancelButtonStyle;

        private Style _spinnerStyle;

        private Rect? _ownerRect;

        private bool _isClosed;

        private bool _interopOwnersMask;
        #endregion

        #region Ctor
        public PendingBoxWindow(Window owner, bool interopOwnersMask, Rect? ownerRect, string message, string caption, bool canCancel, string windowStyle, string cancelButtonStyle, string spinnerStyle, string contentTemplate, object cancelButtonContent, PendingHandlerImpl handler)
        {
            _captionText = caption;
            _messageText = message;
            _canCancel = canCancel;

            _cancelButtonContent = cancelButtonContent;

            _handler = handler;
            _handler.SetWindow(this);


            Style = XamlUtil.FromXaml<Style>(windowStyle);
            ContentTemplate = XamlUtil.FromXaml<DataTemplate>(contentTemplate);
            _cancelButtonStyle = XamlUtil.FromXaml<Style>(cancelButtonStyle);
            _spinnerStyle = XamlUtil.FromXaml<Style>(spinnerStyle);

            if (ownerRect == null)
            {
                WindowStartupLocation = owner == null
                  ? WindowStartupLocation.CenterScreen
                  : WindowStartupLocation.CenterOwner;
            }

            if (ownerRect == null)
            {
                Owner = owner;
            }
            else
            {
                _ownerRect = ownerRect;
                Topmost = true;
            }
            _interopOwnersMask = interopOwnersMask;
            if (owner is WindowX ownerX && interopOwnersMask)
            {
                ownerX.Dispatcher.BeginInvoke(new Action(() =>
                {
                    WindowXMaskManager.Push(ownerX);
                    ownerX.SetCurrentValue(WindowX.IsMaskVisibleProperty, true);
                }));
                _owner = ownerX;
            }
            Loaded += PendingBoxWindow_Loaded;
        }

        #endregion

        #region Overrides

        #region OnApplyTemplate
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                _cancelButton = FrameworkElementUtil.FindVisualChild<Button>(this, CancelButtonTemplateName);
                var captionTextBlock = FrameworkElementUtil.FindVisualChild<TextBlock>(this, CaptionTextBlockTemplateName);
                _messageTextBlock = FrameworkElementUtil.FindVisualChild<TextBlock>(this, MessageTextBlockTemplateName);
                var spinner = FrameworkElementUtil.FindVisualChild<Spin>(this, SpinTemplateName);

                if (_cancelButton != null)
                {
                    _cancelButton.Style = _cancelButtonStyle;
                    _cancelButton.Content = _cancelButtonContent;
                    _cancelButton.Visibility = _canCancel ? Visibility.Visible : Visibility.Collapsed;
                    _cancelButton.Click += CancelButton_Click;
                }
                if (captionTextBlock != null)
                {
                    if (_captionText == null)
                    {
                        captionTextBlock.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        captionTextBlock.Text = _captionText;
                    }
                }
                if (_messageTextBlock != null)
                {
                    _messageTextBlock.Text = _messageText;
                }
                if (spinner != null)
                {
                    spinner.Style = _spinnerStyle;
                    spinner.IsSpinning = true;
                }

            }), DispatcherPriority.DataBind);
        }
        #endregion

        #region OnClosing
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
        #endregion

        #region OnClosed
        protected override void OnClosed(EventArgs e)
        {
            if (_interopOwnersMask && _owner != null)
            {
                _owner.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (WindowXMaskManager.Pop(_owner) == 0)
                    {
                        _owner.SetCurrentValue(WindowX.IsMaskVisibleProperty, false);
                    }
                }));
            }
            base.OnClosed(e);
            _isClosed = true;
            _handler.TriggerClosed();
        }
        #endregion

        #region OnRenderSizeChanged

        #endregion

        #endregion

        #region Methods
        public new void Close()
        {
            if (_isClosed)
            {
                return;
            }

            _canClose = true;
            base.Close();
        }

        public void UpdateMessage(string message)
        {
            if (_isClosed)
            {
                return;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                if (_messageTextBlock != null)
                {
                    _messageTextBlock.Text = message;
                }
            }));
        }
        #endregion

        #region Event Handlers
        private void PendingBoxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_ownerRect is Rect ownerRect)
            {
                var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                var source = hwndSource.CompositionTarget.TransformToDevice;

                var width = (int)(ActualWidth * source.M11);
                var height = (int)(ActualHeight * source.M22);
                var left = ownerRect.X + (ownerRect.Width - width) / 2;
                var top = ownerRect.Y + (ownerRect.Height - height) / 2;

                Left = left / source.M11;
                Top = top / source.M22;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_handler.TriggerCancel())
            {
                Close();
            }
        }
        #endregion

    }
}
