using BaseLib.ParamWf;
using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	public interface IMatrixExport : IMatrixActivity, IExport{
		void Export(ParametersWf parameters, IMatrixData data, ProcessInfo processInfo);

		/// <summary>
		/// Define here the parameters that determine the specifics of the export.
		/// </summary>
		/// <param name="mdata">The parameters might depend on the data matrix.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		ParametersWf GetParameters(IMatrixData mdata, ref string errString);
	}
}