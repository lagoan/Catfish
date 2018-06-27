﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Piranha.Extend;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;
using System.Web.Mvc;
using Catfish.Core.Models;
using Catfish.Core.Services;
using Catfish.Core.Models.Forms;
using Catfish.Services;

namespace Catfish.Models.Regions
{
    public class QueryResultObject
    {  
        public decimal calculatedValue { get; set; }
        public int count { get; set; }
    }

    [Export(typeof(IExtension))]
    [ExportMetadata("InternalId", "CalculatedFieldPanel")]
    [ExportMetadata("Name", "CalculatedFieldPanel")]
    [ExportMetadata("Type", ExtensionType.Region)]
    [Serializable]
    public class CalculatedFieldPanel : CatfishRegion
    {
       
        public enum FunctionMode
        {
            SUM=0, COUNT, MEAN, MAX, MIN
        }
        [ScriptIgnore]
        public List<SelectListItem> FunctionModes { get; set; }

        public string SelectedFunction { get; set; }

        [ScriptIgnore]
        public List<SelectListItem> ListMetadataSets { get; set; }

        [ScriptIgnore]
        public List<SelectListItem> MetadataFields { get; set; }
       
        public string SelectedFieldMetadataSet { get; set; }
        public string SelectedFilterMetadataSet { get; set; }
        public string SelectedField { get; set; }
        public string selectedFilterField { get; set; }

        [Display(Name = "Min Parameter")]
        public string Min_Parameter { get; set; }
        [Display(Name = "Max Parameter")]
        public string Max_Parameter { get; set; }
       
        public string Title { get; set; }

        public decimal Result { get; set; }
        [ScriptIgnore]
        public int MinValue { get; set; }

        [ScriptIgnore]
        public int MaxValue { get; set; }

        public CalculatedFieldPanel()
        {
            ListMetadataSets = new List<SelectListItem>();
            MetadataFields = new List<SelectListItem>();
            FunctionModes = new List<SelectListItem>();
            foreach (FunctionMode am in Enum.GetValues(typeof(FunctionMode)))
            {
                FunctionModes.Add(new SelectListItem { Text = am.ToString(), Value = am.ToString() });
            }

            Result = 0m;
            MinValue = DateTime.MinValue.Year;
            MaxValue = DateTime.Now.Year;
        }
        public override void InitManager(object model)
        {
            // get db context
            CatfishDbContext db = new CatfishDbContext();
            MetadataService metadataService = new MetadataService(db);
            IQueryable<CFMetadataSet> metadataSets = metadataService.GetMetadataSets();
            foreach (CFMetadataSet m in metadataSets)
            {
                ListMetadataSets.Add(new SelectListItem { Text = m.Name, Value = m.Guid });
            }

            //set the default metadatasEt Fields to the first metadataSet in the list
            if (metadataSets.Count() > 0)
            {
                if (metadataSets.ToArray()[0].Fields.Count > 0)
                {
                    foreach (FormField f in metadataSets.ToArray()[0].Fields)
                    {
                        MetadataFields.Add(new SelectListItem { Text = f.Name, Value = f.Guid });
                    }
                }
            }

            base.InitManager(model);
        }

        public override object GetContent(object model)
        {
            ////For testing -- go to the page that use this region and add ?entity=[entityId]
            HttpContext context = HttpContext.Current;

            if (context != null && SelectedFieldMetadataSet != null)
            {
                string minParam = context.Request.QueryString[Min_Parameter];
                string maxParam = context.Request.QueryString[Max_Parameter];
               

                int min = minParam == null ? int.MinValue : int.Parse(minParam);
                int max = maxParam == null ? int.MaxValue : int.Parse(maxParam);
              

                string functionName = SelectedFunction.ToString();

                if (functionName == FunctionMode.MEAN.ToString())
                    functionName = "AVG";

                ItemQueryService itemQuerySrv = new ItemQueryService();
               
                var result = itemQuerySrv.GetCalculatedField(functionName, SelectedFieldMetadataSet, SelectedField, SelectedFilterMetadataSet, selectedFilterField, min, max);
              
                foreach (var r in result)
                {       if (functionName == FunctionMode.COUNT.ToString())
                        Result = r.count;
                    else
                        Result = r.calculatedValue;    
                }
            }

            return base.GetContent(model);
        }

       
    }
}