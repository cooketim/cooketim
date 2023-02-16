using System;
using Identity;
using DataLib;

namespace Models.ViewModels
{
    public class NowRequirementTextViewModel : ViewModelBase
    {
        private NowRequirementViewModel nowRequirementViewModel;

        public NowRequirementTextViewModel(NowRequirementText nowRequirementText, NowRequirementViewModel nowRequirementViewModel)
        {
            this.NowRequirementText = nowRequirementText;
            this.nowRequirementViewModel = nowRequirementViewModel;
        }

        public NowRequirementText NowRequirementText { get; }

        public string NowReference
        {
            get => NowRequirementText.NowReference;
            set
                {
                    if (SetProperty(() => NowRequirementText.NowReference == value, () => NowRequirementText.NowReference = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string NowText
        {
            get => NowRequirementText.NowText;
            set
                {
                    if (SetProperty(() => NowRequirementText.NowText == value, () => NowRequirementText.NowText = value))
                    {
                        LastModifiedDate = DateTime.Now;

                        //null out the welsh translation since this item is now required to be retranslated
                        NowWelshText = null;
                    }
                }
        }

        public string NowWelshText
        {
            get => NowRequirementText.NowWelshText;
            set
                {
                    if (SetProperty(() => NowRequirementText.NowWelshText == value, () => NowRequirementText.NowWelshText = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        /// <summary>
        /// Fire property changed to ensure that the UI is refreshed
        /// </summary>
        public override DateTime? LastModifiedDate
        {
            get => nowRequirementViewModel.LastModifiedDate;
            set
            {
                nowRequirementViewModel.LastModifiedDate = value;
            }
        }
    }
}