using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using Sitecore.Web.Authentication;
using System;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Sitecore.Support.EmailCampaign.Server.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal sealed class SitecoreAuthorizeAttribute : AuthorizeAttribute
    {
        internal interface ITicketManager
        {
            bool IsCurrentTicketValid();
        }

        private class TicketManagerWrapper : SitecoreAuthorizeAttribute.ITicketManager
        {
            public bool IsCurrentTicketValid()
            {
                return Sitecore.Web.Authentication.TicketManager.IsCurrentTicketValid();
            }
        }

        private static readonly SitecoreAuthorizeAttribute.ITicketManager TicketManager = new SitecoreAuthorizeAttribute.TicketManagerWrapper();

        public bool AdminsOnly
        {
            get;
            set;
        }

        public SitecoreAuthorizeAttribute(params string[] roles)
        {
            base.Roles = string.Join(",", roles);
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            Assert.ArgumentNotNull(actionContext, "actionContext");
            bool arg_4A_0 = base.IsAuthorized(actionContext) && !this.AdminsOnly;
            User user = actionContext.ControllerContext.RequestContext.Principal as User;
            bool flag = user != null && user.IsAdministrator;
            return (arg_4A_0 | flag) && SitecoreAuthorizeAttribute.TicketManager.IsCurrentTicketValid();
        }
    }
}