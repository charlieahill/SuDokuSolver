using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuDokuSolver
{
    public class IntAndResult
    {
        /// <summary>
        /// The value for the cell that was tried
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// The SuDoku game result
        /// </summary>
        public SuDokuGameResult Result { get; set; }
        /// <summary>
        /// Stores an integer value for a cell and the result of trying that value
        /// </summary>
        /// <param name="_number">The value for the cell that was tried</param>
        /// <param name="_result">The SuDoku game result</param>
        public IntAndResult(int _number, SuDokuGameResult _result)
        {
            Number = _number;
            Result = _result;
        }

        public override string ToString()
        {
            return $"{Number} ({Result})";
        }
    }
}
