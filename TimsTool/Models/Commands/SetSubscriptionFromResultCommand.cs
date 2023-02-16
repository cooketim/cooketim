using DataLib;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface ISetSubscriptionFromResultCommand : ICommand { }
    public class SetSubscriptionFromResultCommand : ISetSubscriptionFromResultCommand
    {
        private ITreeModel treeModel;
        public SetSubscriptionFromResultCommand(ITreeModel treeModel)
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
            PasteTarget(((string)parameter).ToLowerInvariant());

            //reset the copied item
            PendingCopyModel().PendingCopyModel = null;
        }

        private ResultPromptTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as ResultPromptTreeViewModel;
        }

        private void PasteTarget(string target)
        {
            //get the Result Prompt Rule
            var prompt = PendingCopyModel().ResultPromptRuleViewModel.ResultPromptRule;

            //get the selected subscription
            var selectedsubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            if (selectedsubscription == null) { return; }

            if (!string.IsNullOrEmpty(prompt.ResultPrompt.PromptType) 
                && (prompt.ResultPrompt.PromptType.ToLowerInvariant() == "address" || prompt.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress"))
            {
                SetAddressPrompt(prompt, selectedsubscription.NowSubscriptionViewModel);

                if (prompt.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress")
                {
                    switch (target)
                    {
                        case "title":
                        case "firstname":
                        case "middlename":
                        case "lastname":
                            {
                                SetPersonPrompt(prompt, selectedsubscription.NowSubscriptionViewModel);
                                if (prompt.ResultPrompt.NameAddressType.ToLowerInvariant() == "both")
                                {
                                    SetOrganisationPrompt(prompt, selectedsubscription.NowSubscriptionViewModel);
                                }
                                break;
                            }
                        case "organisationname":
                            {
                                SetOrganisationPrompt(prompt, selectedsubscription.NowSubscriptionViewModel);
                                if (prompt.ResultPrompt.NameAddressType.ToLowerInvariant() == "both")
                                {
                                    SetPersonPrompt(prompt, selectedsubscription.NowSubscriptionViewModel);
                                }
                                break;
                            }
                        default:
                            break;
                    }
                    SetAddressPrompt(prompt, selectedsubscription.NowSubscriptionViewModel);
                }
            }
            else
            {
                SetTargetPrompt(target, prompt, selectedsubscription.NowSubscriptionViewModel);
            }
        }

        private void SetTargetPrompt(string target, ResultPromptRule prompt, NowSubscriptionViewModel selectedsubscription)
        {
            //set the subscription vm
            switch (target)
            {
                case "title":
                    {
                        selectedsubscription.TitleResultPrompt = prompt;
                        break;
                    }
                case "firstname":
                    {
                        selectedsubscription.FirstNameResultPrompt = prompt;
                        break;
                    }
                case "middlename":
                    {
                        selectedsubscription.MiddleNameResultPrompt = prompt;
                        break;
                    }
                case "lastname":
                    {
                        selectedsubscription.LastNameResultPrompt = prompt;
                        break;
                    }
                case "organisationname":
                    {
                        selectedsubscription.OrganisationNameResultPrompt = prompt;
                        break;
                    }
                case "address1":
                    {
                        selectedsubscription.Address1ResultPrompt = prompt;
                        break;
                    }
                case "address2":
                    {
                        selectedsubscription.Address2ResultPrompt = prompt;
                        break;
                    }
                case "address3":
                    {
                        selectedsubscription.Address3ResultPrompt = prompt;
                        break;
                    }
                case "address4":
                    {
                        selectedsubscription.Address4ResultPrompt = prompt;
                        break;
                    }
                case "address5":
                    {
                        selectedsubscription.Address5ResultPrompt = prompt;
                        break;
                    }
                case "postcode":
                    {
                        selectedsubscription.PostCodeResultPrompt = prompt;
                        break;
                    }
                case "emailaddress1":
                    {
                        selectedsubscription.EmailAddress1ResultPrompt = prompt;
                        break;
                    }
                case "emailaddress2":
                    {
                        selectedsubscription.EmailAddress2ResultPrompt = prompt;
                        break;
                    }
                default:
                    break;
            }
        }

        private void SetOrganisationPrompt(ResultPromptRule prompt, NowSubscriptionViewModel selectedsubscription)
        {
            if (prompt.ResultPrompt.NameAddressType.ToLowerInvariant() != "both")
            {
                selectedsubscription.TitleResultPrompt = null;
                selectedsubscription.FirstNameResultPrompt = null;
                selectedsubscription.MiddleNameResultPrompt = null;
                selectedsubscription.LastNameResultPrompt = null;
            }
            selectedsubscription.OrganisationNameResultPrompt = prompt;
        }

        private void SetPersonPrompt(ResultPromptRule prompt, NowSubscriptionViewModel selectedsubscription)
        {
            selectedsubscription.TitleResultPrompt = prompt;
            selectedsubscription.FirstNameResultPrompt = prompt;
            selectedsubscription.MiddleNameResultPrompt = prompt;
            selectedsubscription.LastNameResultPrompt = prompt;
            if (prompt.ResultPrompt.NameAddressType.ToLowerInvariant() != "both")
            {
                selectedsubscription.OrganisationNameResultPrompt = null;
            }
        }

        private void SetAddressPrompt(ResultPromptRule prompt, NowSubscriptionViewModel selectedsubscription)
        {
            selectedsubscription.Address1ResultPrompt = prompt;
            selectedsubscription.Address2ResultPrompt = prompt;
            selectedsubscription.Address3ResultPrompt = prompt;
            selectedsubscription.Address4ResultPrompt = prompt;
            selectedsubscription.Address5ResultPrompt = prompt;
            selectedsubscription.PostCodeResultPrompt = prompt;
            selectedsubscription.EmailAddress1ResultPrompt = prompt;
            selectedsubscription.EmailAddress2ResultPrompt = prompt;
        }
    }
}
