﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Catfish.Core.Models.Attributes;
using System.Web.Script.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Linq;
using Catfish.Core.Models.Forms;

namespace Catfish.Core.Models
{
    [CFTypeLabel("Metadata Set")]
    public class CFMetadataSet : AbstractForm
    {
        public static string TagName
        {
            get
            {
                return "metadata-set";

            }
        }
        public override string GetTagName() { return TagName; }

        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual ICollection<CFEntityType> EntityTypes { get; set; }

    }
}
