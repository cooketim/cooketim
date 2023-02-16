using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using DataLib;
using System.Linq;

namespace ExportExcel
{
    public class NowSubscriptionBuilder : SheetBuilderBase
    {
        public NowSubscriptionBuilder(WorkbookPart workbookPart, AllData allData) : base("NOW Subscriptions", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Subscription UUID", CellValues.String),
                ConstructCell("Subscription Name", CellValues.String),
                ConstructCell("Parent Subscription UUID", CellValues.String),
                ConstructCell("IsNow", CellValues.String),
                ConstructCell("IsEDT", CellValues.String),
                ConstructCell("IsInformantRegister", CellValues.String),
                ConstructCell("IsCourtRegister", CellValues.String),
                ConstructCell("IsPrisonCourtRegister", CellValues.String),
                ConstructCell("IsFirstClassLetter", CellValues.String),
                ConstructCell("IsSecondClassLetter", CellValues.String),
                ConstructCell("IsEmail", CellValues.String),
                ConstructCell("IsForDistribution", CellValues.String),
                ConstructCell("User Group Variants", CellValues.String),
                ConstructCell("Informant Code", CellValues.String),
                ConstructCell("YOTsCode", CellValues.String),
                ConstructCell("Selected Court Houses", CellValues.String),
            #region Recipient Details
                ConstructCell("RecipientFromCase", CellValues.String),
                ConstructCell("RecipientFromSubscription", CellValues.String),
                ConstructCell("RecipientFromResults", CellValues.String),
                ConstructCell("IsApplyDefenceOrganisationDetails", CellValues.String),
                ConstructCell("IsApplyParentGuardianDetails", CellValues.String),
                ConstructCell("IsApplyApplicantDetails", CellValues.String),
                ConstructCell("IsApplyDefendantDetails", CellValues.String),
                ConstructCell("IsApplyRespondentDetails", CellValues.String),
                ConstructCell("IsApplyThirdPartyDetails", CellValues.String),
                ConstructCell("IsApplyDefendantCustodyDetails", CellValues.String),
                ConstructCell("IsApplyProsecutionAuthorityDetails", CellValues.String),
                ConstructCell("Title", CellValues.String),
                ConstructCell("FirstName", CellValues.String),
                ConstructCell("MiddleName", CellValues.String),
                ConstructCell("LastName", CellValues.String),
                ConstructCell("OrganisationName", CellValues.String),
                ConstructCell("TitleResultPromptId", CellValues.String),
                ConstructCell("FirstNameResultPromptId", CellValues.String),
                ConstructCell("MiddleNameResultPromptId", CellValues.String),
                ConstructCell("LastNameResultPromptId", CellValues.String),
                ConstructCell("OrganisationNameResultPromptId", CellValues.String),
                ConstructCell("TitleResultDefinitionId", CellValues.String),
                ConstructCell("FirstNameResultDefinitionId", CellValues.String),
                ConstructCell("MiddleNameResultDefinitionId", CellValues.String),
                ConstructCell("LastNameResultDefinitionId", CellValues.String),
                ConstructCell("OrganisationNameResultDefinitionId", CellValues.String),
                ConstructCell("Address1", CellValues.String),
                ConstructCell("Address2", CellValues.String),
                ConstructCell("Address3", CellValues.String),
                ConstructCell("Address4", CellValues.String),
                ConstructCell("Address5", CellValues.String),
                ConstructCell("PostCode", CellValues.String),
                ConstructCell("EmailAddress1", CellValues.String),
                ConstructCell("EmailAddress2", CellValues.String),
                ConstructCell("Address1PromptId", CellValues.String),
                ConstructCell("Address2PromptId", CellValues.String),
                ConstructCell("Address3PromptId", CellValues.String),
                ConstructCell("Address4PromptId", CellValues.String),
                ConstructCell("Address5PromptId", CellValues.String),
                ConstructCell("PostCodePromptId", CellValues.String),
                ConstructCell("EmailAddress1PromptId", CellValues.String),
                ConstructCell("EmailAddress2PromptId", CellValues.String),
                ConstructCell("Address1DefinitionId", CellValues.String),
                ConstructCell("Address2DefinitionId", CellValues.String),
                ConstructCell("Address3DefinitionId", CellValues.String),
                ConstructCell("Address4DefinitionId", CellValues.String),
                ConstructCell("Address5DefinitionId", CellValues.String),
                ConstructCell("PostCodeDefinitionId", CellValues.String),
                ConstructCell("EmailAddress1DefinitionId", CellValues.String),
                ConstructCell("EmailAddress2DefinitionId", CellValues.String),
            #endregion Recipient Details
            #region Vocabulary Details
                ConstructCell("AppearedByVideoLink", CellValues.String),
                ConstructCell("AppearedInPerson", CellValues.String),
                ConstructCell("AnyAppearance", CellValues.String),
                ConstructCell("InCustody", CellValues.String),                
                ConstructCell("CustodyLocationIsPrison", CellValues.String),
                ConstructCell("CustodyLocationIsPolice", CellValues.String),
                ConstructCell("IgnoreCustody", CellValues.String),
                ConstructCell("AllNonCustodialResults", CellValues.String),
                ConstructCell("AtleastOneNonCustodialResult", CellValues.String),
                ConstructCell("AtleastOneCustodialResult", CellValues.String),
                ConstructCell("IgnoreResults", CellValues.String),
                ConstructCell("YouthDefendant", CellValues.String),
                ConstructCell("AdultDefendant", CellValues.String),
                ConstructCell("AdultOrYouthDefendant", CellValues.String),
                ConstructCell("WelshCourtHearing", CellValues.String),
                ConstructCell("EnglishCourtHearing", CellValues.String),
                ConstructCell("AnyCourtHearing", CellValues.String),
                ConstructCell("IsProsecutedByCPS", CellValues.String),
                ConstructCell("IncludedResults", CellValues.String),
                ConstructCell("ExcludedResults", CellValues.String),
                ConstructCell("IncludedPrompts", CellValues.String),
                ConstructCell("ExcludedPrompts", CellValues.String),                
            #endregion Vocabulary Details
                ConstructCell("Start Date", CellValues.String),
                ConstructCell("End Date", CellValues.String),
                ConstructCell("Created Date", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String),
                ConstructCell("Deleted Date", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            foreach (var nowSubscription in AllData.NowSubscriptions.OrderBy(x=>x.Name))
            {
                var row = new Row();
                row.Append(
                    ConstructCell(AsString(nowSubscription.UUID), CellValues.String),
                    ConstructCell(nowSubscription.Name, CellValues.String),
                    ConstructCell(AsString(nowSubscription.ParentNowSubscriptionId), CellValues.String),
                    ConstructCell(AsString(nowSubscription.IsNow), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.IsEDT), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.IsInformantRegister), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.IsCourtRegister), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.IsPrisonCourtRegister), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.IsFirstClassLetter), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.IsSecondClassLetter), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.IsEmail), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.IsForDistribution), CellValues.Boolean),
                    ConstructCell(AsString(nowSubscription.UserGroupVariants), CellValues.String),
                    ConstructCell(nowSubscription.InformantCode, CellValues.String),
                    ConstructCell(nowSubscription.YOTsCode, CellValues.String),
                    ConstructCell(AsString(nowSubscription.SelectedCourtHouses), CellValues.String)
                    );
            #region Recipient Details
                if (nowSubscription.NowRecipient == null)
                {
                    row.Append(
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String)
                        );
                }
                else
                {
                    row.Append(
                        ConstructCell(AsString(nowSubscription.NowRecipient.RecipientFromCase), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.RecipientFromSubscription), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.RecipientFromResults), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.IsApplyDefenceOrganisationDetails), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.IsApplyParentGuardianDetails), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.IsApplyApplicantDetails), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.IsApplyDefendantDetails), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.IsApplyRespondentDetails), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.IsApplyThirdPartyDetails), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.IsApplyDefendantCustodyDetails), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.NowRecipient.IsApplyProsecutionAuthorityDetails), CellValues.Boolean),
                        ConstructCell(nowSubscription.NowRecipient.Title, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.FirstName, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.MiddleName, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.LastName, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.OrganisationName, CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.TitleResultPrompt == null ? null : nowSubscription.NowRecipient.TitleResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.FirstNameResultPrompt == null ? null : nowSubscription.NowRecipient.FirstNameResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.MiddleNameResultPrompt == null ? null : nowSubscription.NowRecipient.MiddleNameResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.LastNameResultPrompt == null ? null : nowSubscription.NowRecipient.LastNameResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.OrganisationNameResultPrompt == null ? null : nowSubscription.NowRecipient.OrganisationNameResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.TitleResultPrompt == null ? null : nowSubscription.NowRecipient.TitleResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.FirstNameResultPrompt == null ? null : nowSubscription.NowRecipient.FirstNameResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.MiddleNameResultPrompt == null ? null : nowSubscription.NowRecipient.MiddleNameResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.LastNameResultPrompt == null ? null : nowSubscription.NowRecipient.LastNameResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.OrganisationNameResultPrompt == null ? null : nowSubscription.NowRecipient.OrganisationNameResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.Address1, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.Address2, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.Address3, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.Address4, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.Address5, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.PostCode, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.EmailAddress1, CellValues.String),
                        ConstructCell(nowSubscription.NowRecipient.EmailAddress2, CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address1ResultPrompt == null ? null : nowSubscription.NowRecipient.Address1ResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address2ResultPrompt == null ? null : nowSubscription.NowRecipient.Address2ResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address3ResultPrompt == null ? null : nowSubscription.NowRecipient.Address3ResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address4ResultPrompt == null ? null : nowSubscription.NowRecipient.Address4ResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address5ResultPrompt == null ? null : nowSubscription.NowRecipient.Address5ResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.PostCodeResultPrompt == null ? null : nowSubscription.NowRecipient.PostCodeResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.EmailAddress1ResultPrompt == null ? null : nowSubscription.NowRecipient.EmailAddress1ResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.EmailAddress2ResultPrompt == null ? null : nowSubscription.NowRecipient.EmailAddress2ResultPrompt.ResultPromptUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address1ResultPrompt == null ? null : nowSubscription.NowRecipient.Address1ResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address2ResultPrompt == null ? null : nowSubscription.NowRecipient.Address2ResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address3ResultPrompt == null ? null : nowSubscription.NowRecipient.Address3ResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address4ResultPrompt == null ? null : nowSubscription.NowRecipient.Address4ResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.Address5ResultPrompt == null ? null : nowSubscription.NowRecipient.Address5ResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.PostCodeResultPrompt == null ? null : nowSubscription.NowRecipient.PostCodeResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.EmailAddress1ResultPrompt == null ? null : nowSubscription.NowRecipient.EmailAddress1ResultPrompt.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(AsString(nowSubscription.NowRecipient.EmailAddress2ResultPrompt == null ? null : nowSubscription.NowRecipient.EmailAddress2ResultPrompt.ResultDefinitionUUID), CellValues.String)
                        );
                }
            #endregion Recipient Details
            #region Vocabulary Details
                if (nowSubscription.SubscriptionVocabulary == null)
                {
                    row.Append(
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String),
                        ConstructCell(string.Empty, CellValues.String)
                        );
                }
                else
                {
                    List<Guid?> includedPromptIds = null;
                    if (nowSubscription.SubscriptionVocabulary.IncludedPromptRules != null && nowSubscription.SubscriptionVocabulary.IncludedPromptRules.Where(x => x.DeletedDate == null).Any())
                    {
                        includedPromptIds = nowSubscription.SubscriptionVocabulary.IncludedPromptRules.Where(x => x.DeletedDate == null).Select(x => x.ResultPrompt.UUID).ToList();
                    }

                    List<Guid?> excludedPromptIds = null;
                    if (nowSubscription.SubscriptionVocabulary.ExcludedPromptRules != null && nowSubscription.SubscriptionVocabulary.ExcludedPromptRules.Where(x => x.DeletedDate == null).Any())
                    {
                        excludedPromptIds = nowSubscription.SubscriptionVocabulary.ExcludedPromptRules.Where(x => x.DeletedDate == null).Select(x => x.ResultPrompt.UUID).ToList();
                    }

                    List<Guid?> includedResultIds = null;
                    if (nowSubscription.SubscriptionVocabulary.IncludedResults != null && nowSubscription.SubscriptionVocabulary.IncludedResults.Where(x => x.DeletedDate == null).Any())
                    {
                        includedPromptIds = nowSubscription.SubscriptionVocabulary.IncludedResults.Where(x => x.DeletedDate == null).Select(x => x.UUID).ToList();
                    }

                    List<Guid?> excludedResultIds = null;
                    if (nowSubscription.SubscriptionVocabulary.ExcludedResults != null && nowSubscription.SubscriptionVocabulary.ExcludedResults.Where(x => x.DeletedDate == null).Any())
                    {
                        excludedResultIds = nowSubscription.SubscriptionVocabulary.ExcludedResults.Where(x => x.DeletedDate == null).Select(x => x.UUID).ToList();
                    }

                    row.Append(
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AppearedByVideoLink), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AppearedInPerson), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AnyAppearance), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.InCustody), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.CustodyLocationIsPrison), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.CustodyLocationIsPolice), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.IgnoreCustody), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AllNonCustodialResults), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AtleastOneNonCustodialResult), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AtleastOneCustodialResult), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.IgnoreResults), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.YouthDefendant), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AdultDefendant), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AdultOrYouthDefendant), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.WelshCourtHearing), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.EnglishCourtHearing), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.AnyCourtHearing), CellValues.Boolean),
                        ConstructCell(AsString(nowSubscription.SubscriptionVocabulary.IsProsecutedByCPS), CellValues.Boolean),
                        ConstructCell(AsString(includedResultIds), CellValues.String),
                        ConstructCell(AsString(excludedResultIds), CellValues.String),
                        ConstructCell(AsString(includedPromptIds), CellValues.String),
                        ConstructCell(AsString(excludedPromptIds), CellValues.String)
                        );
                }
            #endregion Vocabulary Details
                row.Append(
                    ConstructDateCell(nowSubscription.StartDate),
                    ConstructDateCell(nowSubscription.EndDate),
                    ConstructDateCell(nowSubscription.CreatedDate),
                    ConstructDateCell(nowSubscription.LastModifiedDate),
                    ConstructDateCell(nowSubscription.DeletedDate),
                    ConstructCell(GetPublishedStatus(nowSubscription).GetDescription(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
