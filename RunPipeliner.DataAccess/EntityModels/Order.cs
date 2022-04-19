using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RunPipeliner.DataAccess.EntityModels
{
	[Table("ll_order")]
	public class Order
	{
		public int ID { get; set; }
		public int PatientID { get; set; }
		public string AccountName { get; set; }
		public int AccountID { get; set; }
		public int CompanyID { get; set; }
		public DateTime DateCollected { get; set; }
		public TimeSpan TimeCollected { get; set; }
		public string PatientGender { get; set; }
		public DateTime PatientDOB { get; set; }
		public int? OldOrderID { get; set; }
		public int? ParentOrderID { get; set; }
		public int LocationID { get; set; }
		public string OrderType { get; set; }
		public DateTime? DateOrdered { get; set; }
		public TimeSpan TimeOrdered { get; set; }
		public DateTime? DateReceived { get; set; }
		public string PatientIssuedID { get; set; }
		public string PatientFirstName { get; set; }
		public string PatientLastName { get; set; }
		public string PatientMI { get; set; }
		public string PatientAddress1 { get; set; }
		public string PatientAddress2 { get; set; }
		public string PatientCity { get; set; }
		public string PatientState { get; set; }
		public string PatientZip1 { get; set; }
		public string PatientZip2 { get; set; }
		public string PatientCountry { get; set; }
		public string PatientSSN { get; set; }
		[DisplayFormat(DataFormatString = "{0:###-###-####}")]
		public string PatientPhone { get; set; }
		public string PatientPhoneExtension { get; set; }
		public string PatientFax { get; set; }
		public string PatientEmail { get; set; }
		public string PatientFasting { get; set; }
		public string DoctorFirstName { get; set; }
		public string DoctorLastName { get; set; }
		public decimal SubTotal { get; set; }
		public decimal SalesTax { get; set; }
		public decimal TaxRate { get; set; }
		public decimal Total { get; set; }
		public decimal Balance { get; set; }
		public int? PromoID { get; set; }
		public string PromoName { get; set; }
		public decimal PromoPrice { get; set; }
		public decimal DiscountAmount { get; set; }
		public int? DoctorID { get; set; }
		public string ExternalOrderNumber { get; set; }
		public int CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }

		public Account Account { get; set; }

	}
}
