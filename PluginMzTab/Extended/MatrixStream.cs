using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BaseLib.Util;
using PerseusApi.Matrix;

namespace PluginMzTab.extended
{
    public class MatrixStream : Stream
    {
        private Encoding encode = Encoding.Default;
        private readonly IMatrixData[] _inputData;
        private readonly int[] _row;
        private long _total;
        private long _position;
        
        private int index;
        private int n;
        
        public MatrixStream(IMatrixData[] inputData, bool[] hasHeader){
            _inputData = inputData;
            
            _row = new int[inputData.Length];
            for (int i = 0; i < inputData.Length; i++){
                _row[i] = hasHeader[i] ? -1 : 0;
            }

            _total = GetLength(inputData, _row);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value){
            _total = value;
        }

        public override int Read(byte[] buffer, int offset, int count){
            int position = 0;
            if (offset > 0){
                throw  new NotImplementedException();
            }
            
            while (position < count && position < buffer.Length && position < Length && n < _inputData.Length){
                if (_row[n] == _inputData[n].RowCount){
                    n++;
                    continue;
                }
                IList<string> values;
                if (_row[n] == -1){
                    values = new List<string>(_inputData[n].StringColumnNames);
                }else{
                    values = new List<string>();
                    for (int i = 0; i < _inputData[n].StringColumnNames.Count; i++){
                        values.Add(_inputData[n].StringColumns[i][_row[n]]);
                    }
                }

                byte[] array = encode.GetBytes(StringUtils.Concat("\t", values) + "\n");

                for (int i = index; i < array.Length && position < buffer.Length; i++){
                    buffer[position] = array[i];
                    index = i  + 1;
                    position++;
                }

                if (index == array.Length){                
                    index = 0;
                    _row[n]++;
                }                
            }
            _position = position;

            return position;
        }

        private int GetLength(IMatrixData[] inputData, int[] row){
            int len = 0;

            Encoding encode = Encoding.Default;

            for (int i = 0; i < inputData.Length; i++){
                IMatrixData matrix = inputData[i];
                IList<string> values;
                if (row[i] == -1){
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
