﻿using Catfish.Core.Models;
using Catfish.Core.Models.Data;
using Catfish.Core.Models.Forms;
using Catfish.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Catfish.Core.Models.Data.CFDataFile;

namespace Catfish.Models.ViewModels
{
    public class EntityViewModel
    {
        public int Id { get; set; }

        public string Guid { get; set; }

        public ICollection<EntityViewModel> Children { get; set; }

        public ICollection<MetadataSetViewModel> MetadataSets { get; set; }

        public ICollection<DataFileViewModel> Files { get; set; }

        public string[] LanguageCodes { get; private set; }
        
        public EntityViewModel()
        {
            Children = new List<EntityViewModel>();
            MetadataSets = new List<MetadataSetViewModel>();
            Files = new List<DataFileViewModel>();
        }

        public EntityViewModel(CFEntity entity, string[] languageCodes, IDictionary<string, EntityViewModel> previousEntities = null) : this()
        {
            this.Id = entity.Id;
            this.Guid = entity.Guid;
            this.LanguageCodes = languageCodes;

            // Added to prevent circular child members.
            if(previousEntities == null)
            {
                previousEntities = new Dictionary<string, EntityViewModel>();
            }
            previousEntities.Add(this.Guid, this);

            foreach(CFMetadataSet metadataset in entity.MetadataSets)
            {
                MetadataSets.Add(new MetadataSetViewModel(metadataset, languageCodes));
            }

            if (typeof(CFItem).IsAssignableFrom(entity.GetType()))
            {
                foreach(CFDataObject dataObject in ((CFItem)entity).DataObjects)
                {
                    if (typeof(CFDataFile).IsAssignableFrom(dataObject.GetType()))
                    {
                        Files.Add(new DataFileViewModel((CFDataFile)dataObject, entity.Id));
                    }
                }
            }
            
            if (typeof(CFAggregation).IsAssignableFrom(entity.GetType()))
            {
                
                foreach (CFEntity member in ((CFAggregation)entity).ChildMembers)
                {
                    if (previousEntities.ContainsKey(member.Guid))
                    {
                        Children.Add(previousEntities[member.Guid]);
                    }
                    else
                    {
                        EntityViewModel child = new EntityViewModel(member, languageCodes, previousEntities);
                        Children.Add(child);
                    }
                }
            }
        }

        public IEnumerable<EntityViewModel> GetParents()
        {
            CatfishDbContext context = new CatfishDbContext();
            EntityService es = new EntityService(context);

            CFEntity entity = es.GetAnEntity(Id);

            if(entity != null && typeof(CFAggregation).IsAssignableFrom(entity.GetType())) {
                return ((CFAggregation)entity).ParentMembers.Select(p => new EntityViewModel(p, LanguageCodes));
            }

            return new List<EntityViewModel>();
        }

        public IEnumerable<FormFieldViewModel> GetAllFormFields()
        {
            return MetadataSets.SelectMany(m => m.Fields);
        }

        public IEnumerable<string> GetFieldValuesByName(string name, string languageCode = null)
        {
            IEnumerable<string> results = GetAllFormFields().SelectMany((f) => {
                if(languageCode == null)
                {
                    var keys = f.Names.Where(n => n.Value == name).Select(n => n.Key);
                    return f.Values.Where(v => keys.Contains(v.Key)).Select(v => v.Value);
                }
                else
                {
                    var result = new List<string>();
                    if(f.Names[languageCode] == name)
                    {
                        result.Add(f.Values[languageCode]);
                    }

                    return result;
                }
            });

            return results;
        }

        public DataFileViewModel GetFirstImage()
        {
            foreach(DataFileViewModel file in Files)
            {
                if(file.MimeType == MimeType.Image)
                {
                    return file;
                }
            }

            foreach(EntityViewModel child in Children)
            {
                DataFileViewModel result = child.GetFirstImage();
                if(result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }

    public class MetadataSetViewModel
    {
        public ICollection<FormFieldViewModel> Fields { get; set; }

        public MetadataSetViewModel()
        {
            Fields = new List<FormFieldViewModel>();
        }

        public MetadataSetViewModel(CFMetadataSet metadataset, string[] languageCodes) : this()
        {
            foreach(FormField field in metadataset.Fields)
            {
                Fields.Add(new FormFieldViewModel(field, languageCodes));
            }
        }
    }

    public class FormFieldViewModel
    {
        public IDictionary<string, string> Names { get; set; }
        public IDictionary<string, string> Values { get; set; }
        public string ModelType { get; set; }

        public FormFieldViewModel()
        {
            Names = new Dictionary<string, string>();
            Values = new Dictionary<string, string>();
        }

        public FormFieldViewModel(FormField field, string[] languageCodes) : this()
        {
            var names = field.GetNames(true);
            var values = field.GetValues(false);

            foreach(var name in names)
            {
                if (languageCodes.Contains(name.LanguageCode))
                {
                    Names.Add(name.LanguageCode, name.Value);
                }
            }

            foreach(var value in values)
            {
                if (languageCodes.Contains(value.LanguageCode))
                {
                    Values.Add(value.LanguageCode, value.Value);
                }
            }
        }
    }

    public class DataFileViewModel
    {
        public int ParentId { get; set; }
        public MimeType MimeType { get; set; }

        public string Guid { get; set; }

        public DataFileViewModel()
        {

        }

        public DataFileViewModel(CFDataFile dataFile, int parentId) : this()
        {
            ParentId = parentId;
            Guid = dataFile.Guid;
            MimeType = dataFile.TopMimeType;
        }
    }
}