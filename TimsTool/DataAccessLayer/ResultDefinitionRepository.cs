using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public interface IResultDefinitionRepository : IRepository<ResultDefinition>
    {
        int Clear();
    }

    public class ResultDefinitionRepository : Repository<ResultDefinition>, IResultDefinitionRepository
    {
        public ResultDefinitionRepository(TimsToolContext context) : base(context) { }

        int IResultDefinitionRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[ResultDefinition]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
