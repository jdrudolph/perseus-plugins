using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PluginMzTab.mztab{
    public class Export : MzTabProcessing{
        public override string Name { get { return "Extract Mz Tab"; } }
        public override float DisplayRank { get { return 6; } }
        public override bool IsActive { get { return true; } }
        public override bool HasButton { get { return false; } }
        public override Bitmap DisplayImage { get { return null; } }
        public override string Description { get { return "Click here to extract the final MzTab file."; } }
        public override string HelpOutput { get { return null; } }
        public override string[] HelpSupplTables { get { return new string[0]; } }
        public override int NumSupplTables { get { return 0; } }
        public override string[] HelpDocuments { get { return new string[0]; } }
        public override int NumDocuments { get { return 0; } }
        public override int MinNumInput { get { return 4; } }
        public override int MaxNumInput { get { return 4; } }
        public override string[] Tables { get { return new[]{Matrix.MetadataSection, Matrix.ProteinSection, Matrix.PeptideSection, Matrix.PSMSection}; } }

        public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables,
                                                ref IDocumentData[] documents, ProcessInfo processInfo){
            IMatrixData result = (IMatrixData) inputData[0].CreateNewInstance();
            List<string> stringColumnNames = new List<string>{"Section"};
            List<string[]> stringColumns = new List<string[]>{new string[MaxNumInput]};
            for (int i = 0; i < MaxNumInput; i++){
                stringColumns[0][i] = GetInputName(i);
            }
            result.SetData("Summary", new List<string>(), new float[MaxNumInput,1], stringColumnNames, stringColumns,
                           new List<string>(), new List<string[][]>(), new List<string>(), new List<double[]>(),
                           new List<string>(), new List<double[][]>());
            string filename = param.GetFileParam("File name").Value;
            StreamWriter writer;
            try{
                writer = new StreamWriter(filename);
            } catch (Exception e){
                processInfo.ErrString = e.Message;
                return null;
            }
            double[] lines = new double[MaxNumInput];
            for (int i = 0; i < MaxNumInput; i++){
                if (inputData.Length < i && inputData[i] == null){
                    continue;
                }
                UpdateStatus(processInfo.Progress, processInfo.Status, i);
                lines[i] = inputData[i].RowCount;
                Write(writer, inputData[i], i != 0);
                writer.WriteLine();
            }
            writer.Close();
            UpdateStatus(processInfo.Progress, processInfo.Status, MaxNumInput);
            result.AddNumericColumn("Rows", "", lines);
            return result;
        }

        private void UpdateStatus(Action<int> progress, Action<string> status, int i){
            if (i < MaxNumInput){
                status("Write " + GetInputName(i));
            } else{
                status("");
            }
            progress((100*i)/MaxNumInput);
        }

        private void Write(StreamWriter writer, IMatrixData mdata, bool hasHeader){
            if (hasHeader){
                List<string> header = new List<string>();
                for (int i = 0; i < mdata.StringColumnCount; i++){
                    header.Add(Trunc(mdata.StringColumnNames[i]));
                }
                writer.WriteLine(StringUtils.Concat("\t", header));
            }
            for (int row = 0; row < mdata.RowCount; row++){
                List<string> line = new List<string>();
                for (int column = 0; column < mdata.StringColumnCount; column++){
                    line.Add(Trunc(mdata.StringColumns[column][row]));
                }
                writer.WriteLine(StringUtils.Concat("\t", line));
            }
        }

        private const int maxlen = 30000;

        private static string Trunc(string s){
            return s.Length <= maxlen ? s : s.Substring(0, maxlen);
        }

        public override Parameters GetParameters(IMatrixData[] inputData, ref string errString){
            return
                new Parameters(new Parameter[]
                {new FileParam("File name"){Filter = "Tab separated file (*.txt)|*.txt", Save = true}});
        }
    }
}