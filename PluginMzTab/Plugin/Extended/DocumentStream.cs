using System;
using System.IO;
using System.Text;
using PerseusApi.Document;

namespace PluginMzTab.Plugin.Extended{
    public class DocumentStream : Stream{
        private readonly Encoding encode = Encoding.Default;
        private readonly IDocumentData _doc;

        private long WritePosition;

        public DocumentStream(IDocumentData doc){
            _doc = doc;
        }

        public override void Flush(){}

        public override long Seek(long offset, SeekOrigin origin){
            throw new NotImplementedException();
        }

        public override void SetLength(long value){
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count){
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count){
            Position = WritePosition;
            if (_doc == null){
                return;
            }
            if (offset > 0){
                throw new NotImplementedException();
            }

            string text = encode.GetString(buffer);
            text = text.Replace("\0", "");
            _doc.AddTextBlock(text);

            ClearBuffer(buffer);

            Position += encode.GetByteCount(text);

            WritePosition = Position;
        }

        public void ClearBuffer(byte[] buffer){
            for (int i = 0; i < buffer.Length; i++){
                buffer[i] = 0;
            }
        }

        public override bool CanRead { get { return false; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return _doc != null; } }

        public override long Length { get { throw new NotImplementedException(); } }

        public override long Position { get; set; }
    }
}