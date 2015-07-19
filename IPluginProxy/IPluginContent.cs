using System.Xml.Serialization;
namespace PluginProxy
{
    /// <summary>
    /// Интерфейс описывающий единицу контента
    /// </summary>
    [XmlRoot("item")]
    public interface IPluginContent
    {
        /// <summary>
        /// ID - элемента
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Название, описывающее, элемент
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Иконка элемента
        /// </summary>
        string Icon { get; }
        /// <summary>
        /// Тип данных
        /// </summary>
        PluginMediaType PluginMediaType { get; }
        /// <summary>
        /// Родительский контэйнер
        /// </summary>
        IPluginContainer Parent { get; }
        /// <summary>
        /// URL контента
        /// </summary>
        string GetUrl(string host);
        /// <summary>
        /// Тип трансляции
        /// </summary>
        TranslationType Translation { get; }
        /// <summary>
        /// В случае если URL на воспроизведение нельзя или нежелательно получить сразу, используется эта функция.
        /// </summary>
        /// <returns>Возвращает подробные сведения об источнике</returns>
        SourceUrl GetSourceUrl();
    }
}