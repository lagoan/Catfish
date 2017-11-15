﻿using Catfish.Core.Models.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Catfish.Core.Models.Forms
{
    public abstract class Form : XmlModel
    {
        public Form()
        {
            Data.Add(new XElement("fields"));
        }

        [NotMapped]
        public List<FormField> Fields
        {
            get
            {
                return GetChildModels("fields/field", Data).Select(c => c as FormField).ToList();
            }

            set
            {
                //Removing all children inside the metadata set element
                RemoveAllElements("fields/field", Data);

                foreach (FormField ms in value)
                    InsertChildElement("./fields", ms.Data);
            }
        }

        [NotMapped]
        [TypeLabel("String")]
        public string Name { get { return GetName(); } set { SetName(value); } }

        [NotMapped]
        [DataType(DataType.MultilineText)]
        public string Description { get { return GetDescription(); } set { SetDescription(value); } }


        public override void UpdateValues(XmlModel src)
        {
            base.UpdateValues(src);

            var src_item = src as Form;

            foreach (FormField field in this.Fields)
            { // checkhere type of 
                var src_field = src_item.Fields.Where(x => x.Ref == field.Ref).FirstOrDefault();
                field.UpdateValues(src_field);
            }
        }

        public void SetFieldValue(string fieldName, string fieldValue, string language)
        {
            SetFieldValue(fieldName, new List<string> { fieldValue }, language);
        }
        public void SetFieldValue(string fieldName, IEnumerable<string> fieldValues, string language)
        {
            FormField field = Fields.Where(f => f.Name == fieldName).FirstOrDefault();
            field.SetValues(fieldValues, language);
        }

    }
}
