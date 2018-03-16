﻿using System;
using System.Collections;
using System.Collections.Generic;
using Catfish.Core.Models.Forms;
using System.Web.Script.Serialization;
using Catfish.Core.Models.Attributes;
using System.Xml.Linq;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Catfish.Core.Helpers;

namespace Catfish.Core.Models
{
    public abstract class Entity : XmlModel
    {
        public int? EntityTypeId { get; set; }
        public virtual EntityType EntityType { get; set; }

        public Entity()
        {
            Data.Add(new XElement("metadata"));
        }

        [NotMapped]
        public List<MetadataSet> MetadataSets
        {
            get
            {
                return GetChildModels("metadata/metadata-set", Data).Select(c => c as MetadataSet).ToList();
            }

            set
            {
                RemoveAllMetadataSets();
                InitMetadataSet(value);
            }

        }

        public void RemoveAllMetadataSets()
        {
            //Removing all children inside the metadata set element
            RemoveAllElements("metadata/metadata-set", Data);
        }

        public void InitMetadataSet(IReadOnlyList<MetadataSet> src)
        {
            XElement metadata = GetImmediateChild("metadata");
            foreach (MetadataSet ms in src)
                metadata.Add(ms.Data);
        }

        protected FormField GetMetadataSetField(string metadatasetGuid, string fieldName) 
        {
            MetadataSet metadataSet = MetadataSets.Where(ms => ms.Guid == metadatasetGuid).FirstOrDefault();
            FormField field = metadataSet.Fields.Where(f => f.Name == fieldName).FirstOrDefault();

            return field;
        }

        public string GetName(string lang = null)
        {
            var mapping = EntityType.GetNameMapping();
            if(mapping != null)
            {
                string msGuid = mapping.MetadataSet.Guid;
                string fieldName = mapping.FieldName;

                FormField field = GetMetadataSetField(msGuid, fieldName);

                if (field == null)
                {
                    return "ERROR: INCORRECT NAME MAPPING FOUND FOR THIS ENTITY TYPE";
                   // throw new Exception("ERROR: INCORRECT NAME MAPPING FOUND FOR THIS ENTITY TYPE");
                }

                return MultilingualHelper.Join(field.GetValues(), " / ", false);
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

            MetadataSet metadataSet = MetadataSets.Where(ms => ms.Guid == mapping.MetadataSet.Guid).FirstOrDefault();
            metadataSet.SetFieldValue(mapping.FieldName, val, lang);
        }
        public override string GetDescription(string lang = null)
        {
            var mapping = EntityType.GetDescriptionMapping();
            if (mapping != null)
            {
                string msGuid = mapping.MetadataSet.Guid;
                string fieldName = mapping.FieldName;
                FormField field = GetMetadataSetField(msGuid, fieldName);

                if (field == null)
                {
                    return "ERROR: INCORRECT DESCRIPTION MAPPING FOUND FOR THIS ENTITY TYPE";
                   // throw new Exception("ERROR: INCORRECT DESCRIPTION MAPPING FOUND FOR THIS ENTITY TYPE");
                }

                return MultilingualHelper.Join(field.GetValues(), " / ", false);
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

            MetadataSet metadataSet = MetadataSets.Where(ms => ms.Guid == mapping.MetadataSet.Guid).FirstOrDefault();
            metadataSet.SetFieldValue(mapping.FieldName, val, lang);
        }

        public override void UpdateValues(XmlModel src)
        {
            if(src == this)
            {
                // Updating will delete the child content. Since it's the same, we will return without any changes.
                return;
            }

            base.UpdateValues(src);

            var src_item = src as Entity;

            foreach (MetadataSet ms in this.MetadataSets)
            {
                var src_ms = src_item.MetadataSets.Where(x => x.Guid == ms.Guid).FirstOrDefault();
                ms.UpdateValues(src_ms);
            }
        }

        public override string Name
        {
            get
            {
                return GetName();
            }
        }

        public override string Description
        {
            get
            {
                return GetDescription();
            }
        }

    }
}