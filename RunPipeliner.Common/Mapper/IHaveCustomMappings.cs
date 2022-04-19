using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace RunPipeliner.Common.Mapper
{
    public interface IHaveCustomMappings
    {
        void CreateMappings(IMapperConfigurationExpression configuration);
    }
}

