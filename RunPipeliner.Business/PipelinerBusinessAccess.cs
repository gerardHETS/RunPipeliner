using RunPipeliner.DataAccess.PipelinerAPI;
using RunPipeliner.Domain.BusinessInterfaces;
using RunPipeliner.Domain.DataAccess;
using RunPipeliner.Domain.DataAccessInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunPipeliner.Business
{
    public class PipelinerBusinessAccess : IPipelinerBusinessAccess
    {
        private readonly IPipelinerDataAccess _pipelinerDataAccessObject;

        public PipelinerBusinessAccess(IPipelinerDataAccess pipelinerDataAccess)
        {
            _pipelinerDataAccessObject = pipelinerDataAccess;
        }

        public int RunPipeliner()
        {
            //return _pipelinerBusinessAccessObject.RunPipeliner();
            // Production
            string username = "cloud_LabXpress_xmeCbu4GoaJ56ogNwQ6PdwmnKqAO9gxuQNer0PgZ";
            string password = "m2BWXNtb2YzMKQVkjtGjXSceQItJIqex6Xe5oQRj";
            string teamPipelineID = "cloud_LabXpress";

            // Test
            username = "cloud_NewLabxpressTest_PBe7c3GcAvUjxPKvu2C1WbI4X7seOXO11dUAEw6Y";
            password = "RU1wTC70buZlV4xtc9UcRtxWL1PqkIarUJd6zeJG";
            teamPipelineID = "cloud_NewLabxpressTest";


            string serviceURL = "https://us-east.pipelinersales.com";
            PipelinerRestAPI service = new PipelinerRestAPI(serviceURL);
            service.setCredentials(username, password);


            List<AccountExportVM> accounts = GetAccountsForExport();
            List<AccountExportVM> vaccounts = PipelinerMonthlySalesQuery();
            accounts.AddRange(vaccounts);

            var recordcount = BuildAccountDataForExport(accounts, service, teamPipelineID);

            return recordcount;
        }

        public int RunPipelinerV2()
        {
            throw new NotImplementedException();
        }


        public List<AccountExportVM> GetAccountsForExport()
        {
            return _pipelinerDataAccessObject.GetAccountsForExport();
        }

        public List<AccountExportVM> PipelinerMonthlySalesQuery()
        {
            return _pipelinerDataAccessObject.PipelinerMonthlySalesQuery();
        }

        public int BuildAccountDataForExport(List<AccountExportVM>  accounts, PipelinerRestAPI service, string teamPipelineID)
        {
            return BuildAccountDataForExport(accounts, service, teamPipelineID);
        }


    }
}
