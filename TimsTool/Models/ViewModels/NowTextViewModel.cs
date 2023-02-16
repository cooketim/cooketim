using System;
using DataLib;

namespace Models.ViewModels
{
    public class NowTextViewModel : ViewModelBase
    {
        private readonly NowViewModel nowViewModel;

        public NowTextViewModel(NowText nowText, NowViewModel nowViewModel)
        {
            this.NowText = nowText;
            this.nowViewModel = nowViewModel;
        }

        public NowText NowText { get; }

        public string NowReference
        {
            get => NowText.Reference;
            set
                {
                    if (SetProperty(() => NowText.Reference == value, () => NowText.Reference = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string Text
        {
            get => NowText.Text;
            set
                {
                    if (SetProperty(() => NowText.Text == value, () => NowText.Text = value))
                    {
                        LastModifiedDate = DateTime.Now;

                        //null out the welsh translation since this item is now required to be retranslated
                        NowWelshText = null;
                    }
                }
        }

        public string NowWelshText
        {
            get => NowText.WelshText;
            set
                {
                    if (SetProperty(() => NowText.WelshText == value, () => NowText.WelshText = value))
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
            get => nowViewModel.LastModifiedDate;
            set
            {
                nowViewModel.LastModifiedDate = value;
            }
        }
    }
}