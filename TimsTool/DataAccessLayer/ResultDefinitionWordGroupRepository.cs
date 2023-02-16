using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public interface IResultDefinitionWordGroupRepository : IRepository<ResultDefinitionWordGroup>
    {
        int Clear();
    }

    public class ResultDefinitionWordGroupRepository : Repository<ResultDefinitionWordGroup>, IResultDefinitionWordGroupRepository
    {
        public ResultDefinitionWordGroupRepository(TimsToolContext context) : base(context) { }

        int IResultDefinitionWordGroupRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[ResultDefinitionWordGroup]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
