using Firebase.Auth;

namespace GorselProgOdev3
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell(new HomePage());
        }
    }
}
