using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GorselProgOdev3;

public partial class NewsPage : ContentPage
{
    public NewsPage()
    {
        InitializeComponent();
        KategoriPicker.ItemsSource = Kategori.Liste.Select(k => k.Baslik).ToList();
    }

    private async void GetNewsButton_Clicked(object sender, EventArgs e)
    {
        if (KategoriPicker.SelectedIndex == -1)
        {
            await DisplayAlert("Uyarý", "Lütfen bir kategori seçin.", "Tamam");
            return;
        }

        var selectedKategori = Kategori.Liste[KategoriPicker.SelectedIndex];
        string url = $"https://api.rss2json.com/v1/api.json?rss_url={selectedKategori.Link}";

        using HttpClient client = new();
        try
        {
            var response = await client.GetStringAsync(url); // JSON yanýtýný alýyoruz

            // Deserialization iþlemi
            JsonSerializerOptions jsonSerializerOptions1 = new()
            {
                PropertyNameCaseInsensitive = true
            };
            JsonSerializerOptions options = jsonSerializerOptions1;
            var newsResponse = JsonSerializer.Deserialize<NewsResponse>(response, options);
            if (newsResponse != null && newsResponse.Items.Count > 0)
            {
                var items = newsResponse.Items.Select(i => new NewsItem(
                    i.Title,
                    i.Link,
                    i.PubDate,
                    i.Description,
                    i.Enclosure // JSON'daki enclosure nesnesi burada iþleniyor
                )).ToList();

                HaberListView.ItemsSource = items; // ListView'e haberleri baðla
            }
            else
            {
                await DisplayAlert("Uyarý", "Haber bulunamadý.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Bir hata oluþtu: {ex.Message}", "Tamam");
        }
    }
    public class Kategori(string baslik, string link)
    {
        public string Baslik { get; private set; } = baslik;
        public string Link { get; private set; } = link;

        // Statik liste tanýmý
        public static List<Kategori> Liste { get; } =
    [
        new Kategori("Manþet", "https://www.trthaber.com/manset_articles.rss"),
        new Kategori("Son Dakika", "https://www.trthaber.com/sondakika_articles.rss"),
        new Kategori("Gündem", "https://www.trthaber.com/gundem_articles.rss"),
        new Kategori("Ekonomi", "https://www.trthaber.com/ekonomi_articles.rss"),
        new Kategori("Spor", "https://www.trthaber.com/spor_articles.rss")
    ];
    }
    public class NewsResponse(string Status, List<NewsItem> Items)
    {
        public string Status { get; } = Status;
        public List<NewsItem> Items { get; } = Items;
    }

    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string PubDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Orijinal Description
        public string CleanDescription { get; set; } = string.Empty; // Temizlenmiþ Description
        public string ImageUrl { get; set; } = string.Empty;
        public JsonElement? Enclosure { get; internal set; }

        public NewsItem(string title, string link, string pubDate, string description, JsonElement? enclosure)
        {
            Title = title ?? string.Empty;
            Link = link ?? string.Empty;
            PubDate = pubDate ?? string.Empty;

            // Önce ImageUrl'yi ayarla
            ImageUrl = GetImageUrlFromEnclosure(enclosure, description);

            // Orijinal Description'ý sakla
            Description = description;

            // Temizlenmiþ Description'ý oluþtur
            CleanDescription = RemoveHtmlTags(description);
        }

        private static string GetImageUrlFromEnclosure(JsonElement? enclosure, string description)
        {
            if (enclosure != null && enclosure.Value.ValueKind == JsonValueKind.Object)
            {
                if (enclosure.Value.TryGetProperty("url", out JsonElement urlElement))
                {
                    var url = urlElement.GetString();
                    if (!string.IsNullOrEmpty(url))
                        return url;
                }
            }

            if (!string.IsNullOrEmpty(description) && description.Contains("img src="))
            {
                var startIndex = description.IndexOf("img src=\"") + 9;
                var endIndex = description.IndexOf('\"', startIndex);
                if (startIndex > 8 && endIndex > startIndex)
                {
                    return description[startIndex..endIndex];
                }
            }

            return string.Empty;
        }

        private static string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Regex.Replace(input, "<.*?>", string.Empty); // HTML etiketlerini temizler
        }
    }
}

