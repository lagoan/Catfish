using Catfish.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catfish.Core.Models.Access;
using Catfish.Core.Models;
using System.Configuration;

namespace Catfish.Services
{
    public class SecurityService : SecurityServiceBase
    {
        private UserService mUserService;
        private UserService UserService { get { if (mUserService == null) mUserService = new UserService(); return mUserService; } }

        public SecurityService(CatfishDbContext db) : base(db)
        {
        }

        protected override bool IsAdmin(string userGuid)
        {
            Piranha.Entities.User user = UserService.GetUserById(userGuid);

            if(user != null)
            {
                string defaultMode = ConfigurationManager.AppSettings["SecurityAdminRoleName"];

                if (string.IsNullOrEmpty(defaultMode))
                {
                    defaultMode = "ADMIN_CONTENT";
                }

                return user.Group.Permissions.Where(p => p.Name == defaultMode).Any();
            }

            return false;
        }


        public AccessMode CurrentUserPermissions(CFEntity entity)
        {
            string userGuid = UserService.GetCurrentUser().Id.ToString();
            return GetPermissions(userGuid, entity);
        }

        public bool CurrentUserHasPermissions(CFEntity entity, AccessMode accessMode)
        {
            string userGuid = UserService.GetCurrentUser().Id.ToString();
            return UserHasPermissions(userGuid, entity, accessMode);
        }

        public IEnumerable<CFEntity> CurrentUserFilterCFEntities(IEnumerable<CFEntity> entities, 
            AccessMode accessMode)
        {
            string userGuid = UserService.GetCurrentUser().Id.ToString();
            return FilterCFEntities(userGuid, entities, accessMode);
        }

    }
}