﻿namespace Sitecore.Support.EmailCampaign.Server.Conrollers.Language
{
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.EmailCampaign.Model.Web;
    using Sitecore.EmailCampaign.Server.Contexts;
    using Sitecore.EmailCampaign.Server.Filters;
    using Sitecore.EmailCampaign.Server.Responses;
    using Sitecore.ExM.Framework.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Modules.EmailCampaign.Core.Extensions;
    using Sitecore.Modules.EmailCampaign.Messages;
    using Sitecore.Modules.EmailCampaign.Messages.Dto;
    using Sitecore.Modules.EmailCampaign.Messages.Interfaces;
    using Sitecore.Modules.EmailCampaign.Services;
    using Sitecore.Services.Core;
    using Sitecore.Services.Infrastructure.Web.Http;
    using System;
    using System.Linq;
    using System.Web.Http;

    [SitecoreAuthorize(new string[]
    {
        "sitecore\\EXM Advanced Users",
        "sitecore\\EXM Users"
    }), ServicesController("EXM.AddLanguage")]
    public class AddLanguageController : ServicesApiController
    {
        private readonly IExmCampaignService _exmCampaignService;

        private readonly ILogger logger;

        private readonly ILanguageRepository _languageRepository;

        public AddLanguageController(IExmCampaignService exmCampaignService, ILogger logger, ILanguageRepository languageRepository)
        {
            Assert.ArgumentNotNull(exmCampaignService, "exmCampaignService");
            Assert.ArgumentNotNull(logger, "logger");
            Assert.ArgumentNotNull(languageRepository, "languageRepository");
            this._exmCampaignService = exmCampaignService;
            this.logger = logger;
            this._languageRepository = languageRepository;
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
                AddLanguageInfo addLanguageResult = this._languageRepository.AddLanguage(data.MessageId.IgnoreHashes(), data.NewLanguage);
                if (addLanguageResult == null || addLanguageResult.Variants == null || addLanguageResult.MessageLanguage == null || addLanguageResult.Error)
                {
                    addLanguageResponse.ErrorMessage = EcmTexts.Localize("Failed to add the language.", Array.Empty<object>());
                }
                else
                {
                    addLanguageResponse.Error = false;
                    addLanguageResponse.ErrorMessage = string.Format(EcmTexts.Localize("The {0} language has been created.", Array.Empty<object>()), addLanguageResult.MessageLanguage.DisplayName);
                    addLanguageResponse.VariantMessages = (from variant in addLanguageResult.Variants
                                                           select string.Format(EcmTexts.Localize("The {0} version has been added to variant {1}.", Array.Empty<object>()), addLanguageResult.MessageLanguage.DisplayName, variant.Name)).ToArray<string>();
                    MessageItem messageItem = this._exmCampaignService.GetMessageItem(Guid.Parse(data.MessageId), data.Language);
                    if (messageItem != null && messageItem.Attachments.Any<MediaItem>())
                    {
                        addLanguageResponse.NotificationMessages = new MessageBarMessageContext[]
                        {
                            new MessageBarMessageContext
                            {
                                ActionLink = string.Format("trigger:language:copyattachmentfromlanguage({{\"fromLanguage\":\"{0}\", \"toLanguage\":\"{1}\"}})", data.Language, data.NewLanguage),
                                ActionText = EcmTexts.Localize("click here.", Array.Empty<object>()),
                                Message = EcmTexts.Localize(string.Format("The {0} language version of this message contains attachments. If you want to copy the attachments to the {1} language version of the message,", Language.Parse(data.Language).CultureInfo.DisplayName, Language.Parse(data.NewLanguage).CultureInfo.DisplayName), Array.Empty<object>())
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message, ex);
                addLanguageResponse.Error = true;
                addLanguageResponse.ErrorMessage = EcmTexts.Localize("A serious error occurred please contact the administrator", Array.Empty<object>());
            }
            return addLanguageResponse;
        }
    }
}
