// Project: ClientList.cs
// Description: ClientList class prototype, used for storing deserialized XML data.
// Coded and owned by: Hok Uy
// Last Source Update: 05 Feb 2023

using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace RagnarokInfo
{
	[XmlRoot(ElementName = "Character")]
	public class Character_Addr
	{
		[XmlElement(ElementName = "Name")]
		public uint Name { get; set; }
		[XmlElement(ElementName = "BaseLevel")]
		public uint BaseLevel { get; set; }
		[XmlElement(ElementName = "BaseExp")]
		public uint BaseExp { get; set; }
		[XmlElement(ElementName = "BaseExpRequired")]
		public uint BaseExpRequired { get; set; }
		[XmlElement(ElementName = "JobLevel")]
		public uint JobLevel { get; set; }
		[XmlElement(ElementName = "JobExp")]
		public uint JobExp { get; set; }
		[XmlElement(ElementName = "JobExpRequired")]
		public uint JobExpRequired { get; set; }
	}

	[XmlRoot(ElementName = "Homunculus")]
	public class Homunculus_Addr
	{
		[XmlElement(ElementName = "Name")]
		public uint Name { get; set; }
		[XmlElement(ElementName = "Out")]
		public uint Out { get; set; }
		[XmlElement(ElementName = "Loyalty")]
		public uint Loyalty { get; set; }
		[XmlElement(ElementName = "Exp")]
		public uint Exp { get; set; }
		[XmlElement(ElementName = "ExpRequired")]
		public uint ExpRequired { get; set; }
		[XmlElement(ElementName = "Hunger")]
		public uint Hunger { get; set; }
	}

	[XmlRoot(ElementName = "Pet")]
	public class Pet_Addr
	{
		[XmlElement(ElementName = "Name")]
		public uint Name { get; set; }
		[XmlElement(ElementName = "Out")]
		public uint Out { get; set; }
		[XmlElement(ElementName = "Loyalty")]
		public uint Loyalty { get; set; }
		[XmlElement(ElementName = "Hunger")]
		public uint Hunger { get; set; }
	}

	[XmlRoot(ElementName = "Offsets")]
	public class Offsets
	{
		[XmlElement(ElementName = "LoggedIn")]
		public uint LoggedIn { get; set; }
		[XmlElement(ElementName = "Character")]
		public Character_Addr Character { get; set; }
		[XmlElement(ElementName = "Homunculus")]
		public Homunculus_Addr Homunculus { get; set; }
		[XmlElement(ElementName = "Pet")]
		public Pet_Addr Pet { get; set; }
	}

	[XmlRoot(ElementName = "ClientInfo")]
	public class ClientInfo
	{
		[XmlElement(ElementName = "Account")]
		public string Account { get; set; }
		[XmlElement(ElementName = "Offsets")]
		public Offsets Offsets { get; set; }
	}

	[XmlRoot(ElementName = "ClientList")]
	public class ClientList
	{
		[XmlElement(ElementName = "ClientInfo")]
		public ClientInfo Client { get; set; }
	}
}
