using System.Net.Http;
using System.Text.Json;

namespace GorselProgOdev3;

public partial class CurrencyPage : ContentPage
{
    public CurrencyPage()
    {
        InitializeComponent();
    }

    private async void GetRatesButton_Clicked(object sender, EventArgs e)
    {
        string url = "https://finans.truncgil.com/today.json";
        HttpClient client = new();

        try
        {
            // API'den veri al
            var response = await client.GetStringAsync(url);

            // JSON'u bir Dictionary olarak deserialize et
            var rawData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response);

            if (rawData == null)
            {
                await DisplayAlert("Hata", "Döviz kurlarý alýnamadý.", "Tamam");
                return;
            }

            // Ýlgili anahtarlarý seç
            var selectedKeys = new[] { "USD", "EUR", "GBP", "gram-altin", "tam-altin", "gumus" };

            // Seçilen verileri iþleyin
            var selectedCurrencies = selectedKeys
                .Where(key => rawData.ContainsKey(key))
                .Select(key =>
                {
                    var element = rawData[key];
                    return new SelectedCurrency
                    {
                        Name = key.ToUpper(),
                        Buying = element.TryGetProperty("Alýþ", out var buyingProp) ? buyingProp.GetString() ?? "N/A" : "N/A",
                        Selling = element.TryGetProperty("Satýþ", out var sellingProp) ? sellingProp.GetString() ?? "N/A" : "N/A",
                        Change = element.TryGetProperty("Deðiþim", out var changeProp) ? changeProp.GetString() ?? "N/A" : "N/A",
                        DirectionIcon = element.TryGetProperty("Deðiþim", out var changePropForIcon)
                            ? GetDirectionIcon(changePropForIcon.GetString() ?? "0")
                            : string.Empty
                    };
                })
                .ToList();

            // Veriyi CollectionView'e baðla
            CurrencyCollectionView.ItemsSource = selectedCurrencies;
        }
        catch (Exception ex)
        {
            // Hata durumunda mesaj göster
            await DisplayAlert("Hata", $"Döviz bilgileri alýnamadý: {ex.Message}", "Tamam");
        }
    }


    // Fark'a göre yukarý veya aþaðý ok belirleme
    private static string GetDirectionIcon(string Deðiþim)
    {
        if (string.IsNullOrWhiteSpace(Deðiþim)) return string.Empty;

        // Yüzdelik iþareti ve virgülü kaldýr, noktaya çevir
        Deðiþim = Deðiþim.Replace("%", "").Replace(",", ".");

        if (double.TryParse(Deðiþim, out double difference))
        {
            return difference >= 0 ? "up_arrow.png" : "down_arrow.png";
        }

        return string.Empty; // Hatalý veri durumunda boþ döner
    }
}
public class CurrencyApiResponse
{
    public string Update_Date { get; set; } = string.Empty;
    public Dictionary<string, Dictionary<string, string>> Rates { get; set; } = [];
}
// Döviz modeli
public class SelectedCurrency
{
    public string Name { get; set; } = string.Empty; // Döviz veya altýn adý
    public string Buying { get; set; } = "N/A";      // Alýþ fiyatý
    public string Selling { get; set; } = "N/A";     // Satýþ fiyatý
    public string Change { get; set; } = "N/A";      // Deðiþim oraný
    public string DirectionIcon { get; set; } = string.Empty; // Yukarý/Aþaðý ok simgesi
}