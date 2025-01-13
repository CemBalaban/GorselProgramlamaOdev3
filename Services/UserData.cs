using Google.Cloud.Firestore;
using GorselProgOdev3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GorselProgOdev3.Services
{
    [FirestoreData]
    internal class UserData
    {
        public List<ToDo> Tasks { get; set; } = new List<ToDo>();
        [FirestoreProperty]
        public string? Task { get; set; }
        [FirestoreProperty] 
        public string? Detail { get; set; }
        [FirestoreProperty] 
        public string? Date { get; set; }
        [FirestoreProperty] 
        public string? Time { get; set; }
        [FirestoreProperty]
        public Boolean IsChecked { get; set; }
    }
}
