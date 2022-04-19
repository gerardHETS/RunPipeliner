using RunPipeliner.Domain.BusinessInterfaces;
using RunPipeliner.Domain.DataAccess;
using RunPipeliner.Domain.DataAccessInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunPipeliner.DataAccess
{
    public class PipelinerDataAccess : IPipelinerDataAccess
    {
        public List<AccountExportVM> GetAccountsForExport()
        {
            throw new NotImplementedException();
        }

        public List<AccountExportVM> PipelinerMonthlySalesQuery()
        {
            throw new NotImplementedException();
        }

        public int RunPipeliner()
        {
            throw new NotImplementedException();
        }

        public int RunPipelinerV2()
        {
            throw new NotImplementedException();
        }

		/*
        public List<AccountExportVM> GetAccountsForExport()
        {
			

			//	List<Account> acts = new List<Account>();
			using (var db = new AccountDbContext())
			{
				// Gets the Current Day of the month
				DateTime dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

				// Set the time of the 'dtFrom' to '7:00' 
				TimeSpan ts = new TimeSpan(7, 00, 0);
				dtFrom = dtFrom.Date + ts;

				//	DateTime dtTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);					

				var acts = (from a in db.Accounts
							join c in db.Contacts
							on a.ID equals c.RecordID
							//where c.RecordID.Equals(a.ID)
							// && accountids.Contains(a.ID)
							where c.RecordType == "ACT"
					   //&& c.PrimaryRecord == 1
					   // Only Pull Accounts for the current month		
					   && (a.DateCreated >= dtFrom
						|| a.DateUpdated >= dtFrom)
						&& a.Active == 1
						&& a.AccountType == "PRC"
							select new AccountExportVM
							{
								ID = a.ID,
								AccountName = a.Name,
								//	 SalespersonNumber = c.PhoneNumber,
								State = c.State,
								Country = c.Country,
								Address = c.Address1,
								Email1 = c.Email,
								Phone1 = c.PhoneNumber,
								NationalProviderID = a.NationalProviderID,
								MainContact = c.Contactname
							}).ToList();
				return acts;
			}
			
		}

        public List<AccountExportVM> PipelinerMonthlySalesQuery()
        {
			
			using (var db = new AccountDbContext())
			using (var command = db.Database.GetDbConnection().CreateCommand())
			{
				command.CommandText = "select a.ID as 'AcctId', a.AccountType as 'AcctType'," +
						 " a.name, ContactName, concat(Address1, ' ', Address2) as Address, City, State, Zip1, Country," +
			 " PhoneNumber, FaxNumber, Email, a.SalesUserID, CurMonCnt, CurMon, CurMonCnt2, CurMon2, CurMonCnt3, CurMon3,CurMonCnt4," +
			  " CurMon4, CurMonCnt5,CurMon5,CurMonCnt6, CurMon6 from ll_account a " +
		  " Left Join ll_contact On a.Id = ll_contact.recordid and recordtype = 'ACT' " +
		   " Left Join(" +
			" select z.id," +
			 " Count(case when Month(o.Dateordered) = month(DATE_SUB(curdate(), INTERVAL 1 MONTH)) then o.id end) 'CurMonCnt'," +
			 " Count(case when Month(o.Dateordered) = month(DATE_SUB(curdate(), INTERVAL 2 MONTH)) then o.id end) 'CurMonCnt2'," +
			 " Count(case when Month(o.Dateordered) = month(DATE_SUB(curdate(), INTERVAL 3 MONTH)) then o.id end) 'CurMonCnt3'," +
			 " Count(case when Month(o.Dateordered) = month(DATE_SUB(curdate(), INTERVAL 4 MONTH)) then o.id end) 'CurMonCnt4'," +
			 " Count(case when Month(o.Dateordered) = month(DATE_SUB(curdate(), INTERVAL 5 MONTH)) then o.id end) 'CurMonCnt5'," +
			 " Count(case when Month(o.Dateordered) = month(DATE_SUB(curdate(), INTERVAL 6 MONTH)) then o.id end) 'CurMonCnt6'," +
			 " Sum(case when Month(o.Dateordered) = month(DATE_SUB(curdate(), INTERVAL 1 MONTH)) then o.Total else 0 end) 'CurMon'," +
			 " Sum(case when Month(o.Dateordered) = Month(DATE_SUB(curdate(), INTERVAL 2 MONTH)) then o.Total else 0 end) 'CurMon2'," +
			 " Sum(case when Month(o.Dateordered) = Month(DATE_SUB(curdate(), INTERVAL 3 MONTH)) then o.Total else 0 end) 'CurMon3'," +
			 " Sum(case when Month(o.Dateordered) = Month(DATE_SUB(curdate(), INTERVAL 4 MONTH)) then o.Total else 0 end) 'CurMon4'," +
			 " Sum(case when Month(o.Dateordered) = Month(DATE_SUB(curdate(), INTERVAL 5 MONTH)) then o.Total else 0 end) 'CurMon5'," +
			 " Sum(case when Month(o.Dateordered) = Month(DATE_SUB(curdate(), INTERVAL 6 MONTH)) then o.Total else 0 end) 'CurMon6'" +

			  " from ll_account z" +
			  " Left Join ll_order o On(z.Id = o.accountid or z.Id = o.practiceid) and o.active = '1' and " +
				  " o.dateordered between date_format(DATE_SUB(CURDATE(), INTERVAL 6 MONTH), '%Y-%m-01') and date_sub(date_format(CURDATE(), '%Y-%m-01'), interval 1 day)" +
			  " Where z.companyid = '1' and z.Active = '1' and(z.AccountType = 'ACT' or z.AccountType = 'PRC')" +
			  " Group By z.id " +
			" Limit 5000)  b on b.id = a.id" +
			 " Where a.companyid = '1' and a.Active = '1' and(a.AccountType = 'ACT' or a.AccountType = 'PRC') " +
			  " Group By a.id Order by a.AccountType, a.id Limit 5000;";


				List<AccountExportVM> pmList = new List<AccountExportVM>();

				db.Database.OpenConnection();
				using (var results = command.ExecuteReader())
				{
					while (results.Read())
					{
						AccountExportVM actexport = new AccountExportVM();
						actexport.ID = Convert.ToInt32(results["AcctId"]);
						actexport.AccountType = results["AcctType"].ToString();
						actexport.AccountName = results["name"].ToString();
						actexport.MainContact = results["ContactName"].ToString();
						actexport.Address = results["Address"].ToString();
						actexport.City = results["City"].ToString();
						actexport.State = results["State"].ToString();
						actexport.ZIP_CODE = results["Zip1"].ToString();
						actexport.Country = results["Country"].ToString();
						actexport.Phone3 = results["FaxNumber"].ToString();
						actexport.Email1 = results["Email"].ToString();
						actexport.Salesperson_Number = results["SalesUserID"].ToString();

						if (!String.IsNullOrEmpty(results["CurMonCnt"].ToString()))
						{
							actexport.Current_Month_Orders = results["CurMonCnt"].ToString();
						}
						if (!String.IsNullOrEmpty(results["CurMon"].ToString()))
						{
							actexport.Current_Month_Amount = Convert.ToDecimal(results["CurMon"]);
						}
						if (!String.IsNullOrEmpty(results["CurMonCnt2"].ToString()))
						{
							actexport.Previous_Month_Orders = results["CurMonCnt2"].ToString();
						}
						if (!String.IsNullOrEmpty(results["CurMon2"].ToString()))
						{
							actexport.Previous_Month_Amount = Convert.ToDecimal(results["CurMon2"]);
						}
						if (!String.IsNullOrEmpty(results["CurMonCnt3"].ToString()))
						{
							actexport.Month_3_Orders = results["CurMonCnt3"].ToString();
						}
						if (!String.IsNullOrEmpty(results["CurMon3"].ToString()))
						{
							actexport.Month_3_Amount = Convert.ToDecimal(results["CurMon3"]);
						}
						if (!String.IsNullOrEmpty(results["CurMonCnt4"].ToString()))
						{
							actexport.Month_4_Orders = results["CurMonCnt4"].ToString();
						}
						if (!String.IsNullOrEmpty(results["CurMon4"].ToString()))
						{
							actexport.Month_4_Amount = Convert.ToDecimal(results["CurMon4"]);
						}
						if (!String.IsNullOrEmpty(results["CurMonCnt5"].ToString()))
						{
							actexport.Month_5_Orders = results["CurMonCnt5"].ToString();
						}
						if (!String.IsNullOrEmpty(results["CurMon5"].ToString()))
						{
							actexport.Month_5_Amount = Convert.ToDecimal(results["CurMon5"]);
						}
						if (!String.IsNullOrEmpty(results["CurMonCnt6"].ToString()))
						{
							actexport.Month_6_Orders = results["CurMonCnt6"].ToString();
						}
						if (!String.IsNullOrEmpty(results["CurMon6"].ToString()))
						{
							actexport.Month_6_Amount = Convert.ToDecimal(results["CurMon6"]);
						}

						pmList.Add(actexport);
					}
				}
				return pmList;
			}
		}
		*/
			
    }
}
