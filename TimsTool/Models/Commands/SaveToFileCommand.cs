using System;
using Models.ViewModels;
using System.Windows.Input;
using System.Runtime.Serialization;
using Serilog;
using System.Threading;
using DataLib;
using System.Collections.Generic;
using System.Linq;
using DataLib.DataModel;

namespace Models.Commands
{
    public interface ISaveToFileCommand : ICommand { }

    public class SaveToFileCommand : ISaveToFileCommand
    {
        private readonly ITreeModel treeModel;
        public SaveToFileCommand(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            // I intentionally left these empty because
            // this command never raise the event, and
            // not using the WeakEvent pattern here can
            // cause memory leaks.  WeakEvent pattern is
            // not simple to implement, so why bother.
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {

            //first resequence
            treeModel.ResequenceCommand.Execute(null);

            SaveToFile(parameter);
        }

        private void SaveToFile(object parameter)
        {
            var filePath = GetFilePath(parameter);
            if (string.IsNullOrEmpty(filePath)) { return; }

            RebuildMasterCollections();

            for (var i = 0; i < 5; i++)
            {
                try
                {                    
                    //serialise without the map to temp file first, then replace the destination file
                    TreeModel.EncryptAndSerialize(filePath, treeModel.AllData);
                    Log.Information("Saved data file");
                    break;
                }
                catch (SerializationException e)
                {
                    Log.Error(e, "Failed to serialize. Reason: " + e.Message);
                    throw;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to serialize. Reason: " + e.Message);
                    Log.Information("Sleeping for one second, then trying again");
                    Thread.Sleep(1000);
                }
            }
        }

        private void RebuildMasterCollections()
        {
            var newData = new AllData()
            {
                ResultDefinitionRules = new List<ResultDefinitionRule>(),
                ResultDefinitions = new List<ResultDefinition>(),
                ResultDefinitionWordGroups = new List<ResultDefinitionWordGroup>(),
                ResultPromptWordGroups = new List<ResultPromptWordGroup>(),
                Nows = new List<Now>(),
                NowRequirements = new List<NowRequirement>(),
                NowSubscriptions = new List<NowSubscription>(),
                ResultPromptRules = new List<ResultPromptRule>(),
                FixedLists = new List<FixedList>(),
                ResultDefinitionSynonymCollectionId = treeModel.AllData.ResultDefinitionSynonymCollectionId,
                ResultPromptSynonymCollectionId = treeModel.AllData.ResultPromptSynonymCollectionId,
                Comments = new List<Comment>(treeModel.AllData.Comments)
            };

            var prompts = new List<ResultPrompt>();
            foreach (var rd in treeModel.AllData.ResultDefinitions)
            {
                newData.ResultDefinitions.Add(rd);
                if (rd.ResultDefinitionRules != null)
                {
                    rd.ResultDefinitionRules.RemoveAll(x => x.ResultDefinition == null || x.ParentUUID != rd.UUID);
                    newData.ResultDefinitionRules.AddRange(rd.ResultDefinitionRules);
                }
                
                if (rd.ResultPrompts != null)
                {
                    rd.ResultPrompts.RemoveAll(x => x.ResultDefinitionUUID != rd.UUID);
                    newData.ResultPromptRules.AddRange(rd.ResultPrompts);

                    foreach(var prompt in rd.ResultPrompts)
                    {
                        var matchedPrompt = prompts.FirstOrDefault(x => x.UUID == prompt.ResultPrompt.UUID);
                        if(matchedPrompt == null)
                        {
                            prompts.Add(prompt.ResultPrompt);
                        }

                        if (prompt.ResultPrompt.FixedList != null)
                        {
                            var match = newData.FixedLists.FirstOrDefault(x => x.UUID == prompt.ResultPrompt.FixedList.UUID);
                            if (match == null)
                            {
                                newData.FixedLists.Add(prompt.ResultPrompt.FixedList);
                            }
                            else
                            {
                                prompt.ResultPrompt.FixedList = match;
                            }
                        }

                        if (prompt.ResultPrompt.ResultPromptWordGroups != null)
                        {
                            var newRpwgs = new List<ResultPromptWordGroup>();
                            foreach (var rpwg in prompt.ResultPrompt.ResultPromptWordGroups)
                            {
                                var matchRpwg = newData.ResultPromptWordGroups.FirstOrDefault(x => x.UUID == rpwg.UUID);
                                if (matchRpwg == null)
                                {
                                    newData.ResultPromptWordGroups.Add(rpwg);
                                    newRpwgs.Add(rpwg);
                                }
                                else
                                {
                                    newRpwgs.Add(matchRpwg);
                                }
                            }
                            if (newRpwgs.Any())
                            {
                                prompt.ResultPrompt.ResultPromptWordGroups = newRpwgs;
                            }
                            else
                            {
                                prompt.ResultPrompt.ResultPromptWordGroups = null;
                            }
                        }
                    }
                }

                if (rd.WordGroups != null)
                {
                    rd.WordGroups.RemoveAll(x => x.ResultDefinitionWordGroups == null || !x.ResultDefinitionWordGroups.Any());
                    foreach (var rwg in rd.WordGroups)
                    {
                        rwg.ResultDefinition = rd;
                        var newRdwgs = new List<ResultDefinitionWordGroup>();
                        foreach (var rdwg in rwg.ResultDefinitionWordGroups)
                        {
                            var match = newData.ResultDefinitionWordGroups.FirstOrDefault(x => x.UUID == rdwg.UUID);
                            if (match == null)
                            {
                                newData.ResultDefinitionWordGroups.Add(rdwg);
                                newRdwgs.Add(rdwg);
                            }
                            else
                            {
                                newRdwgs.Add(match);
                            }
                        }
                        rwg.ResultDefinitionWordGroups = newRdwgs;
                    }
                }
            }

            //repoint result prompt references
            foreach(var item in newData.ResultPromptRules)
            {
                var matchedPrompt = prompts.First(x => x.UUID == item.ResultPromptUUID);
                item.ResultPrompt = matchedPrompt;
            }

            //rebuild nows and now requirements
            foreach (var now in treeModel.AllData.Nows)
            {
                newData.Nows.Add(now);
                newData.NowRequirements.AddRange(now.AllNowRequirements);
            }

            //repoint result definition references on the rules
            foreach(var item in newData.ResultDefinitionRules)
            {
                var matchedRd = newData.ResultDefinitions.First(x => x.UUID == item.ChildResultDefinitionUUID);
                item.ResultDefinition = matchedRd;
            }

            //repoint result definition references on now requirements
            foreach (var nrGroup in newData.NowRequirements.GroupBy(x=>x.ResultDefinitionUUID))
            {
                var matchedRd = newData.ResultDefinitions.First(x => x.UUID == nrGroup.Key);
                foreach(var nr in nrGroup)
                {
                    nr.SetResultDefinitionAndPromptRules(matchedRd);
                }
            }

            //rebuild now subscriptions, repointing as required
            foreach(var item in treeModel.AllData.NowSubscriptions)
            {
                newData.NowSubscriptions.Add(item);
                if(item.SubscriptionVocabulary != null)
                {                    
                    if(item.SubscriptionVocabulary.ExcludedResults != null)
                    {
                        var newExcludedResults = new List<ResultDefinition>();
                        foreach (var er in item.SubscriptionVocabulary.ExcludedResults)
                        {
                            var matchedRd = newData.ResultDefinitions.First(x => x.UUID == er.UUID);
                            newExcludedResults.Add(matchedRd);
                        }

                        item.SubscriptionVocabulary.ExcludedResults = newExcludedResults;
                    }
                    
                    if (item.SubscriptionVocabulary.IncludedResults != null)
                    {
                        var newIncludedResults = new List<ResultDefinition>();
                        foreach (var ir in item.SubscriptionVocabulary.IncludedResults)
                        {
                            var matchedRd = newData.ResultDefinitions.First(x => x.UUID == ir.UUID);
                            newIncludedResults.Add(matchedRd);
                        }

                        item.SubscriptionVocabulary.IncludedResults = newIncludedResults;
                    }
                    
                    if (item.SubscriptionVocabulary.IncludedPromptRules != null)
                    {
                        var newIncludedPromptRules = new List<ResultPromptRule>();
                        foreach (var ipr in item.SubscriptionVocabulary.IncludedPromptRules)
                        {
                            var matchedPr = newData.ResultPromptRules.First(x => x.UUID == ipr.UUID);
                            newIncludedPromptRules.Add(matchedPr);
                        }

                        item.SubscriptionVocabulary.IncludedPromptRules = newIncludedPromptRules;
                    }
                    
                    if (item.SubscriptionVocabulary.ExcludedPromptRules != null)
                    {
                        var newExcludedPromptRules = new List<ResultPromptRule>();
                        foreach (var epr in item.SubscriptionVocabulary.ExcludedPromptRules)
                        {
                            var matchedPr = newData.ResultPromptRules.First(x => x.UUID == epr.UUID);
                            newExcludedPromptRules.Add(matchedPr);
                        }

                        item.SubscriptionVocabulary.IncludedPromptRules = newExcludedPromptRules;
                    }
                }

                if(item.IncludedNOWS != null)
                {
                    var newIncludedNows = new List<Now>();
                    foreach (var now in item.IncludedNOWS)
                    {
                        var matchedNow = newData.Nows.First(x => x.UUID == now.UUID);
                        newIncludedNows.Add(matchedNow);
                    }

                    item.IncludedNOWS = newIncludedNows;
                }

                if (item.ExcludedNOWS != null)
                {
                    var newExcludedNows = new List<Now>();
                    foreach (var now in item.ExcludedNOWS)
                    {
                        var matchedNow = newData.Nows.First(x => x.UUID == now.UUID);
                        newExcludedNows.Add(matchedNow);
                    }

                    item.ExcludedNOWS = newExcludedNows;
                }
            }

            //reset the data
            treeModel.AllData = newData;
        }

        private void CleanReferences()
        {
            foreach (var rd in treeModel.AllData.ResultDefinitions.Where(x => x.ResultDefinitionRules != null))
            {
                rd.ResultDefinitionRules = new List<ResultDefinitionRule>();
            }

            foreach (var rd in treeModel.AllData.ResultDefinitions.Where(x => x.ResultPrompts != null))
            {
                rd.ResultPrompts = new List<ResultPromptRule>();
            }

            foreach (var now in treeModel.AllData.Nows.Where(x => x.NowRequirements != null))
            {
                now.NowRequirements = new List<NowRequirement>();
            }

            foreach (var nr in treeModel.AllData.NowRequirements.Where(x => x.NowRequirements != null))
            {
                nr.NowRequirements = new List<NowRequirement>();
            }
        }

        public void PurgeAll()
        {
            var deleted = treeModel.AllData.ResultDefinitionRules.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Result Definition Rules", deleted);

            deleted = treeModel.AllData.ResultDefinitions.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Result Definitions", deleted);

            deleted = treeModel.AllData.ResultPromptRules.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Result Prompt Rules", deleted);

            deleted = treeModel.AllData.ResultDefinitionWordGroups.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Result Definition Word Groups", deleted);

            deleted = treeModel.AllData.ResultPromptWordGroups.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Result Prompt Word Groups", deleted);

            deleted = treeModel.AllData.Nows.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Nows", deleted);

            deleted = treeModel.AllData.NowRequirements.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Now Requirements", deleted);

            deleted = treeModel.AllData.FixedLists.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Fixed Lists", deleted);

            deleted = treeModel.AllData.NowSubscriptions.RemoveAll(x => x.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted Now Subscriptions", deleted);


            foreach (var rd in treeModel.AllData.ResultDefinitions)
            {
                if (rd.ResultDefinitionRules != null)
                {
                    deleted = rd.ResultDefinitionRules.RemoveAll(x => x.DeletedDate != null || x.ResultDefinition.DeletedDate != null);
                    Log.Information("Purged '{0}' unwanted child result definitions from {1}", deleted, rd.Label);
                }

                if (rd.ResultPrompts != null)
                {
                    deleted = rd.ResultPrompts.RemoveAll(x => x.DeletedDate != null || x.ResultPrompt.DeletedDate != null);
                    Log.Information("Purged '{0}' unwanted prompts from {1}", deleted, rd.Label);
                }
            }

            foreach (var now in treeModel.AllData.Nows)
            {
                if (now.NowRequirements != null)
                {
                    PurgeNRs(now.NowRequirements, now);
                }                
            }

            foreach (var sub in treeModel.AllData.NowSubscriptions)
            {
                if (sub.IncludedNOWS != null)
                {
                    sub.IncludedNOWS.RemoveAll(x => x.DeletedDate != null);
                }

                if (sub.ExcludedNOWS != null)
                {
                    sub.ExcludedNOWS.RemoveAll(x => x.DeletedDate != null);
                }
            }
        }

        private void PurgeNRs(List<NowRequirement> nrs, Now now)
        {
            var deleted = nrs.RemoveAll(x => x.DeletedDate != null || x.ResultDefinition.DeletedDate != null);
            Log.Information("Purged '{0}' unwanted now requirements from now {1}", deleted, now.Name);

            foreach (var nr in nrs)
            {
                if (nr.NowRequirementPromptRules != null)
                {
                    deleted = nr.NowRequirementPromptRules.RemoveAll(x => x.DeletedDate != null || x.ResultPromptRule.DeletedDate != null || x.ResultPromptRule.ResultPrompt.DeletedDate != null);
                    Log.Information("Purged '{0}' unwanted now requirement prompt rules from now requirement {1} on now {2}", deleted, nr.ResultDefinition.Label, now.Name);
                }

                if (nr.NowRequirements != null)
                {
                    PurgeNRs(nr.NowRequirements, now);
                }
            }
        }

        private string GetFilePath(object parameter)
        {
            string args = parameter as string;
            if (args == null || args.Length < 0)
            {
                Log.Information("Unable to save data file - must provide file name");
                return null;
            }

            return args;
        }
    }
}
