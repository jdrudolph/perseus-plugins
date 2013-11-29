using BaseLib.Param;
using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	public interface IMatrixUpload : IMatrixActivity, IUpload{
		void LoadData(IMatrixData matrixData, Parameters parameters, ProcessInfo processInfo);
	}
}