using DataLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class AllNowRequirementsViewModel : ViewModelBase
    {
        List<NowRequirementViewModel> nowRequirements;

        public AllNowRequirementsViewModel(ITreeModel treeModel, List<NowRequirement> nowRequirements, AllResultDefinitionsViewModel allResultDefinitions)
        {
            this.treeModel = treeModel;

            this.nowRequirements = new List<NowRequirementViewModel>(
                (from x in nowRequirements
                 select new NowRequirementViewModel(treeModel, x, allResultDefinitions.Definitions.FirstOrDefault(y=>y.UUID == x.ResultDefinition.UUID), null, allResultDefinitions))
                .ToList());

            //process the now requirements to build the tree of now requirements
            foreach (var nowReq in this.nowRequirements)
            {
                if (nowReq.ResultDefinition != null && nowReq.ResultDefinition.ResultDefinition.ResultDefinitionRules != null)
                {
                    foreach (var rule in nowReq.ResultDefinition.ResultDefinition.ResultDefinitionRules)
                    {
                        if (nowReq.NowRequirements == null) { nowReq.NowRequirements = new List<NowRequirementViewModel>(); }

                        var matchedChildNowRequirementVM =
                            this.nowRequirements.FirstOrDefault(x => x.ResultDefinition.UUID == rule.ResultDefinition.UUID
                                                                        && x.NOWUUID == nowReq.NOWUUID
                                                                        && x.ParentNowRequirementUUID == nowReq.UUID);

                        if (matchedChildNowRequirementVM != null)
                        {
                            nowReq.NowRequirements.Add(matchedChildNowRequirementVM);
                        }
                    }
                }
            }
        }

        public List<NowRequirementViewModel> NowRequirements
        {
            get { return nowRequirements; }
        }
    }
}
