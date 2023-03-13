using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ReactiveUI;
using WalletWasabi.Fluent.Helpers;

namespace WalletWasabi.Fluent.Controls;

/// <summary>
/// A simple overlay Dialog control.
/// </summary>
public class Dialog : ContentControl
{
	private Panel? _dismissPanel;
	private Panel? _overlayPanel;
	private bool _canCancelOpenedOnPointerPressed;
	private bool _canCancelActivatedOnPointerPressed;

	public static readonly StyledProperty<bool> IsDialogOpenProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(IsDialogOpen));

	public static readonly StyledProperty<bool> IsActiveProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(IsActive));

	public static readonly StyledProperty<bool> IsBusyProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(IsBusy));

	public static readonly StyledProperty<bool> IsBackEnabledProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(IsBackEnabled));

	public static readonly StyledProperty<bool> IsCancelEnabledProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(IsCancelEnabled));

	public static readonly StyledProperty<bool> EnableCancelOnPressedProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(EnableCancelOnPressed));

	public static readonly StyledProperty<bool> EnableCancelOnEscapeProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(EnableCancelOnEscape));

	public static readonly StyledProperty<double> MaxContentHeightProperty =
		AvaloniaProperty.Register<Dialog, double>(nameof(MaxContentHeight), double.PositiveInfinity);

	public static readonly StyledProperty<double> MaxContentWidthProperty =
		AvaloniaProperty.Register<Dialog, double>(nameof(MaxContentWidth), double.PositiveInfinity);

	public static readonly StyledProperty<double> IncreasedWidthThresholdProperty =
		AvaloniaProperty.Register<Dialog, double>(nameof(IncreasedWidthThreshold), double.NaN);

	public static readonly StyledProperty<double> IncreasedHeightThresholdProperty =
		AvaloniaProperty.Register<Dialog, double>(nameof(IncreasedHeightThreshold), double.NaN);

	public static readonly StyledProperty<double> FullScreenHeightThresholdProperty =
		AvaloniaProperty.Register<Dialog, double>(nameof(FullScreenHeightThreshold), double.NaN);

	public static readonly StyledProperty<bool> FullScreenEnabledProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(FullScreenEnabled));

	public static readonly StyledProperty<bool> IncreasedWidthEnabledProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(IncreasedWidthEnabled));

	public static readonly StyledProperty<bool> IncreasedHeightEnabledProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(IncreasedHeightEnabled));

	public static readonly StyledProperty<bool> IncreasedSizeEnabledProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(IncreasedSizeEnabled));

	public static readonly StyledProperty<bool> ShowAlertProperty =
		AvaloniaProperty.Register<Dialog, bool>(nameof(ShowAlert));

	public Dialog()
	{
		this.GetObservable(IsDialogOpenProperty).Subscribe(UpdateOpenedDelay);

		this.WhenAnyValue(x => x.Bounds)
			.Subscribe(bounds =>
			{
				var width = bounds.Width;
				var height = bounds.Height;
				var increasedWidthThreshold = IncreasedWidthThreshold;
				var increasedHeightThreshold = IncreasedHeightThreshold;
				var fullScreenHeightThreshold = FullScreenHeightThreshold;
				var canIncreasedWidth = !double.IsNaN(increasedWidthThreshold)
										&& width < increasedWidthThreshold;
				var canIncreasedHeight = !double.IsNaN(increasedHeightThreshold)
										 && height < increasedHeightThreshold;
				var canGoToFullScreen = !double.IsNaN(fullScreenHeightThreshold)
										&& height < fullScreenHeightThreshold;
				IncreasedWidthEnabled = canIncreasedWidth && !canIncreasedHeight;
				IncreasedHeightEnabled = !canIncreasedWidth && canIncreasedHeight;
				IncreasedSizeEnabled = canIncreasedWidth && canIncreasedHeight;
				FullScreenEnabled = canIncreasedWidth && canGoToFullScreen;
			});
	}

	public bool IsDialogOpen
	{
		get => GetValue(IsDialogOpenProperty);
		set => SetValue(IsDialogOpenProperty, value);
	}

	public bool IsActive
	{
		get => GetValue(IsActiveProperty);
		set => SetValue(IsActiveProperty, value);
	}

	public bool IsBusy
	{
		get => GetValue(IsBusyProperty);
		set => SetValue(IsBusyProperty, value);
	}

	public bool IsBackEnabled
	{
		get => GetValue(IsBackEnabledProperty);
		set => SetValue(IsBackEnabledProperty, value);
	}

	public bool IsCancelEnabled
	{
		get => GetValue(IsCancelEnabledProperty);
		set => SetValue(IsCancelEnabledProperty, value);
	}

	public bool EnableCancelOnPressed
	{
		get => GetValue(EnableCancelOnPressedProperty);
		set => SetValue(EnableCancelOnPressedProperty, value);
	}

	public bool EnableCancelOnEscape
	{
		get => GetValue(EnableCancelOnEscapeProperty);
		set => SetValue(EnableCancelOnEscapeProperty, value);
	}

	public double MaxContentHeight
	{
		get => GetValue(MaxContentHeightProperty);
		set => SetValue(MaxContentHeightProperty, value);
	}

	public double MaxContentWidth
	{
		get => GetValue(MaxContentWidthProperty);
		set => SetValue(MaxContentWidthProperty, value);
	}

	public double IncreasedWidthThreshold
	{
		get => GetValue(IncreasedWidthThresholdProperty);
		set => SetValue(IncreasedWidthThresholdProperty, value);
	}

	public double IncreasedHeightThreshold
	{
		get => GetValue(IncreasedHeightThresholdProperty);
		set => SetValue(IncreasedHeightThresholdProperty, value);
	}

	public double FullScreenHeightThreshold
	{
		get => GetValue(FullScreenHeightThresholdProperty);
		set => SetValue(FullScreenHeightThresholdProperty, value);
	}

	private bool FullScreenEnabled
	{
		get => GetValue(FullScreenEnabledProperty);
		set => SetValue(FullScreenEnabledProperty, value);
	}

	private bool IncreasedWidthEnabled
	{
		get => GetValue(IncreasedWidthEnabledProperty);
		set => SetValue(IncreasedWidthEnabledProperty, value);
	}

	private bool IncreasedHeightEnabled
	{
		get => GetValue(IncreasedHeightEnabledProperty);
		set => SetValue(IncreasedHeightEnabledProperty, value);
	}

	private bool IncreasedSizeEnabled
	{
		get => GetValue(IncreasedSizeEnabledProperty);
		set => SetValue(IncreasedSizeEnabledProperty, value);
	}

	private bool ShowAlert
	{
		get => GetValue(ShowAlertProperty);
		set => SetValue(ShowAlertProperty, value);
	}

	private CancellationTokenSource? CancelPointerOpenedPressedDelay { get; set; }

	private CancellationTokenSource? CancelPointerActivatedPressedDelay { get; set; }

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		ApplicationHelper.MainWindowActivated.Subscribe(UpdateActivatedDelay);
	}

	private void UpdateOpenedDelay(bool isDialogOpen)
	{
		try
		{
			_canCancelOpenedOnPointerPressed = false;
			CancelPointerOpenedPressedDelay?.Cancel();

			if (isDialogOpen)
			{
				CancelPointerOpenedPressedDelay = new CancellationTokenSource();

				Task.Delay(TimeSpan.FromSeconds(1), CancelPointerOpenedPressedDelay.Token).ContinueWith(_ => _canCancelOpenedOnPointerPressed = true);
			}
		}
		catch (OperationCanceledException)
		{
			// ignored
		}
	}

	private void UpdateActivatedDelay(bool isWindowActivated)
	{
		try
		{
			if (!isWindowActivated)
			{
				_canCancelActivatedOnPointerPressed = false;
			}

			CancelPointerActivatedPressedDelay?.Cancel();

			if (isWindowActivated)
			{
				CancelPointerActivatedPressedDelay = new CancellationTokenSource();

				Task.Delay(TimeSpan.FromSeconds(5), CancelPointerActivatedPressedDelay.Token).ContinueWith(_ => _canCancelActivatedOnPointerPressed = true);
			}
		}
		catch (OperationCanceledException)
		{
			// ignored
		}
	}

	protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == IsDialogOpenProperty)
		{
			var isOpen = change.NewValue.GetValueOrDefault<bool>();

			PseudoClasses.Set(":open", isOpen);

			if (!isOpen)
			{
				PseudoClasses.Set(":alert", false);
			}
		}

		if (change.Property == IsBusyProperty)
		{
			PseudoClasses.Set(":busy", change.NewValue.GetValueOrDefault<bool>());
		}

		if (change.Property == ShowAlertProperty)
		{
			PseudoClasses.Set(":alert", change.NewValue.GetValueOrDefault<bool>());
		}
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);

		_dismissPanel = e.NameScope.Find<Panel>("PART_Dismiss");
		_overlayPanel = e.NameScope.Find<Panel>("PART_Overlay");

		if (this.GetVisualRoot() is TopLevel topLevel)
		{
			topLevel.AddHandler(PointerPressedEvent, CancelPointerPressed, RoutingStrategies.Tunnel);
			topLevel.AddHandler(KeyDownEvent, CancelKeyDown, RoutingStrategies.Tunnel);
		}
	}

	private void Close()
	{
		IsDialogOpen = false;
		ShowAlert = false;
	}

	private void CancelPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (IsDialogOpen && ShowAlert)
		{
			ShowAlert = false;
		}

		if (IsDialogOpen
		    && IsActive
		    && EnableCancelOnPressed
		    && !IsBusy
		    && _dismissPanel is { }
		    && _overlayPanel is { }
		    && _canCancelOpenedOnPointerPressed
		    && _canCancelActivatedOnPointerPressed)
		{
			var point = e.GetPosition(_dismissPanel);
			var isPressedOnTitleBar = e.GetPosition(_overlayPanel).Y < 30;

			if (!_dismissPanel.Bounds.Contains(point) && !isPressedOnTitleBar)
			{
				e.Handled = true;
				Close();
			}
		}
	}

	private void CancelKeyDown(object? sender, KeyEventArgs e)
	{
		if (IsDialogOpen && ShowAlert)
		{
			ShowAlert = false;
		}

		if (e.Key == Key.Escape && EnableCancelOnEscape && !IsBusy && IsActive)
		{
			e.Handled = true;
			Close();
		}
	}
}
