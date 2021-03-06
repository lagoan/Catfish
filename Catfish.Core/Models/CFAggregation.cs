﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Catfish.Core.Models
{
    public abstract class CFAggregation : CFEntity
    {
        public virtual ICollection<CFAggregation> ParentMembers { get; set; }
        public virtual ICollection<CFAggregation> ChildMembers { get; set; }

        public virtual ICollection<CFItem> ChildRelations { get; set; }

        [NotMapped]
        public bool HasAssociations { get { return ParentMembers.Count > 0 || ChildMembers.Count > 0 || ChildRelations.Count > 0; } }


        public CFAggregation()
        {
            ParentMembers = new List<CFAggregation>();
            ChildMembers = new List<CFAggregation>();
            ChildRelations = new List<CFItem>();
        }

        /// <summary>
        /// WARNING: Check for circular references first!
        /// </summary>
        /// <param name="child"></param>
        public void AppendChild(CFAggregation child)
        {
            this.ChildMembers.Add(child);
            child.ParentMembers.Add(this);
        }

        /// <summary>
        /// WARNING: Check for circular references first!
        /// </summary>
        /// <param name="child"></param>
        public void AppendParent(CFAggregation parent)
        {
            parent.ChildMembers.Add(this);
            this.ParentMembers.Add(parent);
        }

        public virtual IEnumerable<CFAggregation> ChildItems
        {
            get
            {
                return ChildMembers.Where(c => typeof(CFItem).IsAssignableFrom(c.GetType()));
            }
        }

    }
}