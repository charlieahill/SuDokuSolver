using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuDokuSolver
{
    public class CellOptionModel
    {
        public int XCoordinate { get; set; }
        public int YCoordinate { get; set; }
        public int Value { get; set; }
        public CellOptionModel(int _x, int _y, int _value)
        {
            XCoordinate = _x;
            YCoordinate = _y;
            Value = _value;
        }

        public override string ToString()
        {
            return $"{Value} ({XCoordinate}, {YCoordinate})";
        }
    }
}
