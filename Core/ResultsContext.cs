using System.Data.Entity;

namespace PhotonKinetics.ResultDatabase
{

	/// <summary>
	/// This class inherits from the Entity Framework DbContext class.  The public properties
	/// that are declared "As DBSet(Of (Type Name))" provide the instructions for EF to construct
	/// the database tables based upon the POCO classes defined in the model. The base class also
	/// handles the CRUD (Create, Read, Update, Delete) methods needed to interact with the database.
	/// </summary>
	public class ResultsContext : DbContext
	{
		public const string PKDB_SETTINGS_NAMESPACE = "PK Measurement Results Database";
		public const string PKDB_DEFAULT_NAME = "PKOTDRResultsDB";

		public DbSet<ResultSetHeader> ResultSetHeaders { get; set; }
		public DbSet<Result> Results { get; set; }
		public DbSet<Instrument> Instruments { get; set; }
		public DbSet<ResultSetLabel> HeaderLabels { get; set; }
		public DbSet<ModeFieldWaveResult> ModeFieldWaveResults { get; set; }
		public DbSet<AttenuationWaveResult> AttenWaveResults { get; set; }
		public DbSet<LengthResult> LengthResults { get; set; }

		/// <summary>
		/// This constructor is used by non-PKSL clients.  To change the database connection string, add a Connection String section to the
		/// app.config file with the name "PKResultsDB"
		/// </summary>
		public ResultsContext() : base(PKDB_DEFAULT_NAME)
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<ResultsContext, Migrations.Configuration>());
		}

		/// <summary>
		/// This constructor is to be used with the PKSL database persister.
		/// </summary>
		/// <param name="nameOrConnectionString">Name of the database or a full connection string</param>
		public ResultsContext(string nameOrConnectionString) : base(nameOrConnectionString)
		{
			// use code-based configuration
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<ResultsContext, Migrations.Configuration>());
		}

		/// <summary>
		/// This method is used by any clients that create instances of this class and provides
		/// ways to override the default behaviors of Entity Framework Code First.
		/// </summary>
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// EF will automatically pluralize the names of the tables for model classes
			// This will turn that default setting off (this programmer prefers singular table names)
			modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();

			// EF Code first uses Table-per-Hierarchy (TPH) inheritance mapping by default
			// Calls to .ToTable override this behavior and implements Table-per-Type (TPT)
			// The choice to use TPT vs. TPH depends on various factors, preference of
			// a well normalized database vs. speed of polymorphic queries
			// TPH can be implemented by commenting out or removing all .ToTable calls below
			modelBuilder.Entity<AttenuationResult>().ToTable("AttenuationResult");
			modelBuilder.Entity<LengthResult>().ToTable("LengthResult");
			modelBuilder.Entity<ModeFieldResult>().ToTable("ModeFieldResult");
			modelBuilder.Entity<SignatureResult>().ToTable("SignatureResult");
		}

	}
}