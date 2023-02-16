using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public interface INowRequirementRepository : IRepository<NowRequirement>
    {
        int Clear();
    }

    public class NowRequirementRepository : Repository<NowRequirement>, INowRequirementRepository
    {
        public NowRequirementRepository(TimsToolContext context) : base(context) { }

        int INowRequirementRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[NowRequirement]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
