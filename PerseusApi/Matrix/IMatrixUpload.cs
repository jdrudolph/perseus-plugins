using BaseLib.ParamWf;
using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	public interface IMatrixUpload : IMatrixActivity, IUpload{
		void LoadData(IMatrixData matrixData, ParametersWf parameters, ProcessInfo processInfo);
	}
}