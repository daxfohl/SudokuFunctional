using System.Collections.Generic;
using System.Linq;

namespace SudokuFunctional {
    public static class BitArrayEx {
        public static int PopCount(this int ii) {
            ii = (ii >> 1 & 0x55555555) + (ii & 0x55555555);
            ii = (ii >> 2 & 0x33333333) + (ii & 0x33333333);
            ii = (ii >> 4 & 0x0f0f0f0f) + (ii & 0x0f0f0f0f);
            ii = (ii >> 8 & 0x00ff00ff) + (ii & 0x00ff00ff);
            return (ii >> 16) + (ii & 0x0000ffff);
        }
        public static int PopCount(this IEnumerable<int> grid) {
            return grid.Sum(c => c.PopCount());
        }
        public static int Max(this int i) {
            return Enumerable.Range(0, 32).Last(ii => ((1 << ii) & i) == i);
        }
    }
}