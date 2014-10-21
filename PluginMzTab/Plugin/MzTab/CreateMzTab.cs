using System.Drawing;
using System.IO;
using BaseLib.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PluginMzTab.Plugin.Extended;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.MzTab{
    public class CreateMzTab : MzTabProcessing {
        public override string Name { get { return "Create MzTab"; } }
        public override string Description { get { return "Starts the MzTab Converter"; } }
        public override float DisplayRank { get { return 3; } }
        public override bool IsActive { get { return true; } }
        public override Bitmap DisplayImage { get { return null; } }
        public override bool HasButton { get { return false; } }
        //public override string Url { get { return null; } }
        public override string HelpOutput { get { return null; } }
        public override string[] HelpSupplTables { get { return null; } }
        public override int NumSupplTables { get { return 0; } }
        public override string[] HelpDocuments { get { return null; } }
        public override int NumDocuments { get { return 1; } }
        public override int MinNumInput { get { return Tables.Length; } }
        public override int MaxNumInput { get { return Tables.Length; } }

        public override string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:plugins:mztab:create_metadata_section"; } }

        private readonly string[] parameter = new[]{
            "ProteinGroups file", "Peptides file", "Msms file",
            "Output folder"
        };

        public override string[] Tables { get { return new[] { Matrix.DatabaseRef, Matrix.SpectraRef, Matrix.MetadataSection }; } }

        public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables,
                                                ref IDocumentData[] documents, ProcessInfo processInfo) {
            


            string proteinGroupsFile = param.GetFileParam(parameter[0]).Value;
            Constants.ValidateColumnNames(proteinGroupsFile, processInfo.Status, true);

            string peptidesFile = param.GetFileParam(parameter[1]).Value;
            Constants.ValidateColumnNames(proteinGroupsFile, processInfo.Status, true);

            string msmsFile = param.GetFileParam(parameter[2]).Value;
            Constants.ValidateColumnNames(proteinGroupsFile, processInfo.Status, true);

            string outputFolder = param.GetFolderParam(parameter[3]).Value;

            Stream databaseRefTable = new MatrixStream(GetMatrixData(Matrix.DatabaseRef, inputData), true);
            Stream spectraRefTable = new MatrixStream(GetMatrixData(Matrix.SpectraRef, inputData), true);
            Stream metadataFile = new MatrixStream(GetMatrixData(Matrix.MetadataSection, inputData), true);

            Converter convert = new Converter(databaseRefTable, spectraRefTable, metadataFile, proteinGroupsFile,
                                              peptidesFile, msmsFile, outputFolder){
                                                  Status = processInfo.Status,
                                                  Progress = processInfo.Progress
                                              };
            convert.Start();

            return (IMatrixData)inputData[0].CreateNewInstance(DataType.Matrix);
        }
        public override Parameters GetParameters(IMatrixData[] inputData, ref string errString){
            ValidateParameters(inputData, ref errString);
            
            Parameter[] param = new Parameter[parameter.Length];
            for (int i = 0; i < parameter.Length; i++) {
                if (i < parameter.Length - 1){
                    param[i] = new FileParam(parameter[i]);
                }
                else{
                    param[i] = new FolderParam(parameter[i]);
                }
            }
            return new Parameters(param);
        }
    }
}