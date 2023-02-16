using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataLib;

namespace Models.ViewModels
{
    public class ResultWordGroupViewModel : ViewModelBase
    {
        public ResultWordGroupViewModel(ResultWordGroup resultWordGroup, AllResultDefinitionWordGroupViewModel allWordGroups)
        {
            ResultWordGroup = resultWordGroup;

            if (resultWordGroup.ResultDefinitionWordGroups != null)
            {
                var definitionWords = resultWordGroup.ResultDefinitionWordGroups.FindAll(x => x.DeletedDate == null);
                if (definitionWords != null && definitionWords.Count() > 0)
                {
                    ResultDefinitionWordGroups = new List<ResultDefinitionWordGroupViewModel>();

                    foreach (var definitionWord in definitionWords)
                    {
                        //find the view model for the given result definition word group
                        var matchedWordGroup = allWordGroups.WordGroups.FirstOrDefault(x => x.UUID == definitionWord.UUID);
                        if (matchedWordGroup != null)
                        {
                            ResultDefinitionWordGroups.Add(matchedWordGroup);

                            //set the parent relationship
                            matchedWordGroup.ParentWordGroups.Add(this);
                        }
                    }

                    //order by the definition words
                    ResultDefinitionWordGroups = ResultDefinitionWordGroups.OrderBy(x => x.ResultDefinitionWord).ToList();
                }
            }
        }

        internal void SetResultWordChanged()
        {
            OnPropertyChanged("WordGroupName");

            //Notify the parent result definition that the group word name has changed
            ParentResultDefinitionVM.SetWordGroupsChanged();
        }

        public ResultWordGroup ResultWordGroup { get; set; }

        public bool IsEmptyGroup
        {
            get => ResultDefinitionWordGroups == null || ResultDefinitionWordGroups.Count == 0;
        }

        public string WordGroupName
        {
            get => ResultWordGroup.WordGroupName;
        }

        public List<ResultDefinitionWordGroupViewModel> ResultDefinitionWordGroups { get; set; }

        public ResultDefinitionViewModel ParentResultDefinitionVM { get; internal set; }

        internal void SetDraftResultDefinitionWordGroup(ResultDefinitionWordGroupViewModel newRdWgVm)
        {
            //Update Views
            var matched = ResultDefinitionWordGroups.FirstOrDefault(x => x.MasterUUID == newRdWgVm.MasterUUID);
            if (matched != null)
            {
                var rdWGIndex = ResultDefinitionWordGroups.IndexOf(matched);
                ResultDefinitionWordGroups.RemoveAt(rdWGIndex);
                ResultDefinitionWordGroups.Insert(rdWGIndex, newRdWgVm);
            }

            //Update Data
            var matchedData = ResultWordGroup.ResultDefinitionWordGroups.FirstOrDefault(x => (x.MasterUUID == null ? x.UUID : x.MasterUUID) == newRdWgVm.MasterUUID);
            if (matchedData != null)
            {
                var rdWGIndex = ResultWordGroup.ResultDefinitionWordGroups.IndexOf(matchedData);
                ResultWordGroup.ResultDefinitionWordGroups.RemoveAt(rdWGIndex);
                ResultWordGroup.ResultDefinitionWordGroups.Insert(rdWGIndex, newRdWgVm.ResultDefinitionWordGroup);
            }

            SetResultWordChanged();
        }

        public override bool IsPublishedPending
        {
            get
            {
                if (!IsEmptyGroup && ResultDefinitionWordGroups.Where(x=>x.PublishedStatus == PublishedStatus.PublishedPending).Any())
                {
                    return true;
                }
                return false;
            }
        }
    }
}