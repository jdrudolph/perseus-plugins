using System.IO;
using System.Security.Policy;
using PluginMzTab.Lib.Model;

namespace PluginMzTab.Plugin.Extended{
    public class MsRunImpl : MsRun{
        public MsRunImpl(int id) : base(id){}

        public MsRunImpl(MsRun run) : base(run.Id){
            Format = run.Format;
            IdFormat = run.IdFormat;
            FragmentationMethod = run.FragmentationMethod;
            Location = run.Location;
        }

        public string Description{
            get{
                return Location != null && Location.Value != null ? Path.GetFileNameWithoutExtension(Location.Value) : null;
            }
        }

        public string FilePath { get { return Location == null || Location.Value == null ? "" : Path.GetDirectoryName(Location.Value); } }


    }
}