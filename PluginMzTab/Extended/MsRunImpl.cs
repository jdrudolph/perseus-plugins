using System.IO;
using System.Security.Policy;
using MzTabLibrary.model;

namespace PluginMzTab.extended
{
    public class MsRunImpl : MsRun
    {
        private string _description;

        public MsRunImpl(int id) : base(id){
            
        }

        public MsRunImpl(MsRun run):base(run.Id){
            Format = run.Format;
            IdFormat = run.IdFormat;
            FragmentationMethod = run.FragmentationMethod;
            Location = run.Location;
        }

        public string Description{
            get{
                if (_description == null && Location != null && Location.Value != null){
                    _description = Path.GetFileNameWithoutExtension(Location.Value);
                }
                return _description;
            }
            /*set{                
                string extension = Format != null && Format.Accession == "MS:1000563" ? ".raw" : "";
                string path = Path.GetDirectoryName(_description);
                _description = Path.GetFileNameWithoutExtension(value);
                Location = new Url(Path.Combine(string.IsNullOrEmpty(path) ? Constants.DefaultPath : path, _description + extension));                
            }*/
        }

        public string FilePath{ get { return Location == null || Location.Value == null ? "" : Path.GetDirectoryName(Location.Value); } }

        public new Url Location{
            get { return base.Location; }
            set{
                Url url = value;
                string extention = Path.GetExtension(url.Value);
                if (string.IsNullOrEmpty(extention)){
                    if (Format != null && Format.Accession == "MS:1000563"){
                        url = new Url(url.Value + ".raw");
                    }
                }
                base.Location = url;
            }
        }
    }
}
