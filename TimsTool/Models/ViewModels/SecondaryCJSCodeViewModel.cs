using System;
using DataLib;

namespace Models.ViewModels
{
    public class SecondaryCJSCodeViewModel : ViewModelBase
    {
        private ResultDefinitionViewModel resultDefinitionViewModel;

        public SecondaryCJSCodeViewModel(ResultDefinitionViewModel resultDefinitionViewModel, SecondaryCJSCode secondaryCJSCode)
        {
            this.resultDefinitionViewModel = resultDefinitionViewModel;
            SecondaryCJSCode = secondaryCJSCode;
        }

        public SecondaryCJSCode SecondaryCJSCode { get; }

        public ResultDefinitionViewModel ResultDefinitionViewModel
        {
            get => resultDefinitionViewModel;
        }

        public string CJSCode
        {
            get => SecondaryCJSCode.CJSCode;
            set
            {
                if (SetProperty(() => SecondaryCJSCode.CJSCode == value, () => SecondaryCJSCode.CJSCode = value))
                {
                    resultDefinitionViewModel.LastModifiedDate = DateTime.Now;
                }
            }
        }

        public string Text
        {
            get => SecondaryCJSCode.Text;
            set
            {
                if (SetProperty(() => SecondaryCJSCode.Text == value, () => SecondaryCJSCode.Text = value))
                {
                    resultDefinitionViewModel.LastModifiedDate = DateTime.Now;
                }
            }
        }

        public override bool IsReadOnly 
        {
            get => resultDefinitionViewModel.IsReadOnly;
        }
    }
}