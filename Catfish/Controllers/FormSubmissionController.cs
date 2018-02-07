﻿using Catfish.Areas.Manager.Models.ViewModels;
using Catfish.Core.Models;
using Catfish.Core.Models.Data;
using Catfish.Core.Services;
using Catfish.Models.Regions;
using Catfish.Models.ViewModels;
using Piranha.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catfish.Controllers
{
    public class FormSubmissionController : CatfishSinglePageController
    {
        // GET: Forms
        public ActionResult Index()
        {
            var model = GetModel();
            ViewBag.PageModel = model;

            FormContainer formContainer = model.Region<FormContainer>("FormContainer");

            Form form = SubmissionService.CreateSubmissionForm(formContainer.FormId);

            FormViewModel vm = new FormViewModel()
            {
                Form = form,
                ItemId = 0
            };

            //////////inserting some test files to the form
            ////////vm.Files = Db.XmlModels.Where(xml => xml is DataFile && xml.Id > 1520).Take(2).Select(xml => xml as DataFile).ToList();
            ////////var field = form.Fields.Where(f => f is Catfish.Core.Models.Forms.Attachment).Select(f => f as Catfish.Core.Models.Forms.Attachment).FirstOrDefault();
            ////////field.FileGuids = string.Join("|", vm.Files.Select(f => f.Guid).ToList());

            return View(model.GetView(), vm);
        }

        [HttpPost]
        public ActionResult Edit(FormViewModel vm)
        {
            var model = GetModel();

            if (ModelState.IsValid)
            {
                FormContainer formContainer = model.Region<FormContainer>("FormContainer");
                Item submission = SubmissionService.SaveSubmission(
                    vm.Form,
                    vm.FormSubmissionRef,
                    vm.ItemId,
                    formContainer.EntityTypeId,
                    formContainer.FormId,
                    formContainer.CollectionId);

                AuditEntry.eAction action = submission.Id == 0 ? AuditEntry.eAction.Create : AuditEntry.eAction.Update;
                string actor = User.Identity.IsAuthenticated ? User.Identity.Name : "Annonymous";
                Db.SaveChanges(User.Identity);

                string confirmLink = "confirmation";
                return Redirect(confirmLink);
            }

            ViewBag.PageModel = model;
            return View(model.GetView(), vm);
        }

        public ActionResult Confirmation()
        {
            var model = GetModel();
            return View(model);
        }
    }
}