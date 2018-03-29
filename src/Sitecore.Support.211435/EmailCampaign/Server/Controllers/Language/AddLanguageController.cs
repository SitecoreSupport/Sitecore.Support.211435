namespace Sitecore.Support.EmailCampaign.Server.Controllers.Language
{
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.EmailCampaign.Server.Contexts;
    using Sitecore.EmailCampaign.Server.Responses;
    using Sitecore.Modules.EmailCampaign;
    using Sitecore.Modules.EmailCampaign.Core;
    using Sitecore.Modules.EmailCampaign.Core.Extensions;
    using Sitecore.Modules.EmailCampaign.Messages;
    using Sitecore.Modules.EmailCampaign.Messages.Dto;
    using Sitecore.Services.Core;
    using Sitecore.Services.Infrastructure.Web.Http;
    using Sitecore.Support.EmailCampaign.Server.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    [SitecoreAuthorize(new string[]
    {
        "sitecore\\ECM Advanced Users",
        "sitecore\\ECM Users"
    }), ServicesController("EXM.SupportAddLanguage")]
    public class SupportAddLanguageController : ServicesApiController
    {
        private readonly Factory factory;

        private readonly Sitecore.ExM.Framework.Diagnostics.ILogger logger;

        public SupportAddLanguageController() : this(Factory.Instance, Sitecore.ExM.Framework.Diagnostics.Logger.Instance)
        {
        }

        public SupportAddLanguageController(Factory factory, Sitecore.ExM.Framework.Diagnostics.ILogger logger)
        {
            Assert.ArgumentNotNull(factory, "factory");
            Assert.ArgumentNotNull(logger, "logger");
            this.factory = factory;
            this.logger = logger;
        }

        [ActionName("DefaultAction")]
        public Response Add(AddLanguageContext data)
        {
            Assert.ArgumentNotNull(data, "data");
            Assert.IsNotNullOrEmpty(data.MessageId, "message context - messageId property is null or empty; for data:{0}", new object[]
            {
                data
            });
            Assert.IsNotNullOrEmpty(data.Language, "message context - language property is null or empty; for data:{0}", new object[]
            {
                data
            });
            Assert.IsNotNullOrEmpty(data.NewLanguage, "message context -new language property is null or empty; for data:{0}", new object[]
            {
                data
            });
            AddLanguageResponse addLanguageResponse = new AddLanguageResponse
            {
                Error = true,
                ErrorMessage = string.Empty
            };
            try
            {
                AddLanguageInfo addLanguageResult = new LanguageRepository().AddLanguage(data.MessageId.IgnoreHashes(), data.NewLanguage);
                if (addLanguageResult == null || addLanguageResult.Variants == null || addLanguageResult.MessageLanguage == null || addLanguageResult.Error)
                {
                    addLanguageResponse.ErrorMessage = EcmTexts.Localize("Failed to add the language.", new object[0]);
                }
                else
                {
                    addLanguageResponse.Error = false;
                    addLanguageResponse.ErrorMessage = string.Format(EcmTexts.Localize("The {0} language has been created.", new object[0]), addLanguageResult.MessageLanguage.DisplayName);
                    addLanguageResponse.VariantMessages = (from variant in addLanguageResult.Variants
                                                           select string.Format(EcmTexts.Localize("The {0} version has been added to variant {1}.", new object[0]), addLanguageResult.MessageLanguage.DisplayName, variant.Name)).ToArray<string>();
                    MessageItem messageItem = this.factory.GetMessageItem(data.MessageId, data.Language);

                    #region sitecore.support.211435
                    List<Database> listOfDatabases = Sitecore.Configuration.Factory.GetDatabases();
                    Item rootManagerItem = null;
                    Database database = null;
                    this.GetManagerRootOfMessage(listOfDatabases, data, out rootManagerItem, out database);
                    ManagerRoot myManagerRoot = Sitecore.Modules.EmailCampaign.Factory.GetManagerRootFromChildItem(rootManagerItem);
                    Item myManagerRootItem = myManagerRoot.InnerItem;
                    Sitecore.Globalization.Language newEmailLanguage = this.SetCurrentLanguage(data.NewLanguage, messageItem);
                    Item rootItemInNewLanguage = database.GetItem(myManagerRootItem.ID, newEmailLanguage);
                    Item emailItemInNewLanguage = database.GetItem(data.MessageId, newEmailLanguage);
                    emailItemInNewLanguage.Editing.BeginEdit();
                    emailItemInNewLanguage.Fields["Reply To"].Value = rootItemInNewLanguage.Fields["Reply To"].Value;
                    emailItemInNewLanguage.Fields["From Address"].Value = rootItemInNewLanguage.Fields["From Address"].Value;
                    emailItemInNewLanguage.Fields["From Name"].Value = rootItemInNewLanguage.Fields["From Name"].Value;
                    emailItemInNewLanguage.Editing.EndEdit();
                    #endregion sitecore.support.211435

                    if (messageItem != null && messageItem.Attachments.Any<MediaItem>())
                    {
                        addLanguageResponse.NotificationMessages = new MessageBarMessageContext[]
                        {
                            new MessageBarMessageContext
                            {
                                ActionLink = string.Format("trigger:language:copyattachmentfromlanguage({{\"fromLanguage\":\"{0}\", \"toLanguage\":\"{1}\"}})", data.Language, data.NewLanguage),
                                ActionText = EcmTexts.Localize("click here.", new object[0]),
                                Message = EcmTexts.Localize(string.Format("The {0} language version of this message contains attachments. If you want to copy the attachments to the {1} language version of the message,", Sitecore.Globalization.Language.Parse(data.Language).CultureInfo.DisplayName, Sitecore.Globalization.Language.Parse(data.NewLanguage).CultureInfo.DisplayName), new object[0])
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message, ex);
                addLanguageResponse.Error = true;
                addLanguageResponse.ErrorMessage = EcmTexts.Localize("A serious error occurred please contact the administrator", new object[0]);
            }
            return addLanguageResponse;
        }

        //sitecore.support.211435
        private Sitecore.Globalization.Language SetCurrentLanguage(string language, MessageItem messageItem)
        {
            Sitecore.Globalization.Language language2 = LanguageManager.GetLanguage(language);
            messageItem.TargetLanguage = language2;
            messageItem.Source.TargetLanguage = language2;
            return language2;
        }

        //sitecore.support.211435
        private void GetManagerRootOfMessage(List<Database> listOfDatabases, AddLanguageContext data, out Item rootManagerItem, out Database database)
        {
            rootManagerItem = null;
            database = null;
            for (int i = 0; i < listOfDatabases.Count; i++)
            {
                rootManagerItem = listOfDatabases[i].GetItem(data.MessageId);
                if (rootManagerItem != null)
                {
                    database = listOfDatabases[i];
                    break;
                }
            }
        }

    }
}