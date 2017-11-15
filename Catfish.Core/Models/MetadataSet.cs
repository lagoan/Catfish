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
    [TypeLabel("Metadata Set")]
    public class MetadataSet : Form
    {
        public override string GetTagName() { return "metadata-set"; }

        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual ICollection<EntityType> EntityTypes { get; set; }

    }
}
