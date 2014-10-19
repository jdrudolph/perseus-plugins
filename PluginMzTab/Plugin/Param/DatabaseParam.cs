using System.Windows;
using BaseLib.Param;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param {
    class DatabaseParam : Parameter{
        private readonly int _count;
        private Database[] Default { get; set; }
        public Database[] Value { get; set; }

        public DatabaseParam(Database[] databases)
            : base(null){
            _count = databases.Length;
            Value = databases;
            Default = databases;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
            DatabasePanel p = control as DatabasePanel;
            if (p == null){
                return;
            }
            Value = p.Value;
        }

        public override void UpdateControlFromValue(){
            DatabasePanel p = control as DatabasePanel;
            if (p == null){
                return;
            }
            p.Value = Value;
        }

        public override object Clone(){
            return new DatabaseParam(Value) { Default = Default };
        }

        public override void Clear(){
            Value = new Database[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

        protected override UIElement Control { get { return new DatabasePanel(Value); } }

        public override float Height { get { return DatabasePanel.MiniumHeight; } }
    }
}
