// Project: ClientList.cs
// Description: ClientList class prototype, used for storing deserialized XML data.
// Coded and owned by: Hok Uy
// Last Source Update: 27 May 2021

using System.Xml.Serialization;

namespace RagnarokInfo
{
    public class ClientInfo
    {
        public string Account { get; set; }
    }

    [XmlRootAttribute("ClientList")]
    public class ClientList
    {
        [XmlElement("ClientInfo")]
        public ClientInfo[] clientList { get; set; }
    }
}
