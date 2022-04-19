using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RunPipeliner.DataAccess.EntityModels
{
	[Table("ll_contact")]
	public class Contact
	{
		#region Properties	
		public int ID { get; set; }

		public int RecordID { get; set; }
		public string RecordType { get; set; }
		public int PrimaryRecord { get; set; }
		public string ContactTitle { get; set; }
		public string Contactname { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip1 { get; set; }
		public string Zip2 { get; set; }
		public string Country { get; set; }
		public string PhoneNumber { get; set; }
		public string PhoneExtension { get; set; }
		public string FaxNumber { get; set; }
		public string Email { get; set; }
		public int SendResults { get; set; }
		public int Newsletter { get; set; }
		public int Active { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateUpdated { get; set; }
		public int CreatedBy { get; set; }
		public int UpdatedBy { get; set; }

		#endregion
	}
}
