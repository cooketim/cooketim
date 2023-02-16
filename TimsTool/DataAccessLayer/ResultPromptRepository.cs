using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{

    public interface IResultPromptRepository : IRepository<ResultPrompt>
    {
        int Clear();
    }

    public class ResultPromptRepository : Repository<ResultPrompt>, IResultPromptRepository
    {
        public ResultPromptRepository(TimsToolContext context) : base(context) { }

        int IResultPromptRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[ResultPrompt]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
