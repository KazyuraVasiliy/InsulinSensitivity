using BusinessLogicLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BusinessLogicLayer.Service
{
    public static class CopyService
    {
        /// <summary>
        /// Копирует public свойства уровня экземпляра
        /// </summary>
        /// <typeparam name="T">Destination</typeparam>
        /// <param name="obj">Source</param>
        /// <param name="destination">Destination</param>
        /// <returns>(instance, errors)</returns>
        public static (T instance, Dictionary<string, string> errors) Copy<T>(object obj, T destination = null, string destinationModel = null, string sourceModel = null) where T : class, new()
        {
            var bindingAttr = BindingFlags.Public | BindingFlags.Instance;

            var destinationProperties = typeof(T).GetProperties(bindingAttr)
                .Where(x =>
                    !x.GetAccessors()
                        .Any(y => y.IsVirtual));

            if (destinationModel != null)
                destinationProperties = destinationProperties
                    .Where(x =>
                        x.GetCustomAttribute<ModelAttribute>()?.Name == destinationModel);

            Type sourceType = obj.GetType();

            T instance = destination ?? new T();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            foreach (var property in destinationProperties)
            {
                try
                {
                    var sourceProperty = sourceType.GetProperty(property.Name, bindingAttr);

                    if (sourceProperty != null)
                    {
                        if (sourceModel != null && sourceProperty.GetCustomAttribute<ModelAttribute>()?.Name != sourceModel)
                            sourceProperty = null;

                        if (sourceProperty != null)
                            property.SetValue(instance, sourceProperty.GetValue(obj));
                    }   
                }
                catch (Exception ex)
                {
                    errors.Add(property.Name, ex.Message);
                }
            }

            return (instance, errors);
        }
    }
}
