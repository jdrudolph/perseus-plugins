using BasicLib.ParamWf;
using PerseusApi.Generic;

namespace PerseusApi.Document {
	public interface IDocumentUpload : IDocumentActivity, IUpload {
		void LoadData(IDocumentData matrixData, ParametersWf parameters, ProcessInfo processInfo);
	}
}
