﻿using Catfish.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Catfish.Core.Models
{
    public class CatfishDbContext : DbContext
    {
        public CatfishDbContext()
            : base("piranha")
        {

        }

        ////private static CatfishDbContext mDb;
        ////public static CatfishDbContext Instance
        ////{
        ////    get
        ////    {
        ////        if (mDb == null)
        ////            mDb = new CatfishDbContext();
        ////        return mDb;
        ////    }
        ////}

        ////private static Piranha.DataContext mPiranhaDb;
        ////public static Piranha.DataContext PiranhaInstance
        ////{
        ////    get
        ////    {
        ////        if (mPiranhaDb == null)
        ////            mPiranhaDb = new Piranha.DataContext();
        ////        return mPiranhaDb;
        ////    }
        ////}


        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Entity<Aggregation>()
                .HasMany<Aggregation>(p => p.ChildMembers)
                .WithMany(c => c.ParentMembers)
                .Map(t =>
                {
                    t.MapLeftKey("ParentId");
                    t.MapRightKey("ChildId");
                    t.ToTable("AggregationHasMembers");
                });

            builder.Entity<Aggregation>()
                .HasMany<Item>(p => p.ChildRelations)
                .WithMany(c => c.ParentRelations)
                .Map(t =>
                {
                    t.MapLeftKey("ParentId");
                    t.MapRightKey("ChildId");
                    t.ToTable("AggregationHasRelatedObjects");
                });

        }

        public DbSet<Entity> Entities { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Item> Items { get; set; }

    }
}