// Project: ClientList.cs
// Description: ClientList class prototype, used for storing deserialized XML data.
// Coded and owned by: Hok Uy
// Last Source Update: 6 May 2017 at 03:34

using System.Xml.Serialization;

namespace RagnarokInfo
{
    public class ClientInfo
    {
        public string Name { get; set; }
        public string BaseExp { get; set; }
        public string JobExp { get; set; }
        public string BaseLvl { get; set; }
        public string JobLvl { get; set; }
        public string BaseRequired { get; set; }
        public string JobRequired { get; set; }
        public string LoggedIn { get; set; }
        public string Account { get; set; }
        public string HomuName { get; set; }
        public string HomuLoyalty { get; set; }
        public string HomuHunger { get; set; }
        public string HomuExp { get; set; }
        public string HomuRequired { get; set; }
        public string HomuOut { get; set; }
        public string PetName { get; set; }
        public string PetLoyalty { get; set; }
        public string PetHunger { get; set; }
        public string PetOut { get; set; }
    }

    [XmlRootAttribute("ClientList")]
    public class ClientList
    {
        [XmlElement("ClientInfo")]
        public ClientInfo[] clientList { get; set; }
    }
}
