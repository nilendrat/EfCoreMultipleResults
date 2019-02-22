using EfCoreMultipleResults.Domain;
using EfCoreMultipleResults.Model;
using EfCoreMultipleResults.Parameters;
using EfCoreMultipleResults.SampleContext;
using System;
using System.Collections.Generic;

namespace EfCoreMultipleResults
{
    public class DataAccess
    {
        private readonly IParameterMapper _parameterMapper;

        public DataAccess(IParameterMapper parameterMapper)
        {

            _parameterMapper = parameterMapper;
        }

        public void GetDBResults()
        {
            //create an instance of the dbcontext
            SampleEntities sampleEntities = new SampleEntities(new Microsoft.EntityFrameworkCore.DbContextOptions<SampleEntities>());


            //setting up parameters
            UserParam1 param1 = new UserParam1()
            {
                UserId = 1
            };

            var spParams = _parameterMapper.Map(param1);

            //method one
            var results1 = sampleEntities.QueryMultipleResults(sql: "YOUR_STORE_PROC_NAME").ExecuteMultipleResults(spParams.Parameters, typeof(Result1), typeof(Result2));




        }
    }
}
