using BaseLibS.Param;

namespace PerseusApi.Generic{
	public interface IVisualization : IActivity{
		int MinNumInput { get; }
		int MaxNumInput { get; }
		string GetInputName(int index);
		DataType GetInputDataType(int index);
		IAnalysisResult VisualizeData(IData[] data, Parameters param, ProcessInfo processInfo);

		/// <summary>
		/// Define here the parameters that determine the specifics of the visualization.
		/// </summary>
		/// <param name="data">The parameters might depend on the input data.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IData[] data, ref string errString);
	}
}