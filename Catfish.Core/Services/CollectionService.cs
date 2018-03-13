﻿using Catfish.Core.Models;
using System.Data.Entity;

namespace Catfish.Core.Services
{
    public class CollectionService: EntityService
    {
        public CollectionService(CatfishDbContext db) : base(db) { }

        public Collection CreateCollection(int entityTypeId)
        {
            return CreateEntity<Collection>(entityTypeId);
        }

        public Collection UpdateStoredCollection(Collection changedCollection)
        {
            Collection dbModel = new Collection();

            if (changedCollection.Id > 0)
            {
                dbModel = Db.XmlModels.Find(changedCollection.Id) as Collection;
                //dbModel.Deserialize();

                //updating the "value" text elements
                dbModel.UpdateValues(changedCollection);
            }
            else
            {
                dbModel = CreateEntity<Collection>(changedCollection.EntityTypeId.Value);
                dbModel.UpdateValues(changedCollection);
            }

            if (changedCollection.Id > 0) //update Item
                Db.Entry(dbModel).State = EntityState.Modified;
            else
                dbModel.Serialize();
                Db.XmlModels.Add(dbModel);

            return dbModel;
        }

    }
}
