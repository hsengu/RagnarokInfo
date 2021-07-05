// Project: UserSettings.cs
// Description: UserSettings class prototype, used for storing deserialized XML data.
// Coded and owned by: Hok Uy
// Last Source Update: 27 May 2021

using System;
using System.Xml.Serialization;

namespace RagnarokInfo
{
    public class UserSettings
    {
        public int Opacity { get; set; }
        public String Filepath { get; set; }

        UserSettings()
        {
            Opacity = 50;
            Filepath = "C:\\";
        }

        public UserSettings(AppSettings setValue)
        {
            Opacity = setValue.appSettings.Opacity;
            Filepath = setValue.appSettings.Filepath;
        }
    }

    [XmlRootAttribute("AppSettings")]
    public class AppSettings
    {
        [XmlElement("UserSettings")]
        public UserSettings appSettings { get; set; }
    }
}
