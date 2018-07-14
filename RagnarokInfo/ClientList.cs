// Project: ClientList.cs
// Description: ClientList class prototype, used for storing deserialized XML data.
// Coded and owned by: Hok Uy
// Last Source Update: 1 December 2017 at 12:57

using System.Xml.Serialization;

namespace RagnarokInfo
{
    public class ClientInfo
    {
        public string Name { get; set; }
        public string LoggedIn { get; set; }
        public string Account { get; set; }
        public string HomuName { get; set; }
        public string PetName { get; set; }
    }

    [XmlRootAttribute("ClientList")]
    public class ClientList
    {
        [XmlElement("ClientInfo")]
        public ClientInfo[] clientList { get; set; }
    }
}
