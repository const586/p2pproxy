namespace PluginProxy
{
    /// <summary>
    /// Тип источника. Используется для определения способ вещания контента
    /// </summary>
    public enum TranslationType
    {
        /// <summary>
        /// VLC Broadcasting
        /// </summary>
        Broadcast, 
        /// <summary>
        /// VLC VoD по протоколу RTSP
        /// </summary>
        VoD, 
        /// <summary>
        /// Внешний источник, или же контент вещает сам плагин
        /// </summary>
        Stream
    }
}