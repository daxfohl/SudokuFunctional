using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuFunctional {
    static class Sudoku {
        public const int SideLen = SideLenQuarter * SideLenQuarter;
        public const int SideLenQuarter = 3;
        public const int Allbits = (1 << SideLen) - 1;
        public static readonly Region[] Rows = Enumerable.Range(0, SideLen).Select(i => Region.Create(RegionType.Row, i)).ToArray();
        public static readonly Region[] Cols = Enumerable.Range(0, SideLen).Select(i => Region.Create(RegionType.Col, i)).ToArray();
        public static readonly Region[] Sqs = Enumerable.Range(0, SideLen).Select(i => Region.Create(RegionType.Sq, i)).ToArray();
        public static bool InTe;
        static readonly int[][] RowCells = Enumerable.Range(0, SideLen).Select(index => Enumerable.Range(0, SideLen).ToArray().Select(col => index * SideLen + col).ToArray()).ToArray();
        static readonly int[][] ColCells = Enumerable.Range(0, SideLen).Select(index => Enumerable.Range(0, SideLen).ToArray().Select(row => row * SideLen + index).ToArray()).ToArray();

        static readonly int[][] SqCells = Enumerable.Range(0, SideLen).Select(index => {
            var rowStart = index / SideLenQuarter * SideLenQuarter;
            var colStart = index % SideLenQuarter * SideLenQuarter;
            return (from row in Enumerable.Range(0, SideLenQuarter).ToArray().Select(i => i + rowStart)
                    from col in Enumerable.Range(0, SideLenQuarter).ToArray().Select(i => i + colStart)
                    select row * SideLen + col).ToArray();
        }).ToArray();

        public static int TeNext;
        public static int Te2Count;

        public static IEnumerable<int> Solve(IEnumerable<int> input) {
            var empty = Enumerable.Range(0, SideLen * SideLen).Select(i => Allbits).ToArray();
            var grid = input.Select(Valdex.Create).Aggregate(empty, InitCell);
            Console.WriteLine("Start " + grid.Sum((Func<int, int>)BitArrayEx.PopCount));
            return RunAll(new[] { TrialError(Run(LastRemaining)), SetIsolation, LastRemaining })(grid);
        }

        public static int[] LastRemaining(int[] grid) {
            var ret = Cols.Concat(Rows).Concat(Sqs).Aggregate(grid, LastRemaining2);
            if (!InTe) {
                Console.WriteLine(grid.PopCount() + " LR " + ret.PopCount());
            }
            return ret;
        }

        public static int[] LastRemaining2(int[] grid, Region region) {
            return Enumerable.Range(0, SideLen).Aggregate(grid, (gridx, value) => LastRemaining3(gridx, region, value));
        }

        public static int[] LastRemaining3(int[] grid, Region region, int value) {
            var p = (1 << value);
            var cells = region.Cells();
            //var first = GetFirst(-1, 0, cells, grid, p);
            var first = -1;
            foreach (var i in cells) {
                if ((grid[i] & p) != 0) {
                    if (first != -1) {
                        return grid;
                    }
                    first = i;
                }
            }
            return first == -1 ? grid : grid.SetCell(first, value);
        }

        public static int[] SetIsolation(int[] grid) {
            var ret = (from i in Enumerable.Range(0, grid.Length)
                       from region in i.Intersections()
                       select Tuple.Create(i, region))
                .Aggregate(grid, (gridx, tuple) => SetIsolation2(gridx, tuple.Item2, gridx[tuple.Item1]));
            Console.WriteLine(grid.PopCount() + " SI " + ret.PopCount());
            return ret;
        }

        public static int[] SetIsolation2(int[] grid, Region region, int p) {
            var indices = region.Cells();
            Func<int, bool> isLimited = i => (grid[i] | p) == p;
            return indices.Count(isLimited) == p.PopCount() ? indices.Where(i => !isLimited(i)).Aggregate(grid, (gridx, i) => gridx.Eliminate(i, p)) : grid;
        }

        public static Func<int[], int[]> TrialError(Func<int[], int[]> f) {
            return grid => {
                InTe = true;
                var ret = Enumerable.Range(2, grid.Length)
                    .Select(i => {
                        TeNext = i;
                        return TrialError2(f, grid, i % (SideLen * SideLen));
                    })
                    .FirstOrDefault(g2 => !g2.SequenceEqual(grid)) ?? grid;
                InTe = false;Console.WriteLine(grid.PopCount() + " TE " + ret.PopCount());
                return ret;
            };
        }

        static int[] TrialError2(Func<int[], int[]> f, int[] grid, int index) {
            ++Te2Count;
            var p = grid[index];
            return Enumerable.Range(0, SideLen)
                .Where(value => (p | (1 << value)) == p)
                .Aggregate(grid, (gridx, value) => TrialError3(f, gridx, index, value));
        }

        static int[] TrialError3(Func<int[], int[]> f, int[] grid, int index, int value) {
            var clone = grid.SetCell(index, value);
            var funcked = f(clone);
            return funcked.IsConsistent() ? grid : grid.Eliminate(index, 1 << value);
        }

        static bool IsConsistent(this IEnumerable<int> grid) {
            return grid.All(i => i != 0);
        }

        public static Func<int[], int[]> Run(Func<int[], int[]> f) {
            return grid => {
                var gridx = f(grid);
                return gridx.SequenceEqual(grid) ? grid : Run(f)(gridx);
            };
        }

        public static Func<int[], int[]> RunAll(IEnumerable<Func<int[], int[]>> fs) {
            var fsp = fs.ToArray();
            if (fsp.Length == 0) {
                return grid=>grid;
            }
            return grid => {
                var gridx = RunAll(fsp.Skip(1))(grid);
                var gridy = fsp[0](gridx);
                return gridy.SequenceEqual(gridx) ? gridx : RunAll(fsp)(gridy);
            };
        }

        public static int[] InitCell(int[] grid, Valdex valdex) {
            return valdex.Value == -1 ? grid : grid.SetCell(valdex.Index, valdex.Value);
        }

        public static int[] SetCell(this int[] grid, int i, int value) {
            return grid.Eliminate(i, ~(1 << value));
        }

        public static int[] Eliminate(this int[] grid, int i, int elim) {
            var p1 = grid[i];
            var p2 = p1 & (~elim);
            if (p1 == p2) {
                return grid;
            }
            var g2 = new int[grid.Length];
            Buffer.BlockCopy(grid, 0, g2, 0, grid.Length * 4);
            g2[i] = p2;
            return p2.Solved() ? g2.Isolate(i) : g2;
        }

        public static int[] Isolate(this int[] grid, int i) {
            var p = grid[i];
            var bros = i.Brothers().ToArray();
            var g2 = new int[grid.Length];
            Buffer.BlockCopy(grid, 0, g2, 0, grid.Length * 4);
            foreach (var bro in bros) {
                g2[bro] = grid[bro] & (~p);
            }
            return bros.Where(bro => g2[bro].Solved() && !grid[bro].Solved()).Aggregate(g2, Isolate);
        }

        public static IEnumerable<int> Brothers(this int i) {
            return (from region in i.Intersections()
                    from cell in region.Cells()
                    where cell != i
                    select cell).Distinct();
        }

        public static int[] Cells(this Region region) {
            if (region.Index >= 25) {
                throw new ArgumentException();
            }
            switch (region.Type) {
                case RegionType.Row:
                    return RowCells[region.Index];
                case RegionType.Col:
                    return ColCells[region.Index];
                case RegionType.Sq:
                    return SqCells[region.Index];
            }
            throw new ArgumentException();
        }

        public static IEnumerable<Region> Intersections(this int i) {
            var row = i / SideLen;
            var col = i % SideLen;
            yield return Rows[row];
            yield return Cols[col];
            yield return Sqs[row / SideLenQuarter * SideLenQuarter + col / SideLenQuarter];
        }


        public static bool Solved(this int p) {
            return (p & (p - 1)) == 0;
        }
    }
}