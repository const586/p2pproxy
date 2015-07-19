using System.Xml.Serialization;
namespace PluginProxy
{
    /// <summary>
    /// ��������� ����������� ������� ��������
    /// </summary>
    [XmlRoot("item")]
    public interface IPluginContent
    {
        /// <summary>
        /// ID - ��������
        /// </summary>
        string Id { get; }
        /// <summary>
        /// ��������, �����������, �������
        /// </summary>
        string Title { get; }
        /// <summary>
        /// ������ ��������
        /// </summary>
        string Icon { get; }
        /// <summary>
        /// ��� ������
        /// </summary>
        PluginMediaType PluginMediaType { get; }
        /// <summary>
        /// ������������ ���������
        /// </summary>
        IPluginContainer Parent { get; }
        /// <summary>
        /// URL ��������
        /// </summary>
        string GetUrl(string host);
        /// <summary>
        /// ��� ����������
        /// </summary>
        TranslationType Translation { get; }
        /// <summary>
        /// � ������ ���� URL �� ��������������� ������ ��� ������������ �������� �����, ������������ ��� �������.
        /// </summary>
        /// <returns>���������� ��������� �������� �� ���������</returns>
        SourceUrl GetSourceUrl();
    }
}