﻿using Catfish.Core.Models.Data;
using Catfish.Core.Models.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Linq;

namespace Catfish.Core.Models
{
    public class Item : Aggregation
    {
        public virtual ICollection<Aggregation> ParentRelations { get; set; }

        public Item()
            : base()
        {
            ParentRelations = new List<Aggregation>();
            Data.Add(new XElement("data"));
        }

        public override string GetTagName() { return "item"; }

        [NotMapped]
        public virtual IEnumerable<DataObject> DataObjects
        {
            get
            {
                return GetChildModels("data/*", Data).Select(c => c as DataObject);
            }
        }

        /// <summary>
        /// This field is only used for passing file attachments between controller action and Items/Edit view through model binding
        /// </summary>
        [NotMapped]
        public Attachment AttachmentField
        {
            get
            {
                if (mAttachmentField == null)
                {
                    mAttachmentField = new Attachment();
                    mAttachmentField.FileGuids = string.Join(Attachment.FileGuidSeparator.ToString(), Files.Select(f => f.Guid));
                }
                return mAttachmentField;
            }
            set
            {
                mAttachmentField = value;
            }
        }
        private Attachment mAttachmentField;

        [NotMapped]
        public virtual IEnumerable<DataFile> Files
        {
            get
            {
                return GetChildModels("data/" + DataFile.TagName, Data).Select(c => c as DataFile);
            }
        }

        [NotMapped]
        public virtual IEnumerable<FormSubmission> FormSubmissions
        {
            get
            {
                return GetChildModels("data/" + FormSubmission.TagName, Data).Select(c => c as FormSubmission);
            }
        }


        protected XElement GetDataObjectRoot(bool createIfNotExist = true)
        {
            return GetImmediateChild("data");
        }

        public void AddData(DataObject obj)
        {
            GetDataObjectRoot().Add(obj.Data);

            if (this.Id > 0)//Onlye adding logs for new data objects when updating existing items
                LogChange(obj.Guid, "Added data object");
        }

        public FormSubmission GetFormSubmission(string formSubmissionRef)
        {
            var xpath = "./data/" + FormSubmission.TagName + "[@ref='" + formSubmissionRef + "']";
            return GetChildModels(xpath, Data).FirstOrDefault() as FormSubmission;
        }

        public void RemoveFile(DataFile file)
        {
            var xpath = "./data/" + DataFile.TagName + "[@guid='" + file.Guid + "']";
            XElement fileElement = GetChildElements(xpath, Data).FirstOrDefault();
            if (fileElement == null)
                throw new Exception("File does not exist.");
            fileElement.Remove();

            file.DeleteFilesFromFileSystem();

            LogChange(file.Guid, "Deleted " + file.FileName);
        }

    }
}