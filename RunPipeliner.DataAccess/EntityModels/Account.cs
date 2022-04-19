using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunPipeliner.DataAccess.EntityModels
{
	[Table("ll_account")]
	public class Account
	{

		public int ID { get; set; }
		public string Name { get; set; }
		public string NationalProviderID { get; set; }
		public int SalesuserID { get; set; }
		public int Active { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateUpdated { get; set; } = DateTime.Now;
		public string AccountType { get; set; }

	}
}
