using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieInfo
{
	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class timestamp
	{
		private ulong timeField;
		private ulong delayField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ulong time
		{
			get
			{
				return this.timeField;
			}
			set
			{
				this.timeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ulong delay
		{
			get
			{
				return this.delayField;
			}
			set
			{
				this.delayField = value;
			}
		}
	}
}
