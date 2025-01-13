using Firebase.Auth;
using GorselProgOdev3.Services;
using Microsoft.Kiota.Abstractions;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GorselProgOdev3;

public partial class AuthPage : ContentPage
{
    private readonly FirebaseAuthProvider authProvider;

    public AuthPage()
    {
        InitializeComponent();
        authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyA-O4ZRF4MEvnIImBHikiqcyyeFu_8m7p8"));
    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            UserSession.Email = EmailEntry.Text;
            // Kullanýcý Giriþi
            var auth = await authProvider.SignInWithEmailAndPasswordAsync(EmailEntry.Text, PasswordEntry.Text);
            var token = auth.FirebaseToken; // Firebase Token alýnýr.
            await DisplayAlert("Baþarýlý", "Giriþ yapýldý", "Tamam");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Hata: {ex.Message}";
        }
        await Navigation.PopModalAsync();

    }

    private async void RegisterButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Kullanýcý Kaydý
            var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(EmailEntry.Text, PasswordEntry.Text);
            await DisplayAlert("Baþarýlý", "Kayýt oluþturuldu", "Tamam");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Hata: {ex.Message}";
        }
    }
    /*private UserData GetEmail()
    {
        string emailEntry = EmailEntry.Text.Trim();

        return new UserData()
        {
            Email = emailEntry,
        };
    }*/
    public static class UserSession
    {
        public static string? Email { get; set; }
    }
}
