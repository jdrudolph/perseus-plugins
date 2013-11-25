namespace PerseusApi.Generic{
	public interface IFromMatrix : IActivity{
		string HelpOutput { get; }
		string[] HelpSupplTables { get; }
		int NumSupplTables { get; }
		string[] HelpDocuments { get; }
		int NumDocuments { get; }
	}
}