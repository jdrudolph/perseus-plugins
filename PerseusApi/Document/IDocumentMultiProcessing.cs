using BasicLib.ParamWf;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusApi.Document{
	public interface IDocumentMultiProcessing : IDocumentActivity, IMultiProcessing{
		IDocumentData ProcessData(IDocumentData[] inputData, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo);

		/// <summary>
		/// Define here the parameters that determine the specifics of the processing.
		/// </summary>
		/// <param name="inputData">The parameters might depend on the data matrices.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		ParametersWf GetParameters(IDocumentData[] inputData, ref string errString);
	}
}