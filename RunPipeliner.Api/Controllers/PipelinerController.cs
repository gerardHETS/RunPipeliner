using System.Collections.Generic;



namespace RunPipeliner.Controllers
{
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using RunPipeliner.DataAccess.PipelinerAPI;
    using RunPipeliner.Domain.BusinessInterfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class PipelinerController : Controller
    {     
        private readonly IPipelinerBusinessAccess _pipelinerBusinessAccessObject;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public PipelinerController(IPipelinerBusinessAccess pipelinerBusinessAccess, IConfiguration configuration, IMapper mapper)
        {
            _config = configuration;
            _mapper = mapper;
            _pipelinerBusinessAccessObject = pipelinerBusinessAccess;
        }

        [HttpPost, Route("run")]        
        public int RunPipeliner()
        {            
            return _pipelinerBusinessAccessObject.RunPipeliner();
        }

        [HttpPost, Route("run/v2")] 
        //public IEnumerable<string> RunPipelinerV2()
        public int RunPipelinerV2()
        {            
            return _pipelinerBusinessAccessObject.RunPipelinerV2();
        }
    }
}
