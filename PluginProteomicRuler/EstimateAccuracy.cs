using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using BaseLib.Param;
using BaseLibS.Param;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PluginProteomicRuler
{
    public class EstimateAccuracy : IMatrixProcessing
    {
        public bool HasButton { get { return false; } }
        public Bitmap DisplayImage { get { return null; } }
        public string Description { get { return "Estimate the accuracy of copy number or concentration values.\n"+
            "Depending on the user-specified minimum number of total identified peptides, the fraction of razor+unique to total peptides and the number of theoretical peptides per sequence length, all proteins will be categorized as high, medium or low accuracy."; } }
        public string HelpOutput { get { return "A categorical annotation column is added indicating the estimated accuracy."; } }
        public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
        public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
        public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
        public string[] HelpSupplTables { get { return new string[0]; } }
        public int NumSupplTables { get { return 0; } }
        public string Name { get { return "Estimate absolute protein quantification accuracy"; } }
        public string Heading { get { return "Proteomic ruler"; } }
        public bool IsActive { get { return true; } }
        public float DisplayRank { get { return 2; } }
        public string[] HelpDocuments { get { return new string[0]; } }
        public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
        public int NumDocuments { get { return 0; } }
        public int GetMaxThreads(Parameters parameters) { return 1; }
        public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:plugins:proteomicruler:estimateaccuracy"; } }

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
                                ref IDocumentData[] documents, ProcessInfo processInfo)
        {
			double[] totalPeptides = mdata.NumericColumns[param.GetParam<int>("Total number of peptides").Value];
			double[] uniqueRazorPeptides = mdata.NumericColumns[param.GetParam<int>("Unique + razor peptides").Value];
			double[] sequenceLength = mdata.NumericColumns[param.GetParam<int>("Sequence length").Value];
			double[] theoreticalPeptides = mdata.NumericColumns[param.GetParam<int>("Number of theoretical peptides").Value];

			double highMinPep = param.GetParam<double>("High: min. peptides").Value;
			double highMinRazorFraction = param.GetParam<double>("High: min. razor fraction").Value;
			double highMinTheorPep = param.GetParam<double>("High: min. theor.pep./100AA").Value;
			double mediumMinPep = param.GetParam<double>("Medium: min. peptides").Value;
			double mediumMinRazorFraction = param.GetParam<double>("Medium: min. razor fraction").Value;
			double mediumMinTheorPep = param.GetParam<double>("Medium: min. theor.pep./100AA").Value;

            double[] razorFraction = new double[mdata.RowCount];
            double[] theoreticalPepsPer100Aa = new double[mdata.RowCount];

            string[][] score = new string[mdata.RowCount][];

            for (int row = 0; row < mdata.RowCount; row++)
            {
                razorFraction[row] = uniqueRazorPeptides[row]/totalPeptides[row];
                theoreticalPepsPer100Aa[row] = theoreticalPeptides[row]/(sequenceLength[row]/100);

                if (totalPeptides[row] >= highMinPep && razorFraction[row] >= highMinRazorFraction && theoreticalPepsPer100Aa[row] >= highMinTheorPep)
                {
                    score[row] = new []{ "high" };
                    continue;
                }
                if (totalPeptides[row] >= mediumMinPep && razorFraction[row] >= mediumMinRazorFraction && theoreticalPepsPer100Aa[row] >= mediumMinTheorPep)
                {
                    score[row] = new []{ "medium" };
                    continue;
                }
                score[row] = new []{ "low" };
            }

            mdata.AddCategoryColumn("Absolute quantification accuracy","",score);



        }




        public Parameters GetParameters(IMatrixData mdata, ref string errorString)
        {
            return
                new Parameters(new Parameter[]
                    {
                        new SingleChoiceParam("Total number of peptides")
                            {
                                Values = mdata.NumericColumnNames,
                                Value = ProteomicRulerUtils.Match(mdata.NumericColumnNames.ToArray(), new string[] {"peptides"}, false, true, true )[0],
                            },
                        new SingleChoiceParam("Unique + razor peptides")
                            {
                                Values = mdata.NumericColumnNames,
                                Value = ProteomicRulerUtils.Match(mdata.NumericColumnNames.ToArray(), new string[] {"razor"}, false, true, true )[0],
                            },
                        new SingleChoiceParam("Sequence length")
                            {
                                Values = mdata.NumericColumnNames,
                                Value = ProteomicRulerUtils.Match(mdata.NumericColumnNames.ToArray(), new string[] {"length"}, false, true, true )[0],
                            },
                        new SingleChoiceParam("Number of theoretical peptides")
                            {
                                Values = mdata.NumericColumnNames,
                                Value = ProteomicRulerUtils.Match(mdata.NumericColumnNames.ToArray(), new string[] {"theoretical"}, false, true, true )[0],
                            },
                        new DoubleParam("High: min. peptides",8)
                            {
                                Help="High confidence class: Specify the minimum number of total peptides."
                            }, 
                        new DoubleParam("High: min. razor fraction",0.75)
                            {
                                Help="High confidence class: Specify the minimum ratio of unique+razor to total peptides."
                            }, 
                        new DoubleParam("High: min. theor.pep./100AA",3)
                            {
                                Help="High confidence class: Specify the minimum number of theoretical peptides per 100 amino acids."
                            }, 
                        new DoubleParam("Medium: min. peptides",3)
                            {
                                Help="Medium confidence class: Specify the minimum number of total peptides."
                            }, 
                        new DoubleParam("Medium: min. razor fraction",0.5)
                            {
                                Help="Medium confidence class: Specify the minimum ratio of unique+razor to total peptides."
                            }, 
                        new DoubleParam("Medium: min. theor.pep./100AA",2)
                            {
                                Help="Medium confidence class: Specify the minimum number of theoretical peptides per 100 amino acids."
                            }, 
                    });
        }
    }

}
