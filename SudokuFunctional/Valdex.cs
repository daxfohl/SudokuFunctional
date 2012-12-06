namespace SudokuFunctional {
    class Valdex {
        public readonly int Index;
        public readonly int Value;

        public Valdex(int value, int index) {
            Value = value;
            Index = index;
        }

        public static Valdex Create(int value, int index) {
            return new Valdex(value, index);
        }
    }
}