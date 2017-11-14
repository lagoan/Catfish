﻿using System;
using System.Collections;
using System.Collections.Generic;
using Catfish.Core.Models.Metadata;
using System.Web.Script.Serialization;
using Catfish.Core.Models.Attributes;
using System.Xml.Linq;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;


namespace Catfish.Core.Models
{
    public class Entity : XmlModel
    {
        public int? EntityTypeId { get; set; }
        public virtual EntityType EntityType { get; set; }

        public Entity()
        {
            Data.Add(new XElement("metadata-sets"));
        }

        [NotMapped]
        public List<MetadataSet> MetadataSets
        {
            get
            {
                return GetChildModels("metadata-sets/metadata-set", Data).Select(c => c as MetadataSet).ToList();
            }

            set
            {
                //Removing all children inside the metadata set element
                RemoveAllElements("metadata-sets/metadata-set", Data);

                foreach (MetadataSet ms in value)
                    InsertChildElement("./metadata-sets", ms.Data);
            }
        }

        public string GetName(string lang = null)
        {
            var mapping = EntityType.GetNameMapping();
            if(mapping != null)
            {
                string msName = mapping.MetadataSet.Name;
                string fieldName = mapping.FieldName;
                MetadataSet metadataSet = MetadataSets.Where(ms => ms.Name == msName).FirstOrDefault();
                MetadataField field = metadataSet.Fields.Where(f => f.Name == fieldName).FirstOrDefault();
                return field != null ? field.Value : "ERROR: INCORRECT NAME MAPPING FOUND FOR THIS ENTITY TYPE";
            }

            return GetChildText("name", Data, Lang(lang));
        }
        public override void SetName(string val, string lang = null)
        {
            EntityTypeAttributeMapping mapping = EntityType.GetNameMapping();
            if (mapping == null)
                throw new Exception("Name mapping metadata set is not specified for this entity type");

            if (string.IsNullOrEmpty(mapping.FieldName))
                throw new Exception("Field is not specified in the Name Mapping of this entity type");

            MetadataSet metadataSet = MetadataSets.Where(ms => ms.Name == mapping.MetadataSet.Name).FirstOrDefault();
            metadataSet.SetFieldValue(mapping.FieldName, val, lang);
        }
        public override string GetDescription(string lang = null)
        {
            var mapping = EntityType.GetDescriptionMapping();
            if (mapping != null)
            {
                string msName = mapping.MetadataSet.Name;
                string fieldName = mapping.FieldName;
                MetadataSet metadataSet = MetadataSets.Where(ms => ms.Name == msName).FirstOrDefault();
                MetadataField field = metadataSet.Fields.Where(f => f.Name == fieldName).FirstOrDefault();
                return field != null ? field.Value : "ERROR: INCORRECT DESCRIPTION MAPPING FOUND FOR THIS ENTITY TYPE";
            }

            return GetChildText("description", Data, Lang(lang));
        }
        public override void SetDescription(string val, string lang = null)
        {
            EntityTypeAttributeMapping mapping = EntityType.GetDescriptionMapping();
            if (mapping == null)
                throw new Exception("Description mapping metadata set is not specified for this entity type");

            if (string.IsNullOrEmpty(mapping.FieldName))
                throw new Exception("Field is not specified in the Description Mapping of this entity type");

            MetadataSet metadataSet = MetadataSets.Where(ms => ms.Name == mapping.MetadataSet.Name).FirstOrDefault();
            metadataSet.SetFieldValue(mapping.FieldName, val, lang);
        }

        public override void UpdateValues(XmlModel src)
        {
            base.UpdateValues(src);

            var src_item = src as Entity;

            foreach (MetadataSet ms in this.MetadataSets)
            {
                var src_ms = src_item.MetadataSets.Where(x => x.Ref == ms.Ref).FirstOrDefault();
                ms.UpdateValues(src_ms);
            }

            //this.Serialize();
        }

        public string Name
        {
            get
            {
                return GetName();
            }
        }
        public string Description
        {
            get
            {
                return GetDescription();
            }
        }

    }
}