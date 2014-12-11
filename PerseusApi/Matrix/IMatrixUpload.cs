using BaseLib.Param;
using PerseusApi.Document;
using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	public interface IMatrixUpload : IMatrixActivity, IUpload{
		void LoadData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
			ProcessInfo processInfo);
	}
}