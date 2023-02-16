using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public interface INowSubscriptionRepository : IRepository<NowSubscription>
    {
        int Clear();
    }

    public class NowSubscriptionRepository : Repository<NowSubscription>, INowSubscriptionRepository
    {
        public NowSubscriptionRepository(TimsToolContext context) : base(context) { }

        int INowSubscriptionRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[NowSubscription]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
