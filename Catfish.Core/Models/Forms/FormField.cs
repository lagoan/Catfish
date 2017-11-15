﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Catfish.Core.Models.Attributes;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Catfish.Core.Helpers;

namespace Catfish.Core.Models.Forms
{
    public class FormField : XmlModel
    {
        [NotMapped]
        public string Name
        {
            get
            {
                return GetName();
            }
            set
            {
                SetName(value);
            }
        }

        //[Display(Name = "Name")]
        public IEnumerable<TextValue> MultilingualName
        {
            get
            {
                return GetNames(true);
            }

            set
            {
                SetName(value);
            }
        }

        public override string GetTagName() { return "field"; }

        [NotMapped]
        [Display(Name="Is Required")]
        public bool IsRequired
        {
            get
            {
                if (Data != null)
                {
                    var att = Data.Attribute("IsRequired");
                    return att != null ? att.Value == "true" : false;
                }
                return false;
            }

            set
            {
                Data.SetAttributeValue("IsRequired", value);
            }
        }

        [NotMapped]
        [Display(Name="Tooltip Help")]
        [DataType(DataType.MultilineText)]
        public string Help
        {
            get
            {
                return GetHelp();
            }

            set
            {
                SetHelp(value);
            }
        }

        [NotMapped]
        public List<TextValue> Values
        {
            get
            {
                XElement valueWrapper = Data.Element("value");
                if (valueWrapper == null)
                    Data.Add(valueWrapper = new XElement("value"));

                return XmlHelper.GetTextValues(valueWrapper, true).ToList();
            }

            set
            {
                SetTextValues(value);
            }
        }

        [NotMapped]
        public string Value
        {
            get
            {
                IEnumerable<string> values = GetValues();
                return values.Any() ? values.First() : "";
            }

            set
            {
                List<string> values = new List<string>(){ value };
                SetValues(values);
            }
        }

        [DataType(DataType.MultilineText)]
        [NotMapped]
        public string Description
        {
            get
            {
                return GetDescription();
            }

            set
            {
                SetDescription(value);
            }
        }

        public override void UpdateValues(XmlModel src)
        {
            XElement srcValueWrapper = src.Data.Element("value");
            if (srcValueWrapper == null)
                return;

            IEnumerable<XElement> srcText = srcValueWrapper.Elements("text");
            if (srcText.Count() == 0)
                return;

            XElement dstValeWrapper = Data.Element("value");
            if (dstValeWrapper == null)
                Data.Add(dstValeWrapper = new XElement("value"));
            else
            {
                //deleting all existing text elements from the destination
                foreach (var txt in dstValeWrapper.Elements("text").ToList())
                    txt.Remove();
            }

            //inserting clones of text elements in the src value wrapper
            foreach (var txt in srcValueWrapper.Elements("text"))
                dstValeWrapper.Add(new XElement(txt));
        }



        ////[XmlIgnore]
        ////[HiddenInput(DisplayValue = false)]
        ////public int MetadataSetId { get; set; }

        ////////public override XElement ToXml()
        ////////{
        ////////    XElement ele = base.ToXml();

        ////////    if (IsRequired)
        ////////        ele.SetAttributeValue("IsRequired", IsRequired);

        ////////    if (!string.IsNullOrWhiteSpace(Help))
        ////////        ele.Add(new XElement("Help") { Value = Help });

        ////////    return ele;
        ////////}

        ////////public override void Initialize(XElement ele)
        ////////{
        ////////    base.Initialize(ele);
        ////////    this.IsRequired = bool.Parse(GetAtt(ele, "IsRequired", "false"));
        ////////    this.Help = GetChildText(ele, "Help");
        ////////}

    }
}