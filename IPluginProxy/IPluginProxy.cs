using System;
using System.Collections.Generic;

namespace PluginProxy
{
    public delegate void LoggerCallback(IPluginProxy sender, string message);
    /// <summary>
    /// Интерфейс описывающий плагин
    /// </summary>
    public interface IPluginProxy : IDisposable
    {
        /// <summary>
        /// ID плагина. Именно он используется для доступа по HTTP или UPnP
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Имя, описывающее, плагин
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Лог
        /// </summary>
        event LoggerCallback Logger;
        /// <summary>
        /// Инициализация первоначальных данных плагина. Вызывается сразу после его загрузки в память
        /// </summary>
        /// <param name="host">IP-адресс:Порт прокси сервера</param>
        void Init(string host);
        /// <summary>
        /// Получить список дополнительных относительных адресов, которые нужно подключить к WEB-серверу прокси.
        /// </summary>
        /// <returns>Коллекция адресов</returns>
        IEnumerable<string> GetRouteUrls();
        /// <summary>
        /// Метод вызываемый в случае запроса со стороны клиента внутреннего контента плагина
        /// </summary>
        /// <param name="path">относительный адрес запроса</param>
        /// <param name="parameters">GET-параметры запроса</param>
        /// <returns>Результат выполнения запроса</returns>
        IRequestData HttpRequest(string path, Dictionary<string, string> parameters);
        /// <summary>
        /// Вызывается сервером UPnP и в случае запроса корневого адреса плагина, для формирования плейлиста средствами прокси.
        /// </summary>
        /// <returns>Контейнер контента</returns>
        IPluginContent GetContent(Dictionary<string, string> parameters);
        /// <summary>
        /// Получить список меню для GUI интерфейса
        /// </summary>
        /// <returns>Список меню</returns>
        IEnumerable<string> GetMenus();
        /// <summary>
        /// Процедура вызываемая при нажатии меню
        /// </summary>
        /// <param name="menu"></param>
        void ClickMenu(string menu);
        
    }
}
