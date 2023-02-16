using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public interface IResultDefinitionRuleRepository : IRepository<ResultDefinitionRule>
    {
        int Clear();
    }

    public class ResultDefinitionRuleRepository : Repository<ResultDefinitionRule>, IResultDefinitionRuleRepository
    {
        public ResultDefinitionRuleRepository(TimsToolContext context) : base(context) { }

        int IResultDefinitionRuleRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[ResultDefinitionRule]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
