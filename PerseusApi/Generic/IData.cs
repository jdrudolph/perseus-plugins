using System;

namespace PerseusApi.Generic{
	/// <summary>
	/// Generic data structure holding the data that flows through the network. Typically this is <code>IMatrixData</code>.
	/// </summary>
	public interface IData : IDisposable, ICloneable{
		string Name { get; set; }
		string AltName { get; set; }
		string Description { get; set; }
		/// <summary>
		/// For data that has been read from a file this string will contain the file name. If it was originally 
		/// created by an activity (e.g. 'Create random matrix') it will contain the name of the creating activity.
		/// </summary>
		string Origin { get; set; }
		DateTime CreationDate { get; set; }
		/// <summary>
		/// Name of the user who created this data item.
		/// </summary>
		string User { get; set; }
		IData CreateNewInstance();
		IData CreateNewInstance(DataType type);
		void Clear();
	}
}