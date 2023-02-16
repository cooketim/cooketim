using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{

    public interface IResultPromptWordGroupRepository : IRepository<ResultPromptWordGroup>
    {
        int Clear();
    }

    public class ResultPromptWordGroupRepository : Repository<ResultPromptWordGroup>, IResultPromptWordGroupRepository
    {
        public ResultPromptWordGroupRepository(TimsToolContext context) : base(context) { }

        int IResultPromptWordGroupRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[ResultPromptWordGroup]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
