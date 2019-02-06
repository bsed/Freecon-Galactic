using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Models;
using System.Data.Entity.Core.Objects;
using Server.EFDB.Models;
using Server.Models.Database;
using System.Data.Entity.Infrastructure;
using Server.Interfaces;
using Core.Logging;
using Server.Models.Structures;
using System.Data.SqlClient;

namespace Server.Database
{
    public class GalacticObjectContext:DbContext
    {
        public DbSet<DBShip> Ships { get; set; }
        public DbSet<DBPlayer> Players { get; set; }

        public DbSet<DBPlanet> Planets { get; set; }
        public DbSet<DBPSystem> Systems { get; set; }
        public DbSet<DBPort> Ports { get; set; }
        public DbSet<DBMoon> Moons { get; set; }
        public DbSet<DBColony> Colonies { get; set; }
        

        public DbSet<ShipStats> ShipStats { get; set; }
        public DbSet<DBAccount> Accounts { get; set; }
        public DbSet<PlanetLayout> Layouts { get; set; }
        public DbSet<DBTeam> Teams { get; set; }
        public DbSet<StructureModel> Structures { get; set; }
        public DbSet<Warphole> Warpholes { get; set; }
        public DbSet<CommandCenterModel> CommandCenters { get; set; }
        public DbSet<SiloModel> Silos { get; set; }

        static string _connectionString;

        public GalacticObjectContext(bool DropExistingDB = false):base(getConStrSQL())
        {
           
           //Console.WriteLine(Database.Connection.ConnectionString);
            
            System.Data.Entity.Database.SetInitializer<GalacticObjectContext>(new CreateDatabaseIfNotExists<GalacticObjectContext>());

            if (DropExistingDB)
                Database.Delete();
           //System.Data.Entity.Database.SetInitializer<GalacticObjectContext>(new DropCreateDatabaseAlways<GalacticObjectContext>());
           //Database.Log = Console.WriteLine;     

           SetInitializers();
        }

        public void SetLogger(SimpleLogger l)
        {
            if(l != null)
                Database.Log = l.Log;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Override EF's default ID generator
            modelBuilder.Entity<DBShip>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<DBPlayerShip>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<DBPlayer>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<DBArea>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<DBAccount>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<ShipStats>().Property(e => e.Name).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<PSystem>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<DBTeam>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<DBColony>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<StructureModel>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<CommandCenterModel>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            
            //modelBuilder.Entity<DBColony>().HasRequired<CommandCenter>(ee => ee.CommandCenter);

            
        }

  


        /// <summary>
        /// Registers delegates to initialize certain classes automatically on db load
        /// Also explicitly loads certain related entities
        /// </summary>
        void SetInitializers()
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;

            objectContext.ObjectMaterialized += (sender, e) =>
            {
                var obj = e.Entity;
                Type t =ObjectContext.GetObjectType(e.Entity.GetType());
                //Console.WriteLine(t);

                if (t == typeof(PlanetLayout))
                {
                    PlanetLayout p = e.Entity as PlanetLayout;

                    ((PlanetLayout)obj).InitLayout();
                }
                else if (t == typeof(DBPlanet))
                {
                    //this.Entry(e.Entity).Reference(ee => ee.Colony).Load();

                }
                else if(t == typeof(StructureModel))
                {
                    this.Structures.Include(e0 => ((StructureModel)e0).Weapon).First(e1=>e1.Id == ((StructureModel)e1).Id);
                    
                }
                else if(t == typeof(DBColony))
                {
                    CommandCenterModel cm = this.CommandCenters.First(ff=>ff.Id == ((DBColony)e.Entity).CommandCenterID);


                    ((DBColony)e.Entity).CommandCenter = new CommandCenterModel(cm);

                }
                else if (t==typeof(CommandCenterModel))
                {
                    
                }
                

                

            };

        }

        public static string getConStrSQL()
        {

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = "(localdb)\\v11.0";
            ////builder["Data Source"] = "(LocalDB)\\MSSQLLocalDB";
            builder["integrated Security"] = true;
            builder["Initial Catalog"] = "Server.Database.Context";
            builder["MultipleActiveResultSets"] = "True";
            builder["Database"] = "freecon-dev-db";
            builder["User ID"] = "freecon-dev";
            builder["Password"] = "asdfasdfasdf";
            //return builder.ConnectionString;
            _connectionString = builder.ConnectionString;


            //_connectionString = "Server=.\\SQLEXPRESS;Database=freecon-dev-db;User ID=freecon-dev;Password=asdfasdfasdf;Integrated Security=True;MultipleActiveResultSets=True;";
            return _connectionString;
        }
    }
}
