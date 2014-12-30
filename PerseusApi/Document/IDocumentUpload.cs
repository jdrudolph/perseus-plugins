using BaseLibS.Param;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusApi.Document{
    public interface IDocumentUpload : IDocumentActivity, IUpload{
        void LoadData(IDocumentData matrixData, Parameters parameters, ref IMatrixData[] supplTables,
                      ref IDocumentData[] documents, ProcessInfo processInfo);
    }
}