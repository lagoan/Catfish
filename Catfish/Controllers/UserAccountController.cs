using Catfish.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Piranha.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Catfish.Controllers
{
    public class UserAccountController : Controller
    {
        // GET: UserAccount
        public ActionResult Index()
        {
            return View();
        }

    #region LOGIN

        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.AllowLocalLogin = AllowLocalLogin();
            ViewBag.AllowGoogleLogin = AllowGoogleLogin();
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            bool success = false;
            ViewBag.AllowLocalLogin = AllowLocalLogin();
            ViewBag.AllowGoogleLogin = AllowGoogleLogin();

            if (ModelState.IsValid)
            {
                using (var db = new Piranha.DataContext())
                {
                    //login sysuser into current context
                    db.LoginSys();

                    //var usr = db.Users.SqlQuery("select * from dbo.sysuser where sysuser_login=@login", new SqlParameter("@login", model.Login));
                    success = Piranha.Models.SysUser.LoginUser(model.Login, model.Password);

                    //logout : Piranha.Models.SysUser.LogoutUser();
                    if(success)
                    {
                        return Redirect("~/");
                    }
                }
            }
            return View();
        }

        public void ExternalLogin(string ReturnUrl = "/", string type = "")
        {
            if (!Request.IsAuthenticated)
            {
                if (type == "Google")
                {
                    HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "UserAccount/GoogleLoginCallback/" }, "Google");
                }
            }
        }
        [AllowAnonymous]

        public ActionResult GoogleLoginCallback()
        {
            var claimsPrincipal = HttpContext.User.Identity as ClaimsIdentity;
            var loginInfo = GoogleLoginViewModel.GetLoginInfo(claimsPrincipal);
            if (loginInfo == null)
            {
                return RedirectToAction("Index");
            }

            var user = GetUserByName(loginInfo.name);
            if (user == null)
            {
                if (AllowAnonymousLogin())
                {
                    //create local account and log he in
                }
                else
                {
                    return RedirectToAction("AuthenticationFailure");
                }
            }
            else
            {
                //log he in
                bool success = Piranha.Models.SysUser.LoginUser(user.Login, user.Password);
            }


            //         var ident = new ClaimsIdentity(
            //           new[] {
            // // adding following 2 claim just for supporting default antiforgery provider
            //// new Claim(ClaimTypes.NameIdentifier,loginInfo.emailaddress),
            //// new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"),
            // new Claim(ClaimTypes.Name, loginInfo.name),
            // new Claim(ClaimTypes.Email, loginInfo.emailaddress),
            // // optionally you could add roles if any
            // new Claim(ClaimTypes.Role, "User")
            //           },
            //         CookieAuthenticationDefaults.AuthenticationType);


            //         // HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            //         HttpContext.GetOwinContext().Authentication.SignIn(
            //         new AuthenticationProperties { IsPersistent = false }, ident);
            //         return Redirect("~/");
            return View();
        }

        // GET: UserAccount
        public ActionResult AuthenticationFailure()
        {
            return View();
        }

        public ActionResult Logout()
        {
            var currUserClaim = HttpContext.User.Identity as ClaimsIdentity;

            System.Web.HttpContext.Current.GetOwinContext().Authentication.SignOut();//logout google account from our app

            FormsAuthentication.SignOut(); //logout local account -- remove form authentication from browser

            Session.Clear();  // This may not be needed -- but can't hurt
            Session.Abandon();

            // Clear authentication cookie
            HttpCookie rFormsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            rFormsCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(rFormsCookie);

            // Clear session cookie 
            HttpCookie rSessionCookie = new HttpCookie("ASP.NET_SessionId", "");
            rSessionCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(rSessionCookie);

            // Invalidate the Cache on the Client Side
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Response.Cache.SetNoStore();
            return Redirect("~/");
        }

        #endregion

     #region Create User
        [HttpGet]
        public ActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(UserViewModel uservm)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Piranha.DataContext())
                {
                    // Login sysuser into the current context.
                    db.LoginSys();

                    var user = new Piranha.Entities.User()
                    {
                        Login = uservm.Login,
                        Email = uservm.Email,
                        Firstname = uservm.FirstName,
                        Surname = uservm.LastName,
                        GroupId =new Guid() // Here you need to add the id to a custom group for front end users.
                    };

                    if (!String.IsNullOrEmpty(uservm.Password))
                    {
                        user.Password = Piranha.Models.SysUserPassword.Encrypt(uservm.Password);
                    }
                    db.Users.Add(user);

                    if (db.SaveChanges() > 0)
                    {
                        // Make sure that you have implemented the Hook Hooks.Mail.SendPassword
                        if (String.IsNullOrEmpty(uservm.Password))
                        {
                            // user.GenerateAndSendPassword(db);
                            //url redirect to ResetPassword page
                            var callbackUrl = Url.Action("ResetPassword", "UserAccount", new { userId = user.Id}, protocol: Request.Url.Scheme);
                            string message = "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>";
                            string subject="Reset Password";
                            string email= System.Configuration.ConfigurationManager.AppSettings["Email"];
                            SendEmail(email, callbackUrl, subject, message);
                        }
                    }
                    else
                    {//error saving user
                        return View("YourFailureView");
                    }
                }
                return View("YourSuccessView");
            }

            //invalid modelview
            return View("YourFailureView");
        }


        #endregion

        //Helpers methods
        private bool ResetPassword(string email, string password)
        {
            bool bsuccess = false;
            User user = new Piranha.Entities.User();
            user = GetUserByEmail(email);
            if(user != null)
            {
                user.Password = Piranha.Models.SysUserPassword.Encrypt(password);
                using (var db = new Piranha.DataContext())
                {
                    //db.LoginSys();
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    bsuccess = true;
                }
            }

            return bsuccess;
        }

        /// <summary>
        /// get piranha User by Login name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// 
        public User GetUserByName(string name)
        {
            User usr = null;
            using (var db = new Piranha.DataContext())
            {
                usr = db.Users.Where(u => u.Login == name).FirstOrDefault();
            }

            return usr;
        }

        /// <summary>
        /// get piranha User by Login name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public User GetUserByEmail(string email)
        {
            User usr = null;
            using (var db = new Piranha.DataContext())
            {
                usr = db.Users.Where(u => u.Email == email).FirstOrDefault();
            }

            return usr;
        }

        private void SendEmail(string email, string callbackUrl, string subject, string message)
        {
            // For information on sending mail, please visit http://go.microsoft.com/fwlink/?LinkID=320771


            MailMessage msg = new MailMessage();
            // string fromEmail = System.Configuration.ConfigurationManager.AppSettings["EmailSender"];
            msg.From = new MailAddress("melaniar@hotmail.com");
            msg.To.Add(new MailAddress(email));
            msg.Subject = subject;
            msg.Body = message;
            // msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            // msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(587));
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("arcdev8@gmail.com", "4rcDev8@");
            //  smtpClient.Credentials = credentials;
            smtpClient.UseDefaultCredentials = true;
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);

        }

        public bool AllowLocalLogin()
        {
            return Convert.ToBoolean( System.Configuration.ConfigurationManager.AppSettings["AllowLocalLogin"]);
        }
        public bool AllowGoogleLogin()
        {
            return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["AllowGoogleLogin"]);
        }
        public bool AllowAnonymousLogin()
        {
            return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["AllowAnonymousLogin"]);
        }
    }
}
