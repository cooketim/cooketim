namespace Models.ViewModels
{
    public class ApplicationRequestViewModel : ViewModelBase
    {
        private bool isLinkedApplication, isStandAloneApplication, isLiveProceedings, isContinuedProceedings, isResentenceOrActivation, isApplicationResulted, isHeardWithOtherCases, isHeardWithoutOtherCases;

        public ApplicationRequestViewModel(bool isLinkedApplication, bool isLiveProceedings)
        {
            this.isLinkedApplication = isLinkedApplication;
            isStandAloneApplication = !isLinkedApplication;
            if (this.isLinkedApplication)
            {
                this.isLiveProceedings = isLiveProceedings;
                isContinuedProceedings = !isLiveProceedings;
                if (isContinuedProceedings)
                {
                    isResentenceOrActivation = true;
                }
                isHeardWithOtherCases = true;
            }
        }

        public bool IsLinkedApplication 
        { 
            get => isLinkedApplication; 
            set
            {
                if (SetProperty(() => isLinkedApplication == value, () => isLinkedApplication = value))
                {
                    isLinkedApplication = value;
                    if (value)
                    {
                        isStandAloneApplication = false;
                        isLiveProceedings = false;
                        isContinuedProceedings = true;
                        isResentenceOrActivation = true;
                        isApplicationResulted = false;
                        isHeardWithoutOtherCases = false;
                        isHeardWithOtherCases = true;
                    }
                    else
                    {
                        isStandAloneApplication = true;
                        isLiveProceedings = false;
                        isContinuedProceedings = false;
                        isResentenceOrActivation = false;
                        isApplicationResulted = false;
                        isHeardWithoutOtherCases = false;
                        isHeardWithOtherCases = false;
                    }

                    OnPropertyChanged("IsStandAloneApplication");
                    OnPropertyChanged("IsLiveProceedings");
                    OnPropertyChanged("IsContinuedProceedings");
                    OnPropertyChanged("IsResentenceOrActivation");
                    OnPropertyChanged("IsApplicationResulted");
                    OnPropertyChanged("IsHeardWithoutOtherCases");
                    OnPropertyChanged("IsHeardWithOtherCases");
                }
            }
        }

        public bool IsStandAloneApplication
        {
            get => isStandAloneApplication;
            set
            {
                if (SetProperty(() => isStandAloneApplication == value, () => isStandAloneApplication = value))
                {
                    isStandAloneApplication = value;
                    if (value)
                    {
                        isLinkedApplication = false;
                        isLiveProceedings = false;
                        isContinuedProceedings = false;
                        isResentenceOrActivation = false;
                        isApplicationResulted = false;
                        isHeardWithoutOtherCases = false;
                        isHeardWithOtherCases = false;
                    }
                    else
                    {
                        isLinkedApplication = true;
                        isLiveProceedings = false;
                        isContinuedProceedings = true;
                        isResentenceOrActivation = false;
                        isApplicationResulted = false;
                        isHeardWithoutOtherCases = false;
                        isHeardWithOtherCases = true;
                    }

                    OnPropertyChanged("IsLinkedApplication");
                    OnPropertyChanged("IsLiveProceedings");
                    OnPropertyChanged("IsContinuedProceedings");
                    OnPropertyChanged("IsResentenceOrActivation");
                    OnPropertyChanged("IsApplicationResulted");
                    OnPropertyChanged("IsHeardWithoutOtherCases");
                    OnPropertyChanged("IsHeardWithOtherCases");
                }
            }
        }

        public bool IsLiveProceedings
        {
            get => isLiveProceedings;
            set
            {
                if (SetProperty(() => isLiveProceedings == value, () => isLiveProceedings = value))
                {
                    isLiveProceedings = value;
                    if (value)
                    {
                        isContinuedProceedings = false;
                        isResentenceOrActivation = false;
                        isApplicationResulted = false;
                    }
                    else
                    {
                        isContinuedProceedings = true;
                        isResentenceOrActivation = true;
                        isApplicationResulted = false;
                    }

                    OnPropertyChanged("IsContinuedProceedings");
                    OnPropertyChanged("IsResentenceOrActivation");
                    OnPropertyChanged("IsApplicationResulted");
                }
            }
        }

        public bool IsContinuedProceedings
        {
            get => isContinuedProceedings;
            set
            {
                if (SetProperty(() => isContinuedProceedings == value, () => isContinuedProceedings = value))
                {
                    isContinuedProceedings = value;
                    if (value)
                    {
                        isLiveProceedings = false;
                        isResentenceOrActivation = true;
                        isApplicationResulted = false;
                    }
                    else
                    {
                        isLiveProceedings = true;
                        isResentenceOrActivation = false;
                        isApplicationResulted = false;
                    }

                    OnPropertyChanged("IsLiveProceedings");
                    OnPropertyChanged("IsResentenceOrActivation");
                    OnPropertyChanged("IsApplicationResulted");
                }
            }
        }

        public bool IsResentenceOrActivation
        {
            get => isResentenceOrActivation;
            set
            {
                if (SetProperty(() => isResentenceOrActivation == value, () => isResentenceOrActivation = value))
                {
                    isResentenceOrActivation = value;
                    if (value)
                    {
                        isApplicationResulted = false;
                    }
                    else
                    {
                        isApplicationResulted = true;
                    }

                    OnPropertyChanged("IsApplicationResulted");
                }
            }
        }

        public bool IsApplicationResulted
        {
            get => isApplicationResulted;
            set
            {
                if (SetProperty(() => isApplicationResulted == value, () => isApplicationResulted = value))
                {
                    isApplicationResulted = value;
                    if (value)
                    {
                        isResentenceOrActivation = false;
                    }
                    else
                    {
                        isResentenceOrActivation = true;
                    }

                    OnPropertyChanged("IsResentenceOrActivation");
                }
            }
        }

        public bool IsHeardWithOtherCases
        {
            get => isHeardWithOtherCases;
            set
            {
                if (SetProperty(() => isHeardWithOtherCases == value, () => isHeardWithOtherCases = value))
                {
                    isHeardWithOtherCases = value;
                    if (value)
                    {
                        isHeardWithoutOtherCases = false;
                    }
                    else
                    {
                        isHeardWithoutOtherCases = true;
                    }

                    OnPropertyChanged("IsHeardWithoutOtherCases");
                }
            }
        }

        public bool IsHeardWithoutOtherCases
        {
            get => isHeardWithoutOtherCases;
            set
            {
                if (SetProperty(() => isHeardWithoutOtherCases == value, () => isHeardWithoutOtherCases = value))
                {
                    isHeardWithoutOtherCases = value;
                    if (value)
                    {
                        isHeardWithOtherCases = false;
                    }
                    else
                    {
                        isHeardWithOtherCases = true;
                    }

                    OnPropertyChanged("IsHeardWithOtherCases");
                }
            }
        }

        public bool WithDebug { get; set; }
    }
}
