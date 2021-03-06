﻿using Catfish.Areas.Manager.Models.ViewModels;
using Catfish.Core.Models;
using Catfish.Core.Models.Access;
using Catfish.Core.Services;
using Catfish.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catfish.Areas.Manager.Services
{
    public class AccessGroupService : ServiceBase
    {
        public AccessGroupService(CatfishDbContext db) : base(db) { }

        
        public EntityAccessDefinitionsViewModel UpdateViewModel(CFEntity entity)
        {
            UserService us = new UserService();
            UserListService uListService = new UserListService(Db);

            EntityAccessDefinitionsViewModel entityAccessVM = new EntityAccessDefinitionsViewModel();
            entityAccessVM.Id = entity.Id;
            entityAccessVM.EntityName = entity.Name;
            entityAccessVM.BlockInheritance = entity.BlockInheritance;

            entityAccessVM.AvailableUsers2 = us.GetUserIdAndLoginName();
            Dictionary<string, string> allUserLists = uListService.GetDictionaryUserLists();

            allUserLists.ToList().ForEach(x => entityAccessVM.AvailableUsers2.Add(x.Key, x.Value));

            AccessDefinitionService accessDefinitionService = new AccessDefinitionService(Db);
            SelectList accessDefs = new SelectList(accessDefinitionService.GetSelectListAccessDefinitions().GroupBy(a => a.Name).Select(a => a.FirstOrDefault()), "AccessModes", "StringAccessModesList");

            entityAccessVM.AvailableAccessDefinitions = accessDefs;

            entityAccessVM.AvailableAccessDefinitions2 = accessDefs.ToList();

            if (entity.AccessGroups.Count > 0)
            {

                //update SelectedAccessGroups
                foreach (CFAccessGroup gr in entity.AccessGroups)
                {
                    AccessGroup accGrp = new Models.ViewModels.AccessGroup();
                    accGrp.userId = gr.AccessGuids.FirstOrDefault().ToString();
                    var user = us.GetUserById(accGrp.userId);
                    string name = string.Empty;
                    if (user == null)
                    {
                        name = uListService.GetEntityGroup(accGrp.userId).Name;
                    }
                    else
                    {
                        name = user.Login;
                    }
                    accGrp.User = name;
                    accGrp.AccessMode = gr.AccessDefinition.StringAccessModesList;
                    accGrp.AccessModesNum = (int)gr.AccessDefinition.AccessModes;

                    entityAccessVM.SelectedAccessGroups.Add(accGrp);
                }
            }

            return entityAccessVM;

        }

        public CFEntity UpdateEntityAccessGroups(CFEntity entity, EntityAccessDefinitionsViewModel entityAccessVM)
        {
            List<CFAccessGroup> accessGroups = new List<CFAccessGroup>();
            foreach (var ag in entityAccessVM.SelectedAccessGroups)
            {
                CFAccessGroup group = new CFAccessGroup();
                group.AccessGuids = new List<Guid>() { Guid.Parse(ag.userId) };
                group.AccessDefinition.AccessModes = (AccessMode)ag.AccessModesNum;
                if(ag.AccessMode != null)
                    group.AccessDefinition.Name = ag.AccessMode.Substring(0, ag.AccessMode.LastIndexOf("-"));


                accessGroups.Add(group);
            }
            entity.AccessGroups = accessGroups;
           
            entity.BlockInheritance = entityAccessVM.BlockInheritance;

            return entity;

        }

    }
}