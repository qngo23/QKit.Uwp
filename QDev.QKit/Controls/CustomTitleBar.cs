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

namespace QDev.QKit.Controls
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
        protected const string BackgroundBrushPresenterName = "BackgroundBrushPresenter";
        protected const string InactiveBackgroundBrushPresenterName = "InactiveBackgroundBrushPresenter";
        protected const string XamlBackButtonName = "XamlBackButton";
        protected const int DefaultLeftInsetSize = 48;

        private FrameworkElement BackgroundBrushPresenter;
        private FrameworkElement InactiveBackgroundBrushPresenter;
        private ButtonBase XamlBackButton;

        private long _frameCanGoBackPropertyChangedToken;

        protected readonly bool _isBackButtonNeeded = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT";
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
            new PropertyMetadata(null, NavigationFrame_PropertyChanged));
        #endregion

        #region Constructors
        public CustomTitleBar()
        {
            DefaultStyleKey = typeof(CustomTitleBar);
            Loaded += CustomTitleBar_Loaded;
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

        protected virtual void RegisterFrame(DependencyPropertyChangedEventArgs e)
        {
            if (!_isBackButtonNeeded)
                return;

            if (e.OldValue is Frame oldFrame)
                oldFrame.UnregisterPropertyChangedCallback(Frame.CanGoBackProperty, _frameCanGoBackPropertyChangedToken);

            if (e.NewValue is Frame newFrame)
                _frameCanGoBackPropertyChangedToken =
                    newFrame.RegisterPropertyChangedCallback(Frame.CanGoBackProperty, FrameCanGoBack_PropertyChanged);

            UpdateXamlBackButtonVisibility();
        }

        protected void UpdateTitleBarLayout(bool isTitleBarVisible)
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

        protected void UpdateXamlBackButtonVisibility()
        {
            if (NavigationFrame != null && NavigationFrame.CanGoBack)
            {
                XamlBackButtonVisibility = Visibility.Visible;
                LeftInsetWidth = DefaultLeftInsetSize;
            }
            else
            {
                XamlBackButtonVisibility = Visibility.Collapsed;
                LeftInsetWidth = 0;
            }
        }

        protected void OnBackRequested()
        {
            BackRequested?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Event Handlers
        private static void NavigationFrame_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CustomTitleBar;
            if (d is CustomTitleBar titleBar)
                titleBar.RegisterFrame(e);
        }

        private void CustomTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            Unloaded += CustomTitleBar_Unloaded;

            if (DesignMode.DesignModeEnabled)
                return;

            Window.Current.Activated += Current_Activated;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            LeftInsetWidth = coreTitleBar.SystemOverlayLeftInset;
            RightInsetWidth = coreTitleBar.SystemOverlayRightInset;

            coreTitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
            coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;

            coreTitleBar.ExtendViewIntoTitleBar = true;

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            UpdateTitleBarLayout(coreTitleBar.IsVisible);
        }

        private void CustomTitleBar_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.Activated -= Current_Activated;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            coreTitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
            coreTitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
        }

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

        private void FrameCanGoBack_PropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateXamlBackButtonVisibility();
        }
        #endregion
    }
}
