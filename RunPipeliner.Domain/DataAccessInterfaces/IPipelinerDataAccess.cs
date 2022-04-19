using RunPipeliner.Domain.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunPipeliner.Domain.DataAccessInterfaces
{
    public interface IPipelinerDataAccess
    {
        int RunPipeliner();

        int RunPipelinerV2();

        public List<AccountExportVM> GetAccountsForExport();

        public List<AccountExportVM> PipelinerMonthlySalesQuery();
    }
}
