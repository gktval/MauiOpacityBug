namespace OpacityBug.Views;

public partial class BottomSheet : ContentView
{
    public event EventHandler<bool> ExpandChanged;
    public event Func<Task<bool>> Closing;

    private const uint shortDuration = 125;
    private const uint regularDuration = shortDuration * 2u;
    private double _startSheetHeight;
    private bool _isExpanded;
    public int SheetId { get; set; }
    public IList<IView> BottomSheetContent => BottomSheetContentGrid.Children;

    #region Bindable Properties

    public static BindableProperty IsOpenProperty =
            BindableProperty.CreateAttached(nameof(IsOpen),
                typeof(bool),
                typeof(bool),
                false,
                BindingMode.OneWay
                );

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly BindableProperty SheetHeightProperty = BindableProperty.Create(
        nameof(SheetHeight),
        typeof(double),
        typeof(BottomSheet),
        360d,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public static readonly BindableProperty ExpandedHeightProperty = BindableProperty.Create(
        nameof(ExpandedSheetHeight),
        typeof(double),
        typeof(BottomSheet),
        600d,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public static readonly BindableProperty CompressedHeightProperty = BindableProperty.Create(
        nameof(ExpandedSheetHeight),
        typeof(double),
        typeof(BottomSheet),
        300d,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public double SheetHeight
    {
        get => (double)GetValue(SheetHeightProperty);
        set => SetValue(SheetHeightProperty, Math.Max(value, 0));
    }

    public double TotalHeight { get => SheetHeight + 80; }

    public double ExpandedSheetHeight
    {
        get => (double)GetValue(ExpandedHeightProperty);
        set => SetValue(ExpandedHeightProperty, Math.Max(value, 0));
    }

    public double CompressedSheetHeight
    {
        get => (double)GetValue(CompressedHeightProperty);
        set => SetValue(CompressedHeightProperty, Math.Max(value, 0));
    }

    public static readonly BindableProperty HeaderTextProperty = BindableProperty.Create(
        nameof(HeaderText),
        typeof(string),
        typeof(BottomSheet),
        string.Empty,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public static readonly BindableProperty HeaderStyleProperty = BindableProperty.Create(
        nameof(HeaderStyle),
        typeof(Style),
        typeof(BottomSheet),
        new Style(typeof(Label))
        {
            Setters =
            {
                new Setter
                {
                    Property = Label.FontSizeProperty,
                    Value = 24
                }
            }
        },
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public Style HeaderStyle
    {
        get => (Style)GetValue(HeaderStyleProperty);
        set => SetValue(HeaderStyleProperty, value);
    }

    public static readonly BindableProperty HeaderViewProperty = BindableProperty.Create(
        nameof(HeaderView),
        typeof(View),
        typeof(BottomSheet),
        null,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public View HeaderView
    {
        get => (View)GetValue(HeaderViewProperty);
        set => SetValue(HeaderViewProperty, value);
    }

    public static readonly BindableProperty BodyStyleProperty = BindableProperty.Create(
        nameof(BodyStyle),
        typeof(Style),
        typeof(BottomSheet),
        null,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null,
        propertyChanged:
        (bindableObject, oldValue, newValue) =>
        {
            if (newValue is not null && bindableObject is BottomSheet sheet && newValue != oldValue)
            {
                sheet.UpdateTheme();
            }
        });

    public Style BodyStyle
    {
        get => (Style)GetValue(BodyStyleProperty);
        set => SetValue(BodyStyleProperty, value);
    }

    public bool IsExpanded
    {
        get { return _isExpanded; }
    }

    private void UpdateTheme()
    {
        MainContent.Style = BodyStyle;
    }

    #endregion

    public BottomSheet()
    {
        InitializeComponent();

        //Set the Theme
        UpdateTheme();
    }

    public async Task OpenBottomSheet(bool showAnimation)
    {
        InputTransparent = false;
        BackgroundFader.IsVisible = true;
        IsOpen = true;
        SheetHeight = CompressedSheetHeight;
        _isExpanded = false;

        if (showAnimation)
        {
            _ = BackgroundFader.FadeTo(1, shortDuration, Easing.SinInOut);
            await MainContent.TranslateTo(0, 0, regularDuration, Easing.SinInOut);
        }
        else
        {
            BackgroundFader.Opacity = 1;
            MainContent.TranslationY = 0;
        }

    }

    public async Task<bool> CloseBottomSheet(bool showAnimation)
    {
        bool isClosing = true;
        Func<Task<bool>> handler = Closing;
        if (handler != null)
        {
            Delegate[] invocationList = handler.GetInvocationList();
            Task<bool>[] handlerTasks = new Task<bool>[invocationList.Length];
            for (int i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<Task<bool>>)invocationList[i])();
            }

            await Task.WhenAll(handlerTasks);
            isClosing = handlerTasks.First().GetAwaiter().GetResult();
        }

        if (!isClosing)
            return false;

        if (showAnimation)
        {
            _ = MainContent.TranslateTo(0, SheetHeight, shortDuration, Easing.SinInOut);
            await BackgroundFader.FadeTo(0, shortDuration, Easing.SinInOut);
        }
        else
        {
            MainContent.TranslationY = SheetHeight;
            BackgroundFader.Opacity = 0;
        }

        BackgroundFader.IsVisible = true;
        InputTransparent = true;
        IsOpen = false;

        return true;
    }

    async void CloseBottomSheetButton_Tapped(System.Object sender, System.EventArgs e) =>
        await CloseBottomSheet(true);

    private async void Grid_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Started)
        {
            _startSheetHeight = SheetHeight;
        }

        if (e.StatusType == GestureStatus.Completed)
        {
            if (_isExpanded)
            {
                if (Math.Abs(SheetHeight - CompressedSheetHeight) > 20)
                {
                    if (SheetHeight > ExpandedSheetHeight)
                    {
                        SheetHeight = ExpandedSheetHeight;
                        return;
                    }
                    SheetHeight = CompressedSheetHeight;
                    _isExpanded = false;
                    ExpandChanged?.Invoke(this, IsExpanded);
                }
                else
                    SheetHeight = ExpandedSheetHeight;
            }
            else
            {

                if (Math.Abs(SheetHeight - CompressedSheetHeight) > 20)
                {
                    //check if it was dragged up or down
                    if (SheetHeight < CompressedSheetHeight) // Down drag
                    {
                        if (!await CloseBottomSheet(true))
                            SheetHeight = CompressedSheetHeight;
                    }
                    else //Drag Up
                    {
                        SheetHeight = ExpandedSheetHeight;
                        _isExpanded = true;
                        ExpandChanged?.Invoke(this, IsExpanded);
                    }
                }
                else
                    SheetHeight = CompressedSheetHeight;
            }
        }
        else
        {
            if (e.TotalY != 0)
            {
#if WINDOWS || IOS
                 SheetHeight = (_startSheetHeight - e.TotalY);
#else
                SheetHeight = (_startSheetHeight - e.TotalY);
                _startSheetHeight = SheetHeight;
#endif
            }
        }
    }
}
