using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Google.Cloud.Firestore;

namespace GorselProgOdev3.Models
{
    [FirestoreData] // Firestore ile eşleşmeyi sağlar
    public partial class ToDo : INotifyPropertyChanged
    {
        private string? id;
        private string? task;
        private string? detail;
        private string? date;
        private string? time;
        private bool isdone;

        [FirestoreProperty] // Firestore'daki "ID" alanı ile eşleşir
        public string ID
        {
            get
            {
                id ??= Guid.NewGuid().ToString();
                return id;
            }
            set
            {
                id = value;
                NotifyPropertyChanged();
            }
        }

        [FirestoreProperty("Task")] // Firestore'daki "Text" alanı ile eşleşir
        public string Task
        {
            get { return task ?? string.Empty; }
            set
            {
                task = value;
                NotifyPropertyChanged();
            }
        }

        [FirestoreProperty("Detail")] // Firestore'daki "Details" alanı ile eşleşir
        public string Detail
        {
            get { return detail ?? string.Empty; }
            set
            {
                detail = value;
                NotifyPropertyChanged();
            }
        }

        [FirestoreProperty("Date")] // Firestore'daki "Date" alanı ile eşleşir
        public string Date
        {
            get { return date ?? string.Empty; }
            set
            {
                date = value;
                NotifyPropertyChanged();
            }
        }
        [FirestoreProperty("Time")] // Firestore'daki "Date" alanı ile eşleşir
        public string Time
        {
            get { return time ?? string.Empty; }
            set
            {
                time = value;
                NotifyPropertyChanged();
            }
        }

        [FirestoreProperty] // Firestore'daki "IsDone" alanı ile eşleşir
        public bool IsDone
        {
            get { return isdone; }
            set
            {
                isdone = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}