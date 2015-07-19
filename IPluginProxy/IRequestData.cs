using System.Collections.Generic;
using System.IO;

namespace PluginProxy
{
    /// <summary>
    /// Интерфейс описывающий результат IPluginProxy.HttpRequest
    /// </summary>
    public interface IRequestData
    {
        /// <summary>
        /// HTTP-заголовки
        /// </summary>
        Dictionary<string, string> Headers { get; }
        /// <summary>
        /// Метод возвращающий данные результата
        /// </summary>
        /// <returns>Поток данных</returns>
        Stream GetStream();
        /// <summary>
        /// Код результата запроса. Например (200 ОК)
        /// </summary>
        ushort ResultState { get; }
    }
}