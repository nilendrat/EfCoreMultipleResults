
using Microsoft.EntityFrameworkCore;

namespace EfCoreMultipleResults.SampleContext
{
    public partial class SampleEntities : DbContext
    {
        //scafolded db context
        protected SampleEntities()
        {
        }
        public SampleEntities(DbContextOptions<SampleEntities> options) : base(options)
        {
        }
    }
}
