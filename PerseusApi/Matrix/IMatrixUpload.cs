using BaseLib.Param;
using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	public interface IMatrixUpload : IMatrixActivity, IUpload{
		void LoadData(IMatrixData mdata, Parameters parameters, ProcessInfo processInfo);
	}
}