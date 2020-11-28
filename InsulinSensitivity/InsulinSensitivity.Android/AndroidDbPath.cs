using System;
using System.IO;
using Xamarin.Forms;
using DataAccessLayer.Contexts;
using InsulinSensitivity.Droid;

[assembly: Dependency(typeof(AndroidDbPath))]
namespace InsulinSensitivity.Droid
{ 
    public class AndroidDbPath : IPath
    {
        public string GetDatabasePath(string fileName) =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName);
    }
}