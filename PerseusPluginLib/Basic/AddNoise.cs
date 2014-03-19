using System.Drawing;
using BaseLib.Num;
using BaseLib.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Basic{
    public class AddNoise : IMatrixProcessing{
        public bool HasButton{
            get { return false; }
        }

        public Bitmap DisplayImage{
            get { return null; }
        }

        public string Description{
            get { return "Modulate the data with gaussian noise."; }
        }

        public string HelpOutput{
            get { return "Same as input matrix with random noise added to the expression columns."; }
        }

        public string[] HelpSupplTables{
            get { return new string[0]; }
        }

        public int NumSupplTables{
            get { return 0; }
        }

        public string Name{
            get { return "Add noise"; }
        }

        public string Heading{
            get { return "Basic"; }
        }

        public bool IsActive{
            get { return true; }
        }

        public float DisplayOrder{
            get { return 200; }
        }

        public string[] HelpDocuments{
            get { return new string[0]; }
        }

        public int NumDocuments{
            get { return 0; }
        }

        public int GetMaxThreads(Parameters parameters){
            return 1;
        }

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
                                ref IDocumentData[] documents, ProcessInfo processInfo){
            Random2 rand = new Random2();
            double std = param.GetDoubleParam("Standard deviation").Value;
            for (int i = 0; i < mdata.RowCount; i++){
                for (int j = 0; j < mdata.ExpressionColumnCount; j++){
                    mdata[i, j] += (float) rand.NextGaussian(0, std);
                }
            }
        }

        public Parameters GetParameters(IMatrixData mdata, ref string errorString){
            return
                new Parameters(new Parameter[]
                {new DoubleParam("Standard deviation", 0.1){Help = "Standard deviation of the noise distribution."}});
        }
    }
}