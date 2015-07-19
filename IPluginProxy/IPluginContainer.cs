using System.Collections.Generic;
using System.Xml.Serialization;

namespace PluginProxy
{
    /// <summary>
    /// Интерфейс описывающий контейнер данных
    /// </summary>
    [XmlRoot("items")]
    public interface IPluginContainer : IPluginContent
    {
        /// <summary>
        /// Содержание контейнера
        /// </summary>
        IEnumerable<IPluginContent> Children { get; }
        /// <summary>
        /// Получить отсортированный список, по полю
        /// </summary>
        /// <param name="field">Поле сортировки</param>
        /// <returns></returns>
        IEnumerable<IPluginContent> OrderBy(string field);
        /// <summary>
        /// Параметр по которому прокси определяет, стоит ли вообще сортировать контейнер (Используется только в DLNA)
        /// </summary>
        bool CanSorted { get; }

    }
}