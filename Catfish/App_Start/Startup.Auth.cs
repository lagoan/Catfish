using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catfish
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/UserAccount/Login"),
                SlidingExpiration = true
            });
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = GetGoogleClientId,// "898088459362-l9k9fdejv2hci3murhc15v1om69jjmdl.apps.googleusercontent.com",
                ClientSecret = GetGoogleClientSecret,// "Cyx3v3B_BKbx-Y2ZI8LhvDmj",
                CallbackPath = new PathString("/GoogleLoginCallback")
            });
        }

        public static string GetGoogleClientSecret
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["GoogleClientSecret"]; }
        }
        public static string GetGoogleClientId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["GoogleClientId"]; }
        }
    }
}