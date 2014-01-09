using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Load{
	public class BinaryUpload : IMatrixUpload{
		private const string hexAlphabet = "0123456789ABCDEF";
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.binary; } }
		public string Name { get { return "Binary upload"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 12; } }
		public string HelpDescription { get { return "Load all bytes from a binary file and display them as hexadecimal numbers."; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(ref string errString){
			return
				new Parameters(new Parameter[]{
					new FileParam("File"){
						Filter = "All files (*.*)|*.*",
						Help = "Please specify here the name of the file to be uploaded including its full path."
					}
				});
		}

		public void LoadData(IMatrixData mdata, Parameters parameters, ProcessInfo processInfo){
			string filename = parameters.GetFileParam("File").Value;
			BinaryReader reader = FileUtils.GetBinaryReader(filename);
			byte[] x = reader.ReadBytes((int) reader.BaseStream.Length);
			reader.Close();
			const int nb = 16;
			List<string> hexLines = new List<string>();
			List<string> charLines = new List<string>();
			for (int i = 0; i < x.Length/nb; i++){
				byte[] y = ArrayUtils.SubArray(x, i*nb, (i + 1)*(nb));
				hexLines.Add(ToHex(y));
				charLines.Add(ToChar(y));
			}
			if (x.Length/nb > 0){
				byte[] y = ArrayUtils.SubArray(x, (x.Length/nb)*nb, x.Length);
				hexLines.Add(ToHex(y));
				charLines.Add(ToChar(y));
			}
			mdata.SetData("", "", new List<string>(), new List<string>(), new float[hexLines.Count,0], new bool[hexLines.Count,0],
				new float[hexLines.Count,0], "", true, new List<string>(new[]{"Hex", "Char"}),
				new List<string>(new[]{"Hex", "Char"}), new List<string[]>(new[]{hexLines.ToArray(), charLines.ToArray()}),
				new List<string>(), new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(),
				new List<double[]>(), new List<string>(), new List<string>(), new List<double[][]>(), new List<string>(),
				new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(), new List<double[]>());
		}

		private static string ToHex(byte b){
			return "" + hexAlphabet[b >> 4] + hexAlphabet[b & 0xF];
		}

		private static readonly HashSet<byte> replace =
			new HashSet<byte>(new byte[]{
				0x7F, 0x80, 0x81, 0x84, 0x85, 0x88, 0x8C, 0x8D, 0x8E, 0x8F, 0x90, 0x91, 0x92, 0x95, 0x99, 0x9A, 0x9B, 0x9D, 0x9E,
				0xAD
			});

		private static string ToChar(byte b){
			return b <= 0x1F || replace.Contains(b) ? "." : "" + (char) b;
		}

		private static string ToChar(IList<byte> b){
			if (b.Count == 0){
				return "";
			}
			StringBuilder sb = new StringBuilder(ToChar(b[0]));
			for (int i = 1; i < b.Count; i++){
				sb.Append(ToChar(b[i]));
			}
			return sb.ToString();
		}

		private static string ToHex(IList<byte> b){
			if (b.Count == 0){
				return "";
			}
			StringBuilder sb = new StringBuilder(ToHex(b[0]));
			for (int i = 1; i < b.Count; i++){
				sb.Append(" ");
				sb.Append(ToHex(b[i]));
			}
			return sb.ToString();
		}
	}
}