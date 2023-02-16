using DataLib;
using ExportExcel;
using Models.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDataPatchByTagCommand : ICommand { }

    public class DataPatchByTagCommand : IDataPatchByTagCommand
    {
        DataPatchByTagViewModel dataPatch;
        JsonSerializer serialiser = new JsonSerializer();
        TerminateFragment terminateFragment;
        IdMap map = null;
        private ITreeModel treeModel;

        public DataPatchByTagCommand(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

        private class TerminateFragment
        {
            public string terminationDate { get; set; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            // I intentionally left these empty because
            // this command never raises the event, and
            // not using the WeakEvent pattern here can
            // cause memory leaks.  WeakEvent pattern is
            // not simple to implement, so why bother.
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            //Data directory passed as a parameter
            dataPatch = parameter as DataPatchByTagViewModel;
            if (parameter == null)
            {
                Log.Error("Cannot write data patch without the data patch model being passed as a parameter");
                return;
            }
            if (string.IsNullOrEmpty(dataPatch.DataFileDirectory))
            {
                Log.Error("Cannot write data patch without the data file directory being set");
                return;
            }
            var dataFileDirectory = dataPatch.DataFileDirectory;
            var directoryPath = Path.Combine(dataFileDirectory, "DataPatches");

            //Get the map of file numbers for each patch type
            if (treeModel.Map == null)
            {
                Log.Error("Cannot write data patch with the id map being set");
                return;
            }

            map = treeModel.Map;

            dataPatch = parameter as DataPatchByTagViewModel;
            if (dataPatch == null) { return; }

            //Prepare the datapatch model for exporting data
            dataPatch.PrepareExportData();

            serialiser.NullValueHandling = NullValueHandling.Ignore;
            serialiser.Formatting = Formatting.Indented;

            //initialise the terminationDateFragment
            terminateFragment = new TerminateFragment() { terminationDate = dataPatch.PatchDate.ToString("yyyy-MM-dd") };

            //serialise data to json
            Directory.CreateDirectory(directoryPath);
            var fileName = string.Format("dataPatch-forselectedtags-{0}.zip", dataPatch.PatchDate.ToString("yyyyMMdd"));
            var fullFilePath = Path.Combine(directoryPath, fileName);

            var reportList = new List<Tuple<string, StringBuilder>>();

            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    WriteChangeReport(archive);
                    ExportResultDefinitions(archive);
                    ExportNows(archive);
                    ExportNowSubscriptions(archive);
                    ExportFixedLists(archive);
                    ExportResultSynonyms(archive);
                    ExportResultPromptSynonyms(archive);
                }
            }

            //when required update the publication status
            if (dataPatch.IsProdExport)
            {
                SetResultDefinitionsPublished(dataPatch.DeletedResults, dataPatch.CreatedOrModifiedResults);
                SetNowsPublished(dataPatch.DeletedNows, dataPatch.CreatedOrModifiedNows);
                SetNowSubscriptionsPublished(dataPatch.DeletedNowSubscriptions, dataPatch.CreatedOrModifiedNowSubscriptions);
                SetFixedListsPublished(dataPatch.DeletedFixedLists, dataPatch.CreatedOrModifiedFixedLists);
            }
        }

        private void SetFixedListsPublished(List<FixedListViewModel> deletedFixedLists, List<FixedListViewModel> createdOrModifiedFixedLists)
        {
            //set the fixed lists as published
            foreach (var fl in createdOrModifiedFixedLists)
            {
                //set the fl as published
                fl.PublishedStatus = PublishedStatus.Published;
                fl.FixedList.IsNewItem = null;

                //set any corresponding revision pending
                var revision = treeModel.AllFixedListViewModel.FixedLists.FirstOrDefault(x => x.MasterUUID == fl.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
                if (revision != null)
                {
                    revision.PublishedStatus = PublishedStatus.Revision;
                }
            }

            //purge the deleted fixed lists
            deletedFixedLists.ForEach(x => PurgeFixedList(x));
        }

        private void PurgeFixedList(FixedListViewModel fl)
        {
             //remove view models
            treeModel.AllFixedListViewModel.FixedLists.RemoveAll(x => x.UUID == fl.UUID);

            //remove data
            treeModel.AllData.FixedLists.RemoveAll(x => x.UUID == fl.UUID);

            //deal with any associated revision pending
            var revision = treeModel.AllFixedListViewModel.FixedLists.FirstOrDefault(x => x.MasterUUID == fl.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
            if (revision != null)
            {
                revision.PublishedStatus = PublishedStatus.Revision;
            }
        }

        private void SetNowSubscriptionsPublished(List<NowSubscriptionViewModel> deletedNowSubscriptions, List<NowSubscriptionViewModel> createdOrModifiedNowSubscriptions)
        {
            //set the now subscriptions as published
            foreach (var sub in createdOrModifiedNowSubscriptions)
            {
                //set the subscription as published
                sub.PublishedStatus = PublishedStatus.Published;
                sub.NowSubscription.IsNewItem = null;

                //reset the display for the new status
                sub.ResetStatus();

                SetSubscriptionRevision(sub);
            }

            //purge the deleted subscriptions
            deletedNowSubscriptions.ForEach(x => PurgeSubscription(x));

            //Deal with any associated revision pending trees
            treeModel.AllNowSubscriptionsTreeViewModel.ResetRevisionPending();
            treeModel.AllEDTSubscriptionsTreeViewModel.ResetRevisionPending();
            treeModel.AllCourtRegisterSubscriptionsTreeViewModel.ResetRevisionPending();
            treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.ResetRevisionPending();
            treeModel.AllInformantRegisterSubscriptionsTreeViewModel.ResetRevisionPending();
        }

        private void SetSubscriptionRevision(NowSubscriptionViewModel sub)
        {
            //set any corresponding revision pending
            NowSubscriptionViewModel revision=null;
            if (sub.NowSubscription.IsNow)
            {
                revision = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FirstOrDefault(x => x.MasterUUID == sub.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
            }
            else if (sub.NowSubscription.IsEDT)
            {
                revision = treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.FirstOrDefault(x => x.MasterUUID == sub.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
            }
            else if (sub.NowSubscription.IsCourtRegister)
            {
                revision = treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.FirstOrDefault(x => x.MasterUUID == sub.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
            }
            else if (sub.NowSubscription.IsPrisonCourtRegister)
            {
                revision = treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.FirstOrDefault(x => x.MasterUUID == sub.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
            }
            else if (sub.NowSubscription.IsInformantRegister)
            {
                revision = treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.FirstOrDefault(x => x.MasterUUID == sub.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
            }

            if (revision != null)
            {
                revision.PublishedStatus = PublishedStatus.Revision;
                if (sub.IsDeleted)
                {
                    revision.DeletedDate = dataPatch.PatchDate;
                }
            }
        }

        private void PurgeSubscription(NowSubscriptionViewModel sub)
        {
            if (sub.NowSubscription.IsNow)
            {
                //remove tree items
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsPublished.RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);

                //remove views
                treeModel.AllNowSubscriptionViewModel.NowSubscriptions.RemoveAll(x => x.UUID == sub.UUID);
            }
            else if (sub.NowSubscription.IsEDT)
            {
                //remove tree items
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.ToList().RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsPublished.RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);

                //remove views
                treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.RemoveAll(x => x.UUID == sub.UUID);
            }
            else if (sub.NowSubscription.IsCourtRegister)
            {
                //remove tree items
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.ToList().RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsPublished.RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);

                //remove views
                treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.RemoveAll(x => x.UUID == sub.UUID);
            }
            else if (sub.NowSubscription.IsPrisonCourtRegister)
            {
                //remove tree items
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.ToList().RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsPublished.RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);

                //remove views
                treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.RemoveAll(x => x.UUID == sub.UUID);
            }
            else if (sub.NowSubscription.IsInformantRegister)
            {
                //remove tree items
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.ToList().RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsPublished.RemoveAll(x => x.NowSubscriptionViewModel.UUID == sub.UUID);

                //remove views
                treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.RemoveAll(x => x.UUID == sub.UUID);
            }

            //remove data
            treeModel.AllData.NowSubscriptions.RemoveAll(x => x.UUID == sub.UUID);

            //purge the children
            if (sub.ChildNowSubscriptions != null)
            {
                foreach (var child in sub.ChildNowSubscriptions)
                {
                    PurgeSubscription(child);
                }
            }

            //deal with any associated revision pending
            SetSubscriptionRevision(sub);
        }

        private void SetResultDefinitionsPublished(List<ResultDefinitionViewModel> deletedResults, List<ResultDefinitionViewModel> createdOrModifiedResults)
        {
            //set the results as published
            foreach(var rd in createdOrModifiedResults)
            {
                //set the result as published
                rd.PublishedStatus = PublishedStatus.Published;
                rd.ResultDefinition.IsNewItem = null;

                //set any child prompts as published
                if (rd.Prompts != null)
                {
                    foreach (var rpr in rd.Prompts.Where(x => x.ResultPromptViewModel.IsPublishedPending))
                    {
                        rpr.PublishedStatus = PublishedStatus.Published;
                        rpr.ResultPromptRule.IsNewItem = null;

                        rpr.ResultPromptViewModel.PublishedStatus = PublishedStatus.Published;
                        rpr.ResultPromptViewModel.ResultPrompt.IsNewItem = null;

                        rpr.ResetPending();
                    }
                }

                //set any corresponding revision pending
                var revision = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.MasterUUID == rd.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
                if (revision != null)
                {
                    revision.PublishedStatus = PublishedStatus.Revision;
                }
            }

            //set the result rules as published
            foreach (var rd in createdOrModifiedResults)
            {
                foreach (var rule in treeModel.AllResultRuleViewModel.Rules.Where(x => x.ChildResultDefinitionViewModel.MasterUUID == rd.MasterUUID))
                {
                    if (!rule.ParentUUID.HasValue || rule.ParentResultDefinitionViewModel.PublishedStatus == PublishedStatus.Published)
                    {
                        rule.PublishedStatus = PublishedStatus.Published;
                        rule.ResultDefinitionRule.IsNewItem = null;
                    }

                    rule.ResetStatus();
                }
            }

            //purge the deleted results
            deletedResults.ForEach(x => PurgeResult(x));

            //Deal with any associated revision pending trees
            treeModel.AllResultDefinitionsTreeViewModel.ResetRevisionPending();
        }

        private void PurgeResult(ResultDefinitionViewModel rd)
        {
            //remove tree view items
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.RemoveAll(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == rd.UUID);
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.ToList().RemoveAll(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == rd.UUID);
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished.RemoveAll(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == rd.UUID);

            //remove view models
            treeModel.AllResultDefinitionsViewModel.Definitions.RemoveAll(x => x.UUID == rd.UUID);
            treeModel.AllResultRuleViewModel.Rules.RemoveAll(x => x.ChildResultDefinitionViewModel.UUID == rd.UUID);
            treeModel.AllResultPromptViewModel.Prompts.RemoveAll(x => x.ResultPromptRule.ResultDefinitionUUID == rd.UUID);

            //remove data
            treeModel.AllData.ResultDefinitions.RemoveAll(x => x.UUID == rd.UUID);
            treeModel.AllData.ResultDefinitionRules.RemoveAll(x => x.ResultDefinition.UUID == rd.UUID);
            treeModel.AllData.ResultPromptRules.RemoveAll(x => x.ResultDefinitionUUID == rd.UUID);

            //deal with any associated revision pending
            var revision = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.MasterUUID == rd.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
            if (revision != null)
            {
                revision.DeletedDate = dataPatch.PatchDate;
                revision.PublishedStatus = PublishedStatus.Revision;

                foreach (var rpr in revision.Prompts.Where(x => x.IsRevisionPending))
                {
                    rpr.PublishedStatus = PublishedStatus.Revision;
                    rpr.DeletedDate = dataPatch.PatchDate;
                    if (rpr.ResultPromptViewModel.PublishedStatus == PublishedStatus.RevisionPending)
                    {
                        rpr.ResultPromptViewModel.PublishedStatus = PublishedStatus.Revision;
                        rpr.ResultPromptViewModel.DeletedDate = dataPatch.PatchDate;
                    }
                    else
                    {
                        //check for other prompt usages
                        var otherNonRevisionUsages = treeModel.AllResultPromptViewModel.Prompts.Where(x =>
                                                        x.ResultPromptViewModel.MasterUUID == rpr.ResultPromptViewModel.MasterUUID
                                                     && x.ResultPromptViewModel.PublishedStatus != PublishedStatus.Revision
                                                     && x.ResultDefinitionViewModel.MasterUUID != rpr.ResultDefinitionViewModel.MasterUUID).ToList();

                        if (otherNonRevisionUsages.Count() == 0)
                        {
                            //no other usages,set the prompt status to revision and set teh prompt as deleted
                            rpr.ResultPromptViewModel.PublishedStatus = PublishedStatus.Revision;
                            rpr.ResultPromptViewModel.DeletedDate = dataPatch.PatchDate;
                        }
                    }

                    rpr.ResetPending();
                }
            }
        }

        private void SetNowsPublished(List<NowViewModel> deletedNows, List<NowViewModel> createdOrModifiedNows)
        {
            //set the created and modified nows as published
            foreach (var now in createdOrModifiedNows)
            {
                //set the now as published
                SetNowStatus(now, PublishedStatus.Published);

                SetAnyRevisionPending(now);
            }

            //purge the deleted nows
            deletedNows.ForEach(x => PurgeNowEdt(x));

            //Deal with any associated revision pending trees
            treeModel.AllNowsTreeViewModel.ResetRevisionPending();
            treeModel.AllEDTsTreeViewModel.ResetRevisionPending();
        }

        private void SetAnyRevisionPending(NowViewModel now)
        {
            //set any corresponding revision pending
            if (now.IsEDT)
            {
                var revision = treeModel.AllEDTsViewModel.EDTs.FirstOrDefault(x => x.MasterUUID == now.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
                if (revision != null)
                {
                    if (now.IsDeleted)
                    {
                        //set the revision as deleted
                        revision.DeletedDate = dataPatch.PatchDate;
                    }
                    SetNowStatus(revision, PublishedStatus.Revision);
                }
            }
            else
            {
                var revision = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.MasterUUID == now.MasterUUID && x.PublishedStatus == PublishedStatus.RevisionPending);
                if (revision != null)
                {
                    if (now.IsDeleted)
                    {
                        //set the revision as deleted
                        revision.DeletedDate = dataPatch.PatchDate;
                    }
                    SetNowStatus(revision, PublishedStatus.Revision);
                }
            }
        }

        private void PurgeNowEdt(NowViewModel nowEdt)
        {
            //remove now or edt from the tree and view model collections
            if (nowEdt.IsEDT)
            {
                treeModel.AllEDTsTreeViewModel.EDTs.RemoveAll(x => x.NowViewModel.UUID == nowEdt.UUID);
                treeModel.AllEDTsTreeViewModel.EdtsPublished.RemoveAll(x => x.NowViewModel.UUID == nowEdt.UUID);
                treeModel.AllEDTsViewModel.EDTs.RemoveAll(x => x.UUID == nowEdt.UUID);
            }
            else
            {
                treeModel.AllNowsTreeViewModel.Nows.RemoveAll(x => x.NowViewModel.UUID == nowEdt.UUID);
                treeModel.AllNowsTreeViewModel.NowsPublished.RemoveAll(x => x.NowViewModel.UUID == nowEdt.UUID);
                treeModel.AllNowsViewModel.Nows.RemoveAll(x => x.UUID == nowEdt.UUID);
            }

            //remove any now requirement models
            treeModel.AllNowRequirementsViewModel.NowRequirements.RemoveAll(x => x.NOWUUID == nowEdt.UUID);

            //remove the data
            treeModel.AllData.NowRequirements.RemoveAll(x => x.NOWUUID == nowEdt.UUID);
            treeModel.AllData.Nows.RemoveAll(x => x.UUID == nowEdt.UUID);

            //deal with any associated revision pending
            SetAnyRevisionPending(nowEdt);
        }

        private void SetNowStatus(NowViewModel now, PublishedStatus status)
        {
            now.PublishedStatus = status;
            now.Now.IsNewItem = null;
            now.ResetStatus();

            //set any now requirements as revision
            foreach (var nr in now.AllNowRequirements)
            {
                nr.PublishedStatus = status;
                nr.NowRequirement.IsNewItem = null;

                if (now.IsDeleted)
                {
                    //set the now requirements as deleted
                    nr.DeletedDate = dataPatch.PatchDate;
                }

                //deal with any now requirement prompt rules
                if (nr.NowRequirementPromptRules != null)
                {
                    foreach (var nrpr in nr.NowRequirementPromptRules)
                    {
                        nrpr.PublishedStatus = status;
                        nrpr.NowRequirementPromptRule.IsNewItem = null;

                        if (now.IsDeleted)
                        {
                            //set the now requirement prompt rule as deleted
                            nrpr.DeletedDate = dataPatch.PatchDate;
                        }
                    }
                }
            }
        }

        private void WriteChangeReport(ZipArchive archive)
        {
            //write to a memory stream initially because the excel exporter will require a stream that can ve read write and seek whereas the stream for the zip entry is write only
            using (var ms = new MemoryStream())
            {
                var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                exporter.CreateDataPatchChangeExcelExport(ms, dataPatch.ChangeReportData);
                ms.Seek(0, SeekOrigin.Begin);

                var fileNameManifest = "datapatch-change-report/datapatch-change-report.xlsx";
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);

                var archivestream = manifest.Open();
                ms.CopyTo(archivestream);
                archivestream.Close();
            }                        
        }

        private void WriteTerminationDate(ZipArchiveEntry manifest, bool withDate)
        {
            if (!withDate)
            {
                return;
            }
            using (StreamWriter sw = new StreamWriter(manifest.Open()))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    serialiser.Serialize(writer, terminateFragment);
                    writer.Flush();
                }
            }
        }

        private void ExportResultDefinitions(ZipArchive archive)
        {
            //Result Definitions, flattened Prompts (promptRule & prompt)
            var createDirectoryNameManifest = "result-definitions";
            var terminateDirectoryNameManifest = "terminate-resultdefinitions";

            //Process the terminate entries i.e. all deleted entries and all entries that have been changed

            //First determine if any of the result definitions have been deleted
            //Only terminate the deleted entries that have not been created within the same patch period
            foreach (var rd in dataPatch.DeletedResults)
            {
                //set the end date
                rd.EndDate = dataPatch.PatchDate.Date;
                var fileNum = map.GetOrAddFileNum(rd.MasterUUID.Value, IdType.ResultDefinition);
                var fileNameManifest = string.Format("{0}/{1}.terminate-{2}.json", terminateDirectoryNameManifest, fileNum, rd.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                WriteTerminationDate(manifest, false);
            }

            //now terminate the modified excluding entries that have been created since the last export
            foreach (var rd in dataPatch.ModifiedResults)
            {
                var fileNum = map.GetOrAddFileNum(rd.MasterUUID.Value, IdType.ResultDefinition);
                var fileNameManifest = string.Format("{0}/{1}.terminate-{2}.json", terminateDirectoryNameManifest, fileNum, rd.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);

                WriteTerminationDate(manifest, false);
            }

            foreach (var rd in dataPatch.CreatedOrModifiedResults)
            {
                //set the start & end date
                rd.StartDate = dataPatch.StartDate(rd.StartDate);
                rd.EndDate = dataPatch.EndDate(rd.EndDate, rd.StartDate);

                var fileNum = map.GetOrAddFileNum(rd.MasterUUID.Value, IdType.ResultDefinition);
                var fileNameManifest = string.Format("{0}/{1}.create-{2}.json", createDirectoryNameManifest, fileNum, rd.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                using (StreamWriter sw = new StreamWriter(manifest.Open()))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;

                        serialiser.Serialize(writer, new ResultDefinitionJSON(rd.ResultDefinition, dataPatch.PatchDate, dataPatch.FeatureToggles));

                        writer.Flush();
                    }
                }
            }
        }

        private void ExportNows(ZipArchive archive)
        {
            //Nows
            var createDirectoryNameManifest = "now-metadata";
            var terminateDirectoryNameManifest = "terminate-nowmetadata";

            //first terminate the deleted entries that have not been created within the same patch period
            foreach (var now in dataPatch.DeletedNows)
            {
                //set the end date
                now.EndDate = dataPatch.PatchDate.Date;

                var fileNum = map.GetOrAddFileNum(now.MasterUUID.Value, IdType.Now);
                var fileNameManifest = string.Format("{0}/{1}.terminate-{2}.json", terminateDirectoryNameManifest, fileNum, now.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                WriteTerminationDate(manifest, false);
            }

            //now terminate the modified entries excluding entries that have been created since the last export
            foreach (var now in dataPatch.ModifiedNows)
            {
                var fileNum = map.GetOrAddFileNum(now.MasterUUID.Value, IdType.Now);
                var fileNameManifest = string.Format("{0}/{1}.terminate-{2}.json", terminateDirectoryNameManifest, fileNum, now.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                WriteTerminationDate(manifest, false);
            }

            //now create the modified and created entries
            foreach (var now in dataPatch.CreatedOrModifiedNows)
            {
                //set the start & end date
                now.StartDate = dataPatch.StartDate(now.StartDate);
                now.EndDate = dataPatch.EndDate(now.EndDate, now.StartDate);

                var fileNum = map.GetOrAddFileNum(now.MasterUUID.Value, IdType.Now);
                var fileNameManifest = string.Format("{0}/{1}.create-{2}.json", createDirectoryNameManifest, fileNum, now.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                using (StreamWriter sw = new StreamWriter(manifest.Open()))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;

                        var data = new NowJSON(now.Now, dataPatch.PatchDate);
                        serialiser.Serialize(writer, data);

                        writer.Flush();
                    }
                }
            }
        }

        private void ExportNowSubscriptions(ZipArchive archive)
        {
            //Now Subscriptions
            var createDirectoryNameManifest = "now-subscriptions";
            var terminateDirectoryNameManifest = "terminate-nowsubscriptions";

            //first terminate the deleted entries that have not been created within the same patch period
            foreach (var nowSub in dataPatch.DeletedNowSubscriptions)
            {
                //set the end date
                nowSub.EndDate = dataPatch.PatchDate.Date;

                var fileNum = map.GetOrAddFileNum(nowSub.MasterUUID.Value, IdType.NowSubscription);
                var fileNameManifest = string.Format("{0}/{1}.terminate-{2}.json", terminateDirectoryNameManifest, fileNum, nowSub.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                WriteTerminationDate(manifest, false);
            }

            //now terminate the modified entries excluding entries that have been created since the last export
            foreach (var nowSub in dataPatch.ModifiedNowSubscriptions)
            {
                var fileNum = map.GetOrAddFileNum(nowSub.MasterUUID.Value, IdType.NowSubscription);
                var fileNameManifest = string.Format("{0}/{1}.terminate-{2}.json", terminateDirectoryNameManifest, fileNum, nowSub.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                WriteTerminationDate(manifest, false);
            }

            //now create the modified and created entries
            foreach (var nowSub in dataPatch.CreatedOrModifiedNowSubscriptions)
            {
                //set the start & end date
                nowSub.StartDate = dataPatch.StartDate(nowSub.StartDate);
                nowSub.EndDate = dataPatch.EndDate(nowSub.EndDate, nowSub.StartDate);

                var fileNum = map.GetOrAddFileNum(nowSub.MasterUUID.Value, IdType.NowSubscription);
                var fileNameManifest = string.Format("{0}/{1}.create-{2}.json", createDirectoryNameManifest, fileNum, nowSub.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                using (StreamWriter sw = new StreamWriter(manifest.Open()))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;

                        serialiser.Serialize(writer, new NowSubscriptionJSON(nowSub.NowSubscription, dataPatch.PatchDate, dataPatch.FeatureToggles));

                        writer.Flush();
                    }
                }
            }
        }

        private void ExportFixedLists(ZipArchive archive)
        {
            //Fixed Lists
            var createDirectoryNameManifest = "fixed-list";
            var terminateDirectoryNameManifest = "terminate-fixedlist";

            //first terminate the deleted entries that have not been created within the same patch period
            foreach (var fl in dataPatch.DeletedFixedLists)
            {
                //set the end date
                fl.EndDate = dataPatch.PatchDate.Date;

                var fileNum = map.GetOrAddFileNum(fl.MasterUUID.Value, IdType.FixedList);
                var fileNameManifest = string.Format("{0}/{1}.terminate-{2}.json", terminateDirectoryNameManifest, fileNum, fl.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                WriteTerminationDate(manifest, false);
            }

            //now terminate the modified entries excluding entries that have been created since the last export
            foreach (var fl in dataPatch.ModifiedFixedLists)
            {
                var fileNum = map.GetOrAddFileNum(fl.MasterUUID.Value, IdType.FixedList);
                var fileNameManifest = string.Format("{0}/{1}.terminate-{2}.json", terminateDirectoryNameManifest, fileNum, fl.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                WriteTerminationDate(manifest, false);
            }

            //now create the modified and created entries
            foreach (var fl in dataPatch.CreatedOrModifiedFixedLists)
            {
                //set the start & end date
                fl.StartDate = dataPatch.StartDate(fl.StartDate);
                fl.EndDate = dataPatch.EndDate(fl.EndDate, fl.StartDate);

                var fileNum = map.GetOrAddFileNum(fl.MasterUUID.Value, IdType.FixedList);
                var fileNameManifest = string.Format("{0}/{1}.create-{2}.json", createDirectoryNameManifest, fileNum, fl.MasterUUID);
                ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
                using (StreamWriter sw = new StreamWriter(manifest.Open()))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;

                        serialiser.Serialize(writer, new FixedListJSON(fl.FixedList));

                        writer.Flush();
                    }
                }
            }
        }

        private void ExportResultSynonyms(ZipArchive archive)
        {
            //Result Definition Word Synonyms
            var createDirectoryNameManifest = "result-word-synonyms";
            var terminateDirectoryNameManifest = "terminate-resultwordsynonyms";

            //Terminate the existing synonyms
            var fileNameManifest = string.Format("{0}/1.terminate-{1}.json", terminateDirectoryNameManifest, treeModel.AllData.ResultDefinitionSynonymCollectionId);
            ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
            WriteTerminationDate(manifest, false);

            if (treeModel.AllData.ResultDefinitionSynonymCollectionId == null) { return; }

            if (treeModel.AllData.ResultDefinitionWordGroups == null) { return; }

            //Create New Synonyms
            fileNameManifest = string.Format("{0}/1.create-{1}.json", createDirectoryNameManifest, treeModel.AllData.ResultDefinitionSynonymCollectionId);

            if (dataPatch.ActiveResultDefinitionSynonyms.Count == 0) { return; }

            var extractData = new ResultSynonymExtract(treeModel.AllData.ResultDefinitionSynonymCollectionId.Value, dataPatch.ActiveResultDefinitionSynonyms);

            manifest = archive.CreateEntry(fileNameManifest);
            using (StreamWriter sw = new StreamWriter(manifest.Open()))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    serialiser.Serialize(writer, extractData);
                    writer.Flush();
                }
            }
        }

        private void ExportResultPromptSynonyms(ZipArchive archive)
        {

            //Note - no format cahnges from 251 legacy, therefore continue as normal

            //Result Prompt Word Synonyms
            var createDirectoryNameManifest = "result-prompt-word-synonyms";
            var terminateDirectoryNameManifest = "terminate-resultpromptwordsynonyms";

            //Terminate the existing synonyms
            var fileNameManifest = string.Format("{0}/1.terminate-{1}.json", terminateDirectoryNameManifest, treeModel.AllData.ResultPromptSynonymCollectionId);
            ZipArchiveEntry manifest = archive.CreateEntry(fileNameManifest);
            WriteTerminationDate(manifest, false);

            if (treeModel.AllData.ResultPromptSynonymCollectionId == null) { return; }

            if (treeModel.AllData.ResultPromptWordGroups == null) { return; }

            //Create New Synonyms
            fileNameManifest = string.Format("{0}/1.create-{1}.json", createDirectoryNameManifest, treeModel.AllData.ResultPromptSynonymCollectionId);
            if (dataPatch.ActiveResultPromptSynonyms.Count == 0) { return; }

            var extractData = new ResultPromptSynonymExtract(treeModel.AllData.ResultPromptSynonymCollectionId.Value, dataPatch.ActiveResultPromptSynonyms);

            manifest = archive.CreateEntry(fileNameManifest);
            using (StreamWriter sw = new StreamWriter(manifest.Open()))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    serialiser.Serialize(writer, extractData);
                    writer.Flush();
                }
            }
        }

        [Serializable]
        private class ResultSynonymExtract
        {
            public ResultSynonymExtract(Guid id, List<DataLib.ResultDefinitionWordGroup> activeSynonyms)
            {
                this.id = id;
                synonymCollection = new List<WordSynonymExtract>();

                foreach (var group in activeSynonyms)
                {
                    foreach (var synonym in group.Synonyms.FindAll(x => x.DeletedDate == null))
                    {
                        synonymCollection.Add(new WordSynonymExtract(group.ResultDefinitionWord, synonym.Synonym));
                    }
                }

                synonymCollection = synonymCollection.OrderBy(x => x.word).ThenBy(y => y.synonym).ToList();
            }

            public Guid id { get; }

            public List<WordSynonymExtract> synonymCollection { get; set; }
        }

        [Serializable]
        private class ResultPromptSynonymExtract
        {
            public ResultPromptSynonymExtract(Guid id, List<ResultPromptWordGroup> activeSynonyms)
            {
                this.id = id;
                synonymCollection = new List<WordSynonymExtract>();

                foreach (var group in activeSynonyms)
                {
                    foreach (var synonym in group.Synonyms.FindAll(x => x.DeletedDate == null))
                    {
                        synonymCollection.Add(new WordSynonymExtract(group.ResultPromptWord, synonym.Synonym));
                    }
                }

                synonymCollection = synonymCollection.OrderBy(x => x.word).ThenBy(y => y.synonym).ToList();
            }

            public Guid id { get; }

            public List<WordSynonymExtract> synonymCollection { get; set; }
        }

        private class WordSynonymExtract
        {
            public WordSynonymExtract(string word, string synonym)
            {
                this.word = word;
                this.synonym = synonym;
            }

            public string word { get; }

            public string synonym { get; }
        }
    }
}
