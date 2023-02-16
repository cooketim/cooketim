using DataLib;
using Models.ViewModels;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Models.Commands
{ 
    public interface ICascadeNowTextCommand : ICommand { }

    public class CascadeNowTextCommand : ICascadeNowTextCommand
    {
        private bool isExecuting;
        private ITreeModel treeModel;
        private int draftsProcessing, draftsToProcess;
        double percentageComplete;

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

        public CascadeNowTextCommand(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

        public string LastActionText { get; private set; }
        public int PercentageComplete { 
            get
            {
                if (draftsProcessing != 0)
                {
                    //drafting is 20% of the process
                    percentageComplete = (draftsProcessing / draftsToProcess) * 20;
                }
                return percentageComplete <= 0 ? 1 : (int)Math.Round(percentageComplete);
            }
        }

        private BlockingCollection<NowTreeViewModel> drafts = null;
        public BlockingCollection<NowTreeViewModel> Drafts 
        {
            get { return drafts; }
            set 
            {               
                drafts = value;                    
            } 
        }      

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    percentageComplete = 1;
                    isExecuting = true;
                    LastActionText = "Executing...";
                    draftsProcessing = 0;
                    draftsToProcess = 0;
                    Cascade(parameter as List<NowRequirementViewModel>);
                }
                finally
                {
                    percentageComplete = 100;
                    isExecuting = false;
                }
            }
            percentageComplete = 100;
        }

        #region Explicit implementations

        public bool CanExecute(object parameter)
        {
            return !isExecuting && Drafts.Count == 0;
        }

        #endregion

        private void Cascade(List<NowRequirementViewModel> nrVMs)
        {
            //Determine the source
            var source = treeModel.SelectedItem as NowRequirementTreeViewModel;

            if (source == null)
            {
                LastActionText = "Now requirement not selected";
                Log.Error("Now requirement not selected");                
                return;
            }

            //get the draft now and edt requirement usages
            var draftVMs = nrVMs.Where(x => x.PublishedStatus == PublishedStatus.Draft).ToList();

            //get the draft nows & edts for the draft requirements so that we can ensure that we do not create additional drafts for them
            var draftIds = draftVMs.Select(x => x.NOWUUID).Distinct();
            var draftNows = (from now in treeModel.AllNowsViewModel.Nows.Where(x => x.DeletedDate == null && !x.IsEDT)
                            join id in draftIds on now.UUID equals id
                            select now).ToList();
            var draftEdts = (from edt in treeModel.AllEDTsViewModel.EDTs.Where(x => x.DeletedDate == null && x.IsEDT)
                            join id in draftIds on edt.UUID equals id
                            select edt).ToList();

            //get the published nows and edts that require drafts to be made
            var publishedNowIds = nrVMs.Where(x => x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending).Select(x => x.NOWUUID).Distinct();
            var publishedNows = (from now in treeModel.AllNowsViewModel.Nows.Where(x => x.DeletedDate == null && !x.IsEDT)
                                 join id in publishedNowIds on now.UUID equals id
                                 select now).ToList();
            var publishedEdts = (from edt in treeModel.AllEDTsViewModel.EDTs.Where(x => x.DeletedDate == null && x.IsEDT)
                                 join id in publishedNowIds on edt.UUID equals id
                                 select edt).ToList();

            //get any published usages that are not required because they already have drafts
            var intersectNows = publishedNows.Select(x=>x.MasterUUID).Intersect(draftNows.Select(x=>x.MasterUUID)).ToList();
            foreach(var dupe in intersectNows)
            {
                var dupeNow = publishedNows.First(x => x.MasterUUID == dupe);
                nrVMs.RemoveAll(x => x.NOWUUID == dupeNow.UUID);
                publishedNows.Remove(dupeNow);
            }
            var intersectEdts = publishedEdts.Select(x => x.MasterUUID).Intersect(draftEdts.Select(x => x.MasterUUID)).ToList();
            foreach (var dupe in intersectEdts)
            {
                var dupeEdt = publishedEdts.First(x => x.MasterUUID == dupe);
                nrVMs.RemoveAll(x => x.NOWUUID == dupeEdt.UUID);
                publishedEdts.Remove(dupeEdt);
            }

            //build a collection of all draft items
            List<NowViewModel> allNowsEdts = new List<NowViewModel>();
            allNowsEdts.AddRange(draftNows);
            allNowsEdts.AddRange(draftEdts);

            //when required make draft nows and or edts
            if (publishedNows.Count>0 || publishedEdts.Count>0)
            {
                MakeDraftNowsEdts(publishedNows, publishedEdts, draftVMs, allNowsEdts, source);
            }            
            
            //determine the selected text
            var selectedText = source.NowRequirementViewModel.SelectedTextValue;

            //set the text on each draft now requirement vm
            foreach (var vm in draftVMs)
            {
                var nowEdt = allNowsEdts.First(x => x.UUID == vm.NOWUUID);
                LastActionText = String.Format("Setting now text for '{0}", nowEdt.Name);
                //match the text model 
                var matched = vm.TextValues.FirstOrDefault(x => x.NowReference == selectedText.NowReference);
                if (matched != null)
                {
                    matched.NowText = selectedText.NowText;
                    matched.NowWelshText = selectedText.NowWelshText;
                }
                else
                {
                    //create a new item of text
                    var data = new NowRequirementText(source.NowRequirementViewModel.NowRequirement);
                    data.NowReference = selectedText.NowReference;
                    data.NowText = selectedText.NowText;
                    data.NowWelshText = selectedText.NowWelshText;
                    var newItem = new NowRequirementTextViewModel(data, source.NowRequirementViewModel);

                    //insert the new item
                    vm.TextValues.Add(newItem);
                }
            }

            //all drafting is complete
            drafts.CompleteAdding();
        }

        private void MakeDraftNowsEdts(List<NowViewModel> publishedNows, List<NowViewModel> publishedEdts, List<NowRequirementViewModel> draftVMs, List<NowViewModel> allNowsEdts, NowRequirementTreeViewModel source)
        {
            //get the tags from the now for the selected requirement
            var selectedNowEdt = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.UUID == source.NowRequirementViewModel.NOWUUID);
            if (selectedNowEdt == null)
            {
                selectedNowEdt = treeModel.AllEDTsViewModel.EDTs.FirstOrDefault(x => x.UUID == source.NowRequirementViewModel.NOWUUID);
            }

            //deal with any published usages by creating draft nows/edts for them
            draftsToProcess = publishedNows.Count + publishedEdts.Count;

            var cmd = treeModel.DraftNowCommand as DraftNowCommand;

            LastActionText = "Making draft NOWs";
            foreach (var now in publishedNows)
            {
                draftsProcessing++;
                var nowTV = treeModel.AllNowsTreeViewModel.NowsPublished.FirstOrDefault(x => x.NowViewModel.UUID == now.UUID);
                NowTreeViewModel draft = MakeDraftNow(selectedNowEdt.PublicationTags, cmd, nowTV);
                drafts.Add(draft);
                allNowsEdts.Add(draft.NowViewModel);

                //append the draft now requirement VMs to the collection of Now Requirements to be updated
                var toAmend = draft.NowViewModel.AllNowRequirements.Where(x => x.ResultDefinition.UUID == source.NowRequirementViewModel.ResultDefinition.UUID);
                draftVMs.AddRange(toAmend);
            }

            LastActionText = "Making draft EDTs";
            foreach (var edt in publishedEdts)
            {
                draftsProcessing++;
                var edtTV = treeModel.AllEDTsTreeViewModel.EdtsPublished.FirstOrDefault(x => x.NowViewModel.UUID == edt.UUID);
                NowTreeViewModel draft = MakeDraftNow(selectedNowEdt.PublicationTags, cmd, edtTV);
                drafts.Add(draft);
                allNowsEdts.Add(draft.NowViewModel);

                //append the draft now requirement VMs to the collection of Now Requirements to be updated
                var toAmend = draft.NowViewModel.AllNowRequirements.Where(x => x.ResultDefinition.UUID == source.NowRequirementViewModel.ResultDefinition.UUID);
                draftVMs.AddRange(toAmend);
            }
        }

        private NowTreeViewModel MakeDraftNow(List<string> publicationTags, DraftNowCommand cmd, NowTreeViewModel nowTV)
        {
            //make the draft
            LastActionText = string.Format("Making draft {0} '{1}'", nowTV.NowViewModel.IsEDT ? "EDT" : "NOW", nowTV.NowViewModel.Name);

            //create a draft NOW/EDT but do not store it
            return cmd.DraftNowTreeViewModel(nowTV, new List<string>(publicationTags), false);
        }
    }
}
