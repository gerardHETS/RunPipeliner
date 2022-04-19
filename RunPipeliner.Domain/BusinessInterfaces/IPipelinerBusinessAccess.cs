using System;
using System.Collections.Generic;
using System.Text;

namespace RunPipeliner.Domain.BusinessInterfaces
{
    public interface IPipelinerBusinessAccess
    {
        int RunPipeliner();

        int RunPipelinerV2();
    }
}
