﻿using Catfish.Core.Models;
using Catfish.Core.Models.Forms;
using Catfish.Core.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using static Catfish.Core.Models.CFEntityType;
using System.Data.Entity.Core.Common;
using System.Data.SQLite.EF6;
using System.Data.SQLite;
using System.ComponentModel.DataAnnotations.Schema;
using Catfish.Tests.Migrations;
using System.Data.Entity.Migrations;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.IO;
using System.Reflection;
using System.Data;
using Piranha.IO;
using System.Web;
using System.Web.Hosting;

namespace Catfish.Tests.Helpers
{
    class DatabaseHelper
    {
        public const int TOTAL_ENTITYTYPES = 5;
        public const int TOTAL_COLLECTIONS = 7;
        public const int TOTAL_ITEMS = 11;

        private CatfishTestDbContext mDb { get; set; }
        public CatfishTestDbContext Db
        {
            get
            {
                if (mDb == null)
                {
                    DbConnection connection = GetConnection(GetConnectionStringByName("catfish"));
                    connection.Open();
                    mDb = new CatfishTestDbContext(connection);
                }

                return mDb;
            }
        }

        private static Piranha.DataContext mPDb { get; set; }
        public Piranha.DataContext PDb
        {
            get
            {
                if(mPDb == null)
                {
                    mPDb = new Piranha.DataContext();
                    SetupPiranha(mPDb);
                }

                return mPDb;
            }
        }

        private MetadataService mMs { get; set; }
        public MetadataService Ms
        {
            get
            {
                if (mMs == null)
                {
                    mMs = new MetadataService(Db);
                }

                return mMs;
            }
        }

        private EntityService mEs { get; set; }
        public EntityService Es
        {
            get
            {
                if (mEs == null)
                {
                    mEs = new EntityService(Db);
                }

                return mEs;
            }
        }

        private EntityTypeService mEts { get; set; }
        public EntityTypeService Ets
        {
            get
            {
                if (mEts == null)
                {
                    mEts = new EntityTypeService(Db);
                }

                return mEts;
            }
        }

        private CollectionService mCs { get; set; }
        public CollectionService Cs
        {
            get
            {
                if (mCs == null)
                {
                    mCs = new CollectionService(Db);
                }

                return mCs;
            }
        }

        private ItemService mIs { get; set; }
        public ItemService Is
        {
            get
            {
                if (mIs == null)
                {
                    mIs = new ItemService(Db);
                }

                return mIs;
            }
        }

        private IngestionService mIgs { get; set; }
        public IngestionService Igs
        {
            get
            {
                if(mIgs == null){
                    mIgs = new IngestionService(Db);
                }

                return mIgs;
            }
        }

        public DatabaseHelper(bool setupData = false)
        {
            Initialize();

            if (setupData)
            {
                SetupData();
            }
        }

        private ConnectionStringSettings GetConnectionStringByName(string name)
        {
            ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;

            if(settings != null)
            {
                foreach(ConnectionStringSettings s in settings)
                {
                    if(s.Name == name)
                    {
                        return s;
                    }
                }
            }

            return null;
        }

        private DbConnection GetConnection(ConnectionStringSettings settings)
        {
            DbConnection connection = null;

            try
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(settings.ProviderName);
                connection = factory.CreateConnection();
                connection.ConnectionString = settings.ConnectionString;
            }catch(Exception ex)
            {
                throw ex;
            }

            return connection;
        }

        private void CreateMetadata()
        {
            CFMetadataSet metadata = new CFMetadataSet();
            metadata.SetName("Basic Metadata");
            metadata.SetDescription("Metadata Description");

            List<FormField> fields = new List<FormField>();

            FormField field = new TextField();
            field.Name = "Name";
            field.Description = "The Description";
            fields.Add(field);

            field = new TextField();
            field.Name = "Description";
            field.Description = "Description Description";
            fields.Add(field);

            metadata.Fields = fields;
            metadata.Serialize();

            Ms.UpdateMetadataSet(metadata);

            Db.SaveChanges();
        }

        private void CreateEntityTypes()
        {
            CFMetadataSet metadata = Ms.GetMetadataSets().FirstOrDefault();

            for (int i = 0; i < TOTAL_ENTITYTYPES; ++i)
            {
                CFEntityType et = new CFEntityType();
                et.Name = "Entity" + (i + 1);
                et.MetadataSets.Add(metadata);

                List<eTarget> targets = new List<eTarget>();

                if (i % 2 == 0)
                {
                    targets.Add(eTarget.Items);
                }

                if (i % 5 == 1)
                {
                    targets.Add(eTarget.Collections);
                }

                et.TargetTypesList = targets;

                et.AttributeMappings.Add(new CFEntityTypeAttributeMapping()
                {
                    Name = "Name Mapping",
                    MetadataSet = metadata,
                    FieldName = "Name"
                });

                et.AttributeMappings.Add(new CFEntityTypeAttributeMapping()
                {
                    Name = "Description Mapping",
                    MetadataSet = metadata,
                    FieldName = "Description"
                });

                Db.EntityTypes.Add(et);
            }

            Db.SaveChanges();
        }

        private void CreateCollections()
        {
            List<int> ets = Ets.GetEntityTypes().ToList().Where(et => et.TargetTypesList.Contains(eTarget.Collections)).Select(et => et.Id).ToList();

            for (int i = 0; i < TOTAL_COLLECTIONS; ++i)
            {
                int index = i % ets.Count;
                CFCollection c = Cs.CreateEntity<CFCollection>(ets[index]);
                c.SetName("Collection " + (i + 1));
                c.SetDescription("Description for Collection " + (i + 1));
                
                Cs.UpdateStoredCollection(c);
            }

            Db.SaveChanges();
        }

        private void CreateItems()
        {
            List<int> ets = Ets.GetEntityTypes().ToArray().Where(et => et.TargetTypesList.Contains(eTarget.Items)).Select(et => et.Id).ToList();

            for (int i = 0; i < TOTAL_ITEMS; ++i)
            {
                int index = i % ets.Count;
                CFItem e = Is.CreateEntity<CFItem>(ets[index]);
                e.SetName("Item " + (i + 1));
                e.SetDescription("Description for Item " + (i + 1));
                
                Is.UpdateStoredItem(e);
            }

            Db.SaveChanges();
        }

        private void SetupData()
        {
            CreateMetadata();
            CreateEntityTypes();
            CreateCollections();
            CreateItems();
        }

        public void Initialize()
        {
            try
            {

                Catfish.Tests.Migrations.Configuration config = new Catfish.Tests.Migrations.Configuration();
                var migrator = new DbMigrator(config);

                foreach (string migName in migrator.GetPendingMigrations())
                {
                    Type migration = config.MigrationsAssembly.GetType(string.Format("{0}.{1}", config.MigrationsNamespace, migName.Substring(16)));
                    DbMigration m = (DbMigration)Activator.CreateInstance(migration);
                    m.Up();

                    var prop = m.GetType().GetProperty("Operations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (prop != null)
                    {
                        IEnumerable<MigrationOperation> operations = prop.GetValue(m) as IEnumerable<MigrationOperation>;
                        var generator = config.GetSqlGenerator("System.Data.SQLite");
                        var statements = generator.Generate(operations, "2008");
                        foreach (MigrationStatement item in statements)
                            Db.Database.ExecuteSqlCommand(item.Sql);
                    }

                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void SetupPiranha(Piranha.DataContext PDb)
        {
            if (PDb.Database.Exists())
            {
                string db = PDb.Database.Connection.DataSource;
                File.Delete("./piranha.db");
            }

            if(HttpContext.Current == null)
            {
                SimpleWorkerRequest request = new SimpleWorkerRequest("", "", "", null, new StringWriter());
                HttpContext context = new HttpContext(request);
                HttpContext.Current = context;
            }

            // Copied and modified from Piranha.Areas.Manager.Controllers.InstallController
            // Read embedded create script
            Assembly piranhaAssembly = Assembly.GetAssembly(typeof(Piranha.Areas.Manager.Controllers.InstallController));

            Stream str = piranhaAssembly.GetManifestResourceStream(Piranha.Data.Database.ScriptRoot + ".Create.sql");
            String sql = new StreamReader(str).ReadToEnd();
            str.Close();

            // Read embedded data script
            str = piranhaAssembly.GetManifestResourceStream(Piranha.Data.Database.ScriptRoot + ".Data.sql");
            String data = new StreamReader(str).ReadToEnd();
            str.Close();

            // Split statements and execute
            string[] stmts = sql.Split(new char[] { ';' });
            using (var tx = PDb.Database.BeginTransaction())
            {
                // Create database from script
                foreach (string stmt in stmts)
                {
                    if (!String.IsNullOrEmpty(stmt.Trim()))
                        Piranha.Models.SysUser.Execute(stmt, tx.UnderlyingTransaction);
                }
                tx.Commit();
            }

            // Split statements and execute
            stmts = data.Split(new char[] { ';' });
            using (var tx = PDb.Database.BeginTransaction())
            {
                // Create user
                Piranha.Models.SysUser usr = new Piranha.Models.SysUser()
                {
                    Login = ConfigurationManager.AppSettings["AdminLogin"],
                    Email = ConfigurationManager.AppSettings["AdminEmail"],
                    GroupId = new Guid("7c536b66-d292-4369-8f37-948b32229b83"),
                    CreatedBy = new Guid("ca19d4e7-92f0-42f6-926a-68413bbdafbc"),
                    UpdatedBy = new Guid("ca19d4e7-92f0-42f6-926a-68413bbdafbc"),
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                };
                usr.Save(tx.UnderlyingTransaction);

                // Create user password
                Piranha.Models.SysUserPassword pwd = new Piranha.Models.SysUserPassword()
                {
                    Id = usr.Id,
                    Password = ConfigurationManager.AppSettings["AdminPassword"],
                    IsNew = false
                };
                pwd.Save(tx.UnderlyingTransaction);

                // Create default data
                foreach (string stmt in stmts)
                {
                    if (!String.IsNullOrEmpty(stmt.Trim()))
                    {
                        string statement = stmt.Replace("GETDATE()", "date('now')")
                            .Replace("SUBSTRING", "substr")
                            .Replace("NEWID()", string.Format("'{0}'", Guid.NewGuid().ToString()));

                        Piranha.Models.SysUser.Execute(statement, tx.UnderlyingTransaction);
                    }
                        
                }
                tx.Commit();
            }
            
        }

        public void SetupDbData()
        {
            //SetupPiranha();
            SetupData();
            var test = Db.MetadataSets.ToArray();
        }

        public CFCollection CreateCollection(CollectionService cs, int entityTypeId, string name, string description, bool store = false)
        {
            CFCollection c = cs.CreateCollection(entityTypeId);
            c.Name = name;
            c.Description = description;


            if (store)
            {
                c = cs.UpdateStoredCollection(c);
            }

            return c;
        }

        public CFItem CreateItem(ItemService itemSrv, int entityTypeId, string name, string description, bool store = false)
        {
            CFItem i = itemSrv.CreateItem(entityTypeId);
            i.Name = name;
            i.Description = description;


            if (store)
            {
                i = itemSrv.UpdateStoredItem(i);
            }

            return i;
        }
    }

    public class SqliteConfiguration : DbConfiguration
    {
        public SqliteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }
    
    public class CatfishTestDbContext : CatfishDbContext
    {
        public CatfishTestDbContext() : base(){

        }

        public CatfishTestDbContext(DbConnection connection) : base(connection, true)
        {
        }

        protected override void SetColumnTypes(DbModelBuilder builder)
        {
            builder.HasDefaultSchema("");

            builder.Entity<CFXmlModel>().Property(xm => xm.Content).HasColumnType("");
        }
    }
    
    public class CatfishMediaCacheProvider : Piranha.IO.IMediaCacheProvider
    {
        public void Delete(Guid id, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public byte[] Get(Guid id, int width, int? height, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public byte[] GetDraft(Guid id, int width, int? height, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public long GetTotalSize(Guid id, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public void Put(Guid id, byte[] data, int width, int? height, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public void PutDraft(Guid id, byte[] data, int width, int? height, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }
    }

    public class CatfishLogProvider : Piranha.Log.ILogProvider
    {
        public void Error(string origin, string message, Exception details = null)
        {
            Console.Error.WriteLine(string.Format("{0}: {1}", origin, message));
        }
    }

    public class CatfishMediaProvider : Piranha.IO.IMediaProvider
    {
        public void Delete(Guid id, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public void DeleteDraft(Guid id, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public byte[] Get(Guid id, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public byte[] GetDraft(Guid id, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public void Publish(Guid id, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public void Put(Guid id, byte[] data, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public void PutDraft(Guid id, byte[] data, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }

        public void Unpublish(Guid id, MediaType type = MediaType.Media)
        {
            throw new NotImplementedException();
        }
    }
}
