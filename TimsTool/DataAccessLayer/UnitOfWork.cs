using DataAccessLayer.Models;
using System;

namespace DataAccessLayer
{
    public interface IUnitOfWork : IDisposable
    {
        IResultDefinitionRepository ResultDefinitions { get; }
        IResultDefinitionRuleRepository ResultDefinitionRules { get; }
        IResultDefinitionWordGroupRepository ResultDefinitionWordGroups { get; }
        IResultPromptRepository ResultPrompts { get; }
        IResultPromptRuleRepository ResultPromptRules { get; }
        IResultPromptWordGroupRepository ResultPromptWordGroups { get; }
        IFixedListRepository FixedLists { get; }
        INowRepository Nows { get; }
        INowRequirementRepository NowRequirements { get; }
        INowSubscriptionRepository NowSubscriptions { get; }
        ICommentRepository Comments { get; }

        int Complete();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly TimsToolContext context;

        public UnitOfWork(TimsToolContext context)
        {
            this.context = context;
            ResultDefinitions = new ResultDefinitionRepository(context);
            ResultDefinitionRules = new ResultDefinitionRuleRepository(context);
            ResultDefinitionWordGroups = new ResultDefinitionWordGroupRepository(context);
            ResultPrompts = new ResultPromptRepository(context);
            ResultPromptRules = new ResultPromptRuleRepository(context);
            ResultPromptWordGroups = new ResultPromptWordGroupRepository(context);
            FixedLists = new FixedListRepository(context);
            Nows = new NowRepository(context);
            NowRequirements = new NowRequirementRepository(context);
            NowSubscriptions = new NowSubscriptionRepository(context);
            Comments = new CommentRepository(context);
        }

        public IResultDefinitionRepository ResultDefinitions { get; private set; }

        public IResultDefinitionRuleRepository ResultDefinitionRules { get; private set; }

        public IResultDefinitionWordGroupRepository ResultDefinitionWordGroups { get; private set; }

        public IResultPromptRepository ResultPrompts { get; private set; }

        public IResultPromptRuleRepository ResultPromptRules { get; private set; }

        public IResultPromptWordGroupRepository ResultPromptWordGroups { get; private set; }

        public IFixedListRepository FixedLists { get; private set; }

        public INowRepository Nows { get; private set; }

        public INowRequirementRepository NowRequirements { get; private set; }

        public INowSubscriptionRepository NowSubscriptions { get; private set; }

        public ICommentRepository Comments { get; private set; }

        public int Complete()
        {
            return context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
