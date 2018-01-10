using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Piranha.Extend;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;

namespace Catfish.Models.Regions
{
    [Export(typeof(IExtension))]
    [ExportMetadata("InternalId", "ZoomImage")]
    [ExportMetadata("Name", "Zoom Image")]
    [ExportMetadata("Type", ExtensionType.Region)]
    [Serializable]
    public class ZoomImage : CatfishRegion
    {
        public string ImageUrl { get; set; }
    }
}