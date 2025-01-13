using GorselProgOdev3.Services;

namespace GorselProgOdev3;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        using Task task = NavigateToAuthPageAsync();
        FirestoreHelper.SetEnvironmentValue();

    }
    public async Task NavigateToAuthPageAsync()
    {
        await Navigation.PushModalAsync(new AuthPage());
    }
}