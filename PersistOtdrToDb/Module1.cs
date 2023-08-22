using System;

namespace PhotonKinetics.PersistOtdrToDb
{
	static class Module1
	{
		public static void Main()
		{
			var persister = new OtdrPKResultsDbPersister();
			try
			{
				Console.WriteLine("Saving OTDR results...");
				persister.PersistToDB();
				Console.WriteLine("Database results saved.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: " + ex.Message);
			}
		}
	}
}