using System.Collections.Generic;
using System.IO;

namespace PluginProxy
{
    /// <summary>
    /// ��������� ����������� ��������� IPluginProxy.HttpRequest
    /// </summary>
    public interface IRequestData
    {
        /// <summary>
        /// HTTP-���������
        /// </summary>
        Dictionary<string, string> Headers { get; }
        /// <summary>
        /// ����� ������������ ������ ����������
        /// </summary>
        /// <returns>����� ������</returns>
        Stream GetStream();
        /// <summary>
        /// ��� ���������� �������. �������� (200 ��)
        /// </summary>
        ushort ResultState { get; }
    }
}