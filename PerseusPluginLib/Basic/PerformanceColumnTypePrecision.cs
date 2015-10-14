namespace PerseusPluginLib.Basic{
	public class PerformanceColumnTypePrecision : PerformanceColumnType{
		public override string Name => "TP/(TP+FP) (Precision)";

		public override double Calculate(double tp, double tn, double fp, double fn, double np, double nn){
			return tp/(tp + fp);
		}
	}
}