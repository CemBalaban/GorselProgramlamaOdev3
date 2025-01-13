using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using GorselProgOdev3.Models;
using Newtonsoft.Json;
using GorselProgOdev3.Services;
using System.Reflection.Metadata;
using Google.Cloud.Firestore;
using static GorselProgOdev3.AuthPage;

namespace GorselProgOdev3;

public partial class TodoPage : ContentPage
{
    public ObservableCollection<ToDo> ListTodo { get; set; } = new ObservableCollection<ToDo>();

    public TodoPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Baþlangýç görevleri
        ctrlTodoList.ItemsSource = ListTodo;
        _ = LoadTodosFromFirebase();
    }

    private void ShowAddTaskForm(object sender, EventArgs e)
    {
        // Görev ekleme formunu göster
        TaskForm.IsVisible = true;
        ctrlTodoList.IsVisible = false;
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        // Yeni görevi listeye ekle
        if (!string.IsNullOrWhiteSpace(txtTask.Text))
        {
            var newTodo = new ToDo
            {
                ID = Guid.NewGuid().ToString(), // Benzersiz bir ID oluþtur
                Task = txtTask.Text,
                Detail = txtDetails.Text,
                Date = datePicker.Date.ToString("yyyy-MM-dd") + " " + timePicker.Time.ToString(),
                IsDone = false
            };

            ListTodo.Add(newTodo);

            string? email = UserSession.Email;
            string? encodedEmail = (email ?? string.Empty).Replace(".", "_").Replace("@", "_at_");

            var db = FirestoreHelper.Database;
            var data = GetWriteData();

            if (db == null)
            {
                await DisplayAlert("Hata", "Firestore baðlantýsý yapýlandýrýlmamýþ.", "Tamam");
                return;
            }
            // Alt koleksiyona ekleme
            var todoRef = db.Collection("Users").Document(encodedEmail).Collection("Todos").Document(newTodo.ID);
            await todoRef.SetAsync(data);

            // Formu temizle
            txtTask.Text = string.Empty;
            txtDetails.Text = string.Empty;
            datePicker.Date = DateTime.Now;
            timePicker.Time = TimeSpan.Zero;

            // Formu gizle ve listeyi göster
            TaskForm.IsVisible = false;
            ctrlTodoList.IsVisible = true;
        }
        else
        {
            await DisplayAlert("Hata", "Görev adý boþ olamaz.", "Tamam");
        }
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        // Formu gizle ve listeyi göster
        TaskForm.IsVisible = false;
        ctrlTodoList.IsVisible = true;
    }

    private async void DeleteTodo_Click(object sender, EventArgs e)
    {
        var todo = ListTodo.FirstOrDefault(o => o.ID == ((SwipeItem)sender).CommandParameter.ToString());

        if (todo != null)
        {
            var res = await DisplayAlert("Silmeyi onayla", "Silinsin mi?", "Evet", "Hayýr");

            if (res)
            {
                // Firebase'den silme iþlemi
                await DeleteTodoFromFirebase(todo.ID);

                // ListTodo koleksiyonundan öðeyi çýkar
                ListTodo.Remove(todo);

                // Firebase'e kaydetme iþlemi sonrasý güncellenmiþ listeyi Firebase'e kaydedebiliriz.
                await SaveTodosToFirebase();

                await LoadTodosFromFirebase();
            }
        }
    }

    private async void EditTodo_Click(object sender, EventArgs e)
    {
        var todo = ListTodo.FirstOrDefault(o => o.ID == ((SwipeItem)sender).CommandParameter.ToString());

        if (todo != null)
        {
            var res = await DisplayPromptAsync("Düzenle", "Yapýlacak:", initialValue: todo.Task, placeholder: "Yapýlacaklarý buraya yaz");

            if (!string.IsNullOrEmpty(res))
            {
                todo.Task = res;
                await SaveTodosToFirebase(); // Firebase'e kaydet
            }
        }
    }

    private async void EditDetails_Click(object sender, EventArgs e)
    {
        var todo = ListTodo.FirstOrDefault(o => o.ID == ((SwipeItem)sender).CommandParameter.ToString());

        if (todo != null)
        {
            var res = await DisplayPromptAsync("Detay Düzenle", "Detay:", initialValue: todo.Detail, placeholder: "Detaylarý buraya yaz");

            if (!string.IsNullOrEmpty(res))
            {
                todo.Detail = res;
            }
        }
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        var checkBox = sender as CheckBox;

        if (checkBox?.BindingContext is ToDo todo)
        {
            todo.IsDone = e.Value;
        }
    }
    private UserData GetWriteData()
    {
        string task = txtTask.Text.Trim();
        string details = txtDetails.Text.Trim();
        string time = timePicker.Time.ToString().Trim();
        string date = datePicker.Date.ToString().Trim();

        return new UserData()
        {
            Task = task,
            Detail = details,
            Time = time,
            Date = date,
        };
    }
    private async Task SaveTodosToFirebase()
    {
        string? email = UserSession.Email;
        string? encodedEmail = (email ?? string.Empty).Replace(".", "_").Replace("@", "_at_");

        var db = FirestoreHelper.Database;

        var userData = new UserData
        {
            Tasks = ListTodo.ToList()
        };

        if (db == null)
        {
            await DisplayAlert("Hata", "Firestore baðlantýsý yapýlandýrýlmamýþ.", "Tamam");
            return;
        }

        var docRef = db.Collection("Users").Document(encodedEmail);
        await docRef.SetAsync(userData);
    }
    private async Task DeleteTodoFromFirebase(string todoId)
    {
        string? email = UserSession.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Hata", "Kullanýcý e-posta adresi boþ.", "Tamam");
            return;
        }

        string encodedEmail = email.Replace(".", "_").Replace("@", "_at_");

        var db = FirestoreHelper.Database;

        if (db == null)
        {
            await DisplayAlert("Hata", "Firestore baðlantýsý yapýlandýrýlmamýþ.", "Tamam");
            return;
        }

        // Kullanýcýya ait Todos koleksiyonu
        var todosRef = db.Collection("Users").Document(encodedEmail).Collection("Todos");

        try
        {
            // Belirtilen ID'ye sahip ToDo'yu almak
            var todoDocRef = todosRef.Document(todoId);
            var todoDoc = await todoDocRef.GetSnapshotAsync();

            if (todoDoc.Exists)
            {
                // ToDo'yu silme
                await todoDocRef.DeleteAsync();
                await DisplayAlert("Baþarýlý", "Görev baþarýyla silindi.", "Tamam");

                // Firebase'den baþarýlý bir þekilde silme iþlemi tamamlandý
            }
            else
            {
                await DisplayAlert("Uyarý", "Görev bulunamadý.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Bir hata oluþtu: {ex.Message}", "Tamam");
        }
    }
    private async Task LoadTodosFromFirebase()
    {
        try
        {
            // Kullanýcý e-posta adresini kontrol et
            string? email = UserSession.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Hata", "Kullanýcý e-posta adresi boþ.", "Tamam");
                return;
            }

            // E-posta adresini encode et
            string encodedEmail = email.Replace(".", "_").Replace("@", "_at_");

            // Firestore veritabanýna eriþ
            var db = FirestoreHelper.Database;
            if (db == null)
            {
                await DisplayAlert("Hata", "Firestore baðlantýsý yapýlandýrýlmamýþ.", "Tamam");
                return;
            }

            // Kullanýcýya ait Todos koleksiyonunu al
            var todosRef = db.Collection("Users").Document(encodedEmail).Collection("Todos");

            // Snapshot'ý al
            var snapshot = await todosRef.GetSnapshotAsync();

            // Eðer snapshot boþsa listeyi temizle ve çýk
            if (snapshot == null || snapshot.Documents.Count == 0)
            {
                ListTodo.Clear();
                await DisplayAlert("Bilgi", "Henüz yapýlacak bir þey bulunamadý.", "Tamam");
                return;
            }

            // Snapshot'tan verileri listeye ekle
            ListTodo.Clear();
            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var todo = document.ConvertTo<ToDo>();
                    if (todo != null)
                    {
                        ListTodo.Add(todo);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Hata mesajýný göster
            await DisplayAlert("Hata", $"Veriler yüklenirken bir hata oluþtu: {ex.Message}", "Tamam");
        }
    }
    private async void OnDeleteTodoClicked(object sender, EventArgs e)
    {
        var todo = ListTodo.FirstOrDefault(o => o.ID == ((SwipeItem)sender).CommandParameter.ToString());

        if (todo == null)
        {
            await DisplayAlert("Hata", "Hata", "Tamam");
            return;
        }
        var menuItem = (MenuItem)sender;
        var todoId = (string)menuItem.CommandParameter; // CommandParameter'dan ID'yi alýyoruz
        ListTodo.Remove(todo);

        await DeleteTodoFromFirebase(todoId); // ID ile silme iþlemi yapýyoruz
    }
}
