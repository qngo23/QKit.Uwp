﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace QKit.Controls
{
    [TemplatePart(Name = BackgroundBrushPresenterName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = InactiveBackgroundBrushPresenterName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamlBackButtonName, Type = typeof(ButtonBase))]
    public class CustomTitleBar : ContentControl
    {
        #region Events
        public event EventHandler BackRequested;
        #endregion

        #region Fields
        public const string BackgroundBrushPresenterName = "BackgroundBrushPresenter";
        public const string InactiveBackgroundBrushPresenterName = "InactiveBackgroundBrushPresenter";
        public const string XamlBackButtonName = "XamlBackButton";

        private bool IsBackButtonNeeded = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT";

        private FrameworkElement BackgroundBrushPresenter;
        private FrameworkElement InactiveBackgroundBrushPresenter;
        private ButtonBase XamlBackButton;
        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty XamlBackButtonVisibilityProperty =
            DependencyProperty.Register(nameof(XamlBackButtonVisibility),
                typeof(Visibility),
                typeof(CustomTitleBar),
                new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty LeftInsetVisibilityProperty = DependencyProperty.Register(
            nameof(LeftInsetVisibility),
            typeof(Visibility),
            typeof(CustomTitleBar),
            new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty LeftInsetWidthProperty = DependencyProperty.Register(
            nameof(LeftInsetWidth),
            typeof(double),
            typeof(CustomTitleBar),
            new PropertyMetadata(default(double)));

        public static readonly DependencyProperty RightInsetWidthProperty = DependencyProperty.Register(
            nameof(RightInsetWidth),
            typeof(double),
            typeof(CustomTitleBar),
            new PropertyMetadata(default(double)));

        public static readonly DependencyProperty InactiveBackgroundBrushProperty = DependencyProperty.Register(
            nameof(InactiveBackgroundBrush),
            typeof(Brush),
            typeof(CustomTitleBar),
            new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty InactiveBorderBrushProperty = DependencyProperty.Register(
            nameof(InactiveBorderBrush),
            typeof(Brush),
            typeof(CustomTitleBar),
            new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty AutoHideProperty = DependencyProperty.Register(
            nameof(AutoHide),
            typeof(bool),
            typeof(CustomTitleBar),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty NavigationFrameProperty = DependencyProperty.Register(
            nameof(NavigationFrame),
            typeof(Frame),
            typeof(CustomTitleBar),
            new PropertyMetadata(null));
        #endregion

        #region Constructors
        public CustomTitleBar()
        {
            DefaultStyleKey = typeof(CustomTitleBar);

            if (DesignMode.DesignModeEnabled)
                return;

            Window.Current.Activated += Current_Activated;

            CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.Transparent;
            ApplicationView.GetForCurrentView().TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            if (IsBackButtonNeeded)
            {
                XamlBackButtonVisibility = Visibility.Visible;
                LeftInsetWidth = 48;
            }
        }
        #endregion

        #region Properties
        public Visibility XamlBackButtonVisibility
        {
            get { return (Visibility)GetValue(XamlBackButtonVisibilityProperty); }
            protected set { SetValue(XamlBackButtonVisibilityProperty, value); }
        }

        public Visibility LeftInsetVisibility
        {
            get { return (Visibility)GetValue(LeftInsetVisibilityProperty); }
            protected set { SetValue(LeftInsetVisibilityProperty, value); }
        }

        public double LeftInsetWidth
        {
            get { return (double)GetValue(LeftInsetWidthProperty); }
            protected set { SetValue(LeftInsetWidthProperty, value); }
        }

        public double RightInsetWidth
        {
            get { return (double)GetValue(RightInsetWidthProperty); }
            protected set { SetValue(RightInsetWidthProperty, value); }
        }

        public Brush InactiveBackgroundBrush
        {
            get { return (Brush)GetValue(InactiveBackgroundBrushProperty); }
            set { SetValue(InactiveBackgroundBrushProperty, value); }
        }

        public Brush InactiveBorderBrush
        {
            get { return (Brush)GetValue(InactiveBorderBrushProperty); }
            set { SetValue(InactiveBorderBrushProperty, value); }
        }

        public bool AutoHide
        {
            get { return (bool)GetValue(AutoHideProperty); }
            set { SetValue(AutoHideProperty, value); }
        }

        public Frame NavigationFrame
        {
            get { return (Frame)GetValue(NavigationFrameProperty); }
            set { SetValue(NavigationFrameProperty, value); }
        }

        #endregion

        #region Methods
        protected override void OnApplyTemplate()
        {
            BackgroundBrushPresenter = GetTemplateChild(BackgroundBrushPresenterName) as FrameworkElement;
            InactiveBackgroundBrushPresenter = GetTemplateChild(InactiveBackgroundBrushPresenterName) as FrameworkElement;
            XamlBackButton = GetTemplateChild(XamlBackButtonName) as ButtonBase;

            if (XamlBackButton != null)
                XamlBackButton.Click += XamlBackButton_Click;

            base.OnApplyTemplate();
        }

        private void UpdateTitleBarLayout(bool isTitleBarVisible)
        {
            var visibility = isTitleBarVisible ? Visibility.Visible : Visibility.Collapsed;

            if (AutoHide)
            {
                LeftInsetVisibility = Visibility.Visible;
                Visibility = visibility;
            }
            else
            {
                LeftInsetVisibility = visibility;
            }
        }

        protected void OnBackRequested()
        {
            BackRequested?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Event Handlers
        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            MinHeight = sender.Height;
            LeftInsetWidth = sender.SystemOverlayLeftInset;
            RightInsetWidth = sender.SystemOverlayRightInset;

            UpdateTitleBarLayout(sender.IsVisible);
        }

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender.IsVisible);
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            var isWindowActive = e.WindowActivationState != CoreWindowActivationState.Deactivated;
            var backgroundBrushPresenterVisibility = isWindowActive ? Visibility.Visible : Visibility.Collapsed;
            var backgroundInactiveBrushPresenterVisibility = isWindowActive ? Visibility.Collapsed : Visibility.Visible;

            if (BackgroundBrushPresenter != null)
                BackgroundBrushPresenter.Visibility = backgroundBrushPresenterVisibility;

            if (InactiveBackgroundBrushPresenter != null)
                InactiveBackgroundBrushPresenter.Visibility = backgroundInactiveBrushPresenterVisibility;
        }

        private void XamlBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationFrame == null)
                OnBackRequested();
            else if (NavigationFrame.CanGoBack)
                NavigationFrame.GoBack();
        }
        #endregion
    }
}