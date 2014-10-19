using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BaseLibS.Util;
using PerseusApi.Matrix;

namespace PluginMzTab.Plugin.Extended{
    internal class MatrixStream : Stream{
        private readonly Encoding encode = Encoding.Default;
        private readonly IMatrixData _inputData;
        private int _row;
        private long _total;
        private long _position;

        private int index;

        public MatrixStream(IMatrixData inputData, bool hasHeader){
            _inputData = inputData;
            _row = hasHeader ? -1 : 0;
            _total = GetLength(inputData, _row);
        }

        public override void Flush(){
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin){
            throw new NotImplementedException();
        }

        public override void SetLength(long value){
            _total = value;
        }

        public override int Read(byte[] buffer, int offset, int count){
            int position = 0;
            if (offset > 0){
                throw new NotImplementedException();
            }

            while (position < count && position < buffer.Length && position < Length){
                if (_row == _inputData.RowCount){
                    break;
                }
                IList<string> values;
                if (_row == -1){
                    values = new List<string>(_inputData.StringColumnNames);
                }
                else{
                    values = new List<string>();
                    for (int i = 0; i < _inputData.StringColumnNames.Count; i++){
                        values.Add(_inputData.StringColumns[i][_row]);
                    }
                }

                byte[] array = encode.GetBytes(StringUtils.Concat("\t", values) + "\n");

                for (int i = index; i < array.Length && position < buffer.Length; i++){
                    buffer[position] = array[i];
                    index = i + 1;
                    position++;
                }

                if (index == array.Length){
                    index = 0;
                    _row++;
                }
            }
            _position = position;

            return position;
        }

        private int GetLength(IMatrixData matrix, int row){
            int len = 0;

            IList<string> values;
            if (row == -1){
                values = new List<string>(matrix.StringColumnNames);
                len += encode.GetByteCount(StringUtils.Concat("\t", values) + "\n");
            }
            else{
                values = new List<string>();
            }

            for (int r = 0; r < matrix.RowCount; r++){
                values.Clear();
                for (int c = 0; c < matrix.StringColumnNames.Count; c++){
                    values.Add(matrix.StringColumns[c][r]);
                }
                len += encode.GetByteCount(StringUtils.Concat("\t", values) + "\n");
            }


            return len;
        }

        public override void Write(byte[] buffer, int offset, int count){
            throw new NotImplementedException();
        }

        public override bool CanRead { get { return _inputData != null; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return false; } }

        public override long Length { get { return _total; } }

        public override long Position { get { return _position; } set { _position = value; } }
    }
}