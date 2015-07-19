using System.Collections.Generic;
using System.Xml.Serialization;

namespace PluginProxy
{
    /// <summary>
    /// ��������� ����������� ��������� ������
    /// </summary>
    [XmlRoot("items")]
    public interface IPluginContainer : IPluginContent
    {
        /// <summary>
        /// ���������� ����������
        /// </summary>
        IEnumerable<IPluginContent> Children { get; }
        /// <summary>
        /// �������� ��������������� ������, �� ����
        /// </summary>
        /// <param name="field">���� ����������</param>
        /// <returns></returns>
        IEnumerable<IPluginContent> OrderBy(string field);
        /// <summary>
        /// �������� �� �������� ������ ����������, ����� �� ������ ����������� ��������� (������������ ������ � DLNA)
        /// </summary>
        bool CanSorted { get; }

    }
}