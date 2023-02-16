using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public interface INowRepository : IRepository<Now>
    {
        int Clear();
    }

    public class NowRepository : Repository<Now>, INowRepository
    {
        public NowRepository(TimsToolContext context) : base(context) { }

        int INowRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[Now]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
