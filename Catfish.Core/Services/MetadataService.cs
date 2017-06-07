﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catfish.Core.Models;
using Catfish.Core.Models.Attributes;
using Catfish.Core.Models.Metadata;

namespace Catfish.Core.Services
{
    public class MetadataService : ServiceBase
    {
        public MetadataService(CatfishDbContext db) : base(db) { }

        public IQueryable<MetadataSet> GetMetadataSets()
        {
            return Db.MetadataSets;
        }

        public MetadataSet GetMetadataSet(int id)
        {
            return Db.MetadataSets.Where(m => m.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Returns the list of metadata filed types that can be used as metadata fileds. 
        /// This excludes base classes that are not directly usable as fields in a form.
        /// </summary>
        /// <returns></returns>
        public List<Type> GetMetadataFieldTypes()
        {
            var fieldTypes = typeof(MetadataField).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(MetadataField)) 
                    && !t.CustomAttributes.Where(a => a.AttributeType.IsAssignableFrom(typeof(IgnoreAttribute))).Any())
                .ToList();


            return fieldTypes;
        }

        public MetadataSet UpdateMetadataSet(MetadataDefinition metadataDefinition)
        {
            MetadataSet ms;
            if (metadataDefinition.Id > 0)
            {
                ms = Db.MetadataSets.Where(m => m.Id == metadataDefinition.Id).FirstOrDefault();
                if (ms == null)
                    return null;

                Db.Entry(ms).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                ms = new MetadataSet();
                Db.MetadataSets.Add(ms);
            }
            ms.Definition = metadataDefinition;
            ms.Serialize();
            return ms;
        }


    }
}
