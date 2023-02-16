using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public interface ICommentRepository : IRepository<Comment>
    {
        int Clear();
    }

    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public CommentRepository(TimsToolContext context) : base(context) { }

        int ICommentRepository.Clear()
        {
            return ExecuteCommand("delete from [dbo].[Comment]");
        }

        public TimsToolContext TimsToolContext
        {
            get { return Context as TimsToolContext; }
        }
    }
}
