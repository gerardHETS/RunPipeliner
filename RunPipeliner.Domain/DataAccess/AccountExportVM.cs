using System;
using System.Collections.Generic;
using System.Text;

namespace RunPipeliner.Domain.DataAccess
{
	public class AccountExportVM
	{
		#region Properties
		public int ID { get; set; }
		public string AccountName { get; set; }
		public string State { get; set; }
		public string Country { get; set; }
		public string MainContact { get; set; }
		public string Salesperson_Number { get; set; }
		// Primary Phone
		public string Phone1 { get; set; }
		// Secondary Phone
		public string Phone2 { get; set; }
		// Fax Number
		public string Phone3 { get; set; }
		public string OwnerID { get; set; } = "49613";
		public string Address { get; set; }
		public string ZIP_CODE { get; set; }
		public string Email1 { get; set; }
		public string City { get; set; }
		public string SalesUnitID { get; set; } = "0";
		public string NationalProviderID { get; set; }
		public string AccountType { get; set; }

		// Curr Month data
		public decimal Current_Month_Amount { get; set; }
		public decimal Previous_Month_Amount { get; set; }

		public decimal Month_3_Amount { get; set; }
		public decimal Month_4_Amount { get; set; }
		public decimal Month_5_Amount { get; set; }
		public decimal Month_6_Amount { get; set; }
		public string Current_Month_Orders { get; set; }
		public string Previous_Month_Orders { get; set; }
		public string Month_3_Orders { get; set; }
		public string Month_4_Orders { get; set; }
		public string Month_5_Orders { get; set; }
		public string Month_6_Orders { get; set; }

		public decimal cf_current_month { get; set; }
		public decimal cf_previous_month { get; set; }
		public decimal cf_month2n1 { get; set; }
		public decimal cf_month3n { get; set; }
		public decimal cf_month4n { get; set; }
		public decimal cf_month5n { get; set; }
		public decimal cf_month6n { get; set; }

		public string cf_current_month_orders { get; set; }
		public string cf_previous_month_orders { get; set; }
		public string cf_month2_orders { get; set; }
		public string cf_month3_orders { get; set; }
		public string cf_month4_orders { get; set; }
		public string cf_month5_orders { get; set; }
		public string cf_month6_orders { get; set; }


		#endregion
	}
}
