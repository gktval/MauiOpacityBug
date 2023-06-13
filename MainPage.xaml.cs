using OpacityBug.Views;

namespace OpacityBug;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private async void OnBtnSheetClicked(object sender, EventArgs e)
    {
        var btmSheet = (BottomSheet)this.FindByName("BtmSheet");

        if (btmSheet.IsOpen)
        {
            await btmSheet.CloseBottomSheet(true);
        }
        else
        {
            await btmSheet.OpenBottomSheet(true);
        }
    }
}


