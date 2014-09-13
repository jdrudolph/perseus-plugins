using System.Collections.Generic;
using BaseLibS.Util;
using PerseusApi.Generic;

namespace PerseusApi.Document{
	public interface IDocumentData : IData{
		List<string> Text { get; set; }
		List<string> Header { get; set; }
		List<DocumentType> Type { get; set; }
		void AddTextBlock(string text);
		void AddTextBlock(string text, string header);
		void AddTextBlock(string text, string header, DocumentType type);
	}
}