using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Service.Interfaces
{
    public interface IFileWorker
    {
        /// <summary>
        /// Проверяет существования файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string fileName);

        /// <summary>
        /// Записывает текст в файл
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="text">Строка</param>
        /// <returns></returns>
        Task SaveTextAsync(string fileName, string text);

        /// <summary>
        /// Копирует файл
        /// </summary>
        /// <param name="sourcePath">Исходное расположение файла</param>
        /// <param name="destinationPath">Целевое расположение копии</param>
        /// <returns></returns>
        Task CopyAsync(string sourcePath, string destinationPath);

        /// <summary>
        /// Создаёт новую директорию
        /// </summary>
        /// <param name="directoryName">Имя директории</param>
        /// <returns></returns>
        Task CreateDirectoryAsync(string directoryName);

        /// <summary>
        /// Загружает текст из файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns></returns>
        Task<string> LoadTextAsync(string fileName);

        /// <summary>
        /// Получает список файлов каталога
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetFilesAsync();

        /// <summary>
        /// Удаляет файл
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns></returns>
        Task DeleteAsync(string fileName);

        /// <summary>
        /// Получает путь до файла во внешнем хранилище
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns></returns>
        string GetPath(string fileName);
    }
}
