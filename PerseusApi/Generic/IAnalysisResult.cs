using System;

namespace PerseusApi.Generic{
	/// <summary>
	/// The output of a generic <code>IAnalysis</code>. It contains the data for this IAnalysis which is serialized with the session. 
	/// </summary>
	public interface IAnalysisResult{
		/// <summary>
		/// Heading to be displayed on the tab page created for the visual component of this <code>IAnalysisResult</code>.
		/// </summary>
		/// <returns></returns>
		string Heading { get; }
		object CreateUiElement(Action<string> updateStatus, Action<IData> newData);
	}
}