using System.Windows;
using BaseLib.Param;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class SampleProcessingParam : Parameter{
        private readonly CVLookUp _cv;
        private readonly int _count;
        private SplitList<Lib.Model.Param>[] Default { get; set; }
        public SplitList<Lib.Model.Param>[] Value { get; private set; }

        public const string Label = "step count";

        public SampleProcessingParam(SplitList<Lib.Model.Param>[] steps, CVLookUp cv) : base(null){
            _cv = cv;
            _count = steps.Length;
            Value = steps;
            Default = steps;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
            SampleProcessingPanel p = control as SampleProcessingPanel;
            if (p == null){
                return;
            }
            Value = p.Value;
        }

        public override void UpdateControlFromValue(){
            SampleProcessingPanel p = control as SampleProcessingPanel;
            if (p == null){
                return;
            }
            p.Value = Value;
        }

        public override object Clone(){
            return new SampleProcessingParam(Value, _cv);
        }

        public override void Clear(){
            Value = new SplitList<Lib.Model.Param>[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

        protected override UIElement Control { get { return new SampleProcessingPanel(Value, _cv); } }

        public override float Height { get { return SampleProcessingPanel.MiniumHeight; } }
    }
}