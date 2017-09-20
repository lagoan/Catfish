﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Catfish.Areas.Manager.Models.ViewModels;
using Catfish.Core.Models.Metadata;
using Piranha.Areas.Manager.Controllers;

namespace Catfish.Areas.Manager.Controllers
{
    public class MetadataController : CatfishController
    {
        public ActionResult Index()
        {
            return View(MetadataService.GetMetadataSets());
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            MetadataSet model = MetadataService.GetMetadataSet(id);
            if (model == null)
                return HttpNotFound();

            return View(new MetadataDefinition(model, model.Id));
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            MetadataSet model;
            if (id.HasValue)
                model = MetadataService.GetMetadataSet(id.Value);
            else
                model = new MetadataSet();

            return View(model);
        }


        [HttpPost]
        public JsonResult AddField(MetadataSetViewModel vm)
        {
            foreach (MetadataFieldType t in vm.SelectedFieldTypes)
            {
                Type type = Type.GetType(t.FieldType, true);
                if (!typeof(MetadataField).IsAssignableFrom(type))
                    throw new InvalidOperationException("Bad Type");

                MetadataField field = Activator.CreateInstance(type) as MetadataField;
                vm.Fields.Add(new MetadataFieldViewModel(field));
            }
            vm.SelectedFieldTypes.Clear();
            return Json(vm);
        }

        [HttpPost]
        public JsonResult RemoveField(MetadataSetViewModel vm, int idx)
        {
            vm.Fields.RemoveAt(idx);
            return Json(vm);
        }

        [HttpPost]
        public JsonResult Move(MetadataSetViewModel vm, int idx, int step)
        {
            int newIdx = idx + step;
            if (newIdx < 0)
                newIdx = 0;
            if (newIdx >= vm.Fields.Count)
                newIdx = vm.Fields.Count - 1;

            if(idx != newIdx)
            {
                var field = vm.Fields.ElementAt(idx);
                vm.Fields.RemoveAt(idx);
                vm.Fields.Insert(newIdx, field);
            }
            return Json(vm);
        }

        [HttpPost]
        public JsonResult Save(MetadataSetViewModel vm)
        {
            if (ModelState.IsValid)
            {
                MetadataSet ms;

                if (vm.Id > 0)
                {
                    ms = Db.MetadataSets.Where(x => x.Id == vm.Id).FirstOrDefault();
                    if (ms == null)
                    {
                        vm.Status = KoBaseViewModel.eStatus.Error;
                        vm.Message = "Specified metadata set not found";
                        return Json(vm);
                    }
                    else
                    {
                        vm.UpdateMetadataSet(ms);
                        Db.Entry(ms).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                else
                {
                    ms = new MetadataSet();
                    vm.UpdateMetadataSet(ms);
                    Db.MetadataSets.Add(ms);
                }

                ms.Serialize();
                Db.SaveChanges();
                vm.Status = KoBaseViewModel.eStatus.Success;
            }
            else
            {
                vm.Status = KoBaseViewModel.eStatus.Success;
                vm.Message = "Model validation failed";
            }

            return Json(vm);
        }

    }
}