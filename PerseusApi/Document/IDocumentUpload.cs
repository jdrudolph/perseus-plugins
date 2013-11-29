using BaseLib.Param;
using PerseusApi.Generic;

namespace PerseusApi.Document {
	public interface IDocumentUpload : IDocumentActivity, IUpload {
		void LoadData(IDocumentData matrixData, Parameters parameters, ProcessInfo processInfo);
	}
}
