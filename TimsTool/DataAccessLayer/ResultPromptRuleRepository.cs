using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{

    public interface IResultPromptRuleRepository : IRepository<Models.ResultPromptRule>
    {
        IEnumerable<Models.ResultPromptRule> Find(IEnumerable<DataLib.ResultPromptRule> rules);
        int Clear();
    }

    public class ResultPromptRuleRepository : Repository<Models.ResultPromptRule>, IResultPromptRuleRepository
    {
        public ResultPromptRuleRepository(TimsToolContext context) : base(context) { }

        int IResultPromptRuleRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[ResultPromptRule]");
        }

        public IEnumerable<Models.ResultPromptRule> Find(IEnumerable<DataLib.ResultPromptRule> rules)
        {
            //return (from rpr in TimsToolContext.ResultPromptRule
            //               join r in rules on
            //                                   new { rdId = rpr.ResultDefinitionUuid, rpId = rpr.ResultPromptUuid }
            //                                   equals
            //                                   new { rdId = r.ResultDefinitionUUID.Value, rpId = r.ResultPromptUUID.Value }
            //               select rpr).ToList();

            var entitiesByPrompt = Find(x => rules.Select(y => y.ResultPromptUUID).Contains(x.ResultPromptUuid)).ToList();

            var entities = entitiesByPrompt.FindAll(x => rules.Select(y => y.ResultDefinitionUUID).Contains(x.ResultDefinitionUuid)).ToList();

            return entities;
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
