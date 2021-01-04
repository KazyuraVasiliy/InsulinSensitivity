using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace BusinessLogicLayer.Service
{
    public static class Permission
    {
        /// <summary>
        /// Запрос разрешения на запись файла
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/ru-ru/xamarin/essentials/permissions?tabs=android
        /// </remarks>
        /// <returns></returns>
        public static async Task<PermissionStatus> CheckAndRequestStorageWritePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                return status;
            }

            status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            return status;
        }
    }
}
