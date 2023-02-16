using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{

    public interface IFixedListRepository : IRepository<FixedList>
    {
        int Clear();
    }

    public class FixedListRepository : Repository<FixedList>, IFixedListRepository
    {
        public FixedListRepository(TimsToolContext context) : base(context) { }

        int IFixedListRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[FixedList]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
