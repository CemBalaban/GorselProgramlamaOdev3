using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace GorselProgOdev3
{
    public partial class WeatherPage : ContentPage
    {
        // Þehirlerin tutulduðu koleksiyon
        public ObservableCollection<City> Cities { get; set; } = [];

        public WeatherPage()
        {
            InitializeComponent();
            BindingContext = this; // BindingContext ayarý
        }

        // Þehir Ekleme Metodu
        private void OnAddCityClicked(object sender, EventArgs e)
        {
            string cityName = CityEntry.Text;

            if (!string.IsNullOrWhiteSpace(cityName))
            {
                // Þehir adýný normalize et
                cityName = NormalizeCityName(cityName);

                // Eðer þehir zaten listede varsa hata göster
                foreach (var city in Cities)
                {
                    if (city.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase))
                    {
                        DisplayAlert("Hata", "Bu þehir zaten listede.", "Tamam");
                        return;
                    }
                }

                // Þehri listeye ekle
                Cities.Add(new City { Name = cityName });
                CityEntry.Text = string.Empty; // Giriþ alanýný temizle

                /*string imageUrl = GenerateImageUrl(cityName);
                WeatherImage.Source = ImageSource.FromUri(new Uri(imageUrl));*/
            }
            else
            {
                DisplayAlert("Hata", "Lütfen bir þehir adý girin.", "Tamam");
            }
        }
        private static string GenerateImageUrl(string cityName)
        {
            return $"http://www.mgm.gov.tr/sunum/tahmin-klasik-5070.aspx?m={cityName}&basla=1&bitir=5&rC=111&rZ=fff";
        }

        // Þehir Silme Komutu
        public ICommand DeleteCityCommand => new Command<string>((cityName) =>
        {
            for (int i = 0; i < Cities.Count; i++)
            {
                if (Cities[i].Name == cityName)
                {
                    Cities.RemoveAt(i); // Þehri listeden sil
                    break;
                }
            }
        });

        // Türkçe karakterleri normalize eden metod
        private static string NormalizeCityName(string cityName)
        {
            return cityName
                .ToUpper(new CultureInfo("tr-TR"))
                .Replace('Ç', 'C')
                .Replace('Ð', 'G')
                .Replace('Ý', 'I')
                .Replace('Ö', 'O')
                .Replace('Ü', 'U')
                .Replace('Þ', 'S')
                .Replace('ç', 'c')
                .Replace('ð', 'g')
                .Replace('ý', 'i')
                .Replace('ö', 'o')
                .Replace('ü', 'u')
                .Replace('þ', 's');
        }
    }

    // Þehir Modeli
    public class City
    {
        public string Name { get; set; } = string.Empty;
        public string WeatherUrl => $"http://www.mgm.gov.tr/sunum/tahmin-klasik-5070.aspx?m={Name}&basla=1&bitir=5&rC=111&rZ=fff";
    }
}
