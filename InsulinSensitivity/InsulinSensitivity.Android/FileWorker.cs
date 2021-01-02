using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using BusinessLogicLayer.Service.Interfaces;

[assembly: Dependency(typeof(InsulinSensitivity.Droid.FileWorker))]
namespace InsulinSensitivity.Droid
{
    public class FileWorker : IFileWorker
    {
        public Task DeleteAsync(string fileName)
        {
            File.Delete(GetFilePath(fileName));
            return Task.FromResult(true);
        }

        public Task<bool> ExistsAsync(string fileName)
        {
            string filePath = GetFilePath(fileName);
            bool exists = File.Exists(filePath);
            return Task.FromResult(exists);
        }

        public Task<IEnumerable<string>> GetFilesAsync()
        {
            IEnumerable<string> fileNames = Directory.EnumerateFiles(GetDocsPath())
                .Select(x =>
                    Path.GetFileName(x));
            return Task<IEnumerable<string>>.FromResult(fileNames);
        }

        public async Task<string> LoadTextAsync(string fileName)
        {
            string filePath = GetFilePath(fileName);
            using (StreamReader reader = new StreamReader(filePath, System.Text.Encoding.GetEncoding(1251)))
                return await reader.ReadToEndAsync();
        }

        public async Task SaveTextAsync(string fileName, string text)
        {
            string filePath = GetFilePath(fileName);
            using (StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.GetEncoding(1251)))
                await writer.WriteAsync(text);
        }
        
        /// <summary>
        /// Получает путь до файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns></returns>
        public string GetFilePath(string fileName) =>
            Path.Combine(GetDocsPath(), fileName);

        /// <summary>
        /// Получает путь до папки "Мои Документы"
        /// </summary>
        /// <returns></returns>
        private string GetDocsPath() =>
            Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
    }
}