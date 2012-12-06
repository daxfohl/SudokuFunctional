namespace SudokuFunctional {
    struct Region {
        public readonly int Index;
        public readonly RegionType Type;

        public Region(RegionType type, int index) {
            Type = type;
            Index = index;
        }

        public static Region Create(RegionType type, int index) {
            return new Region(type, index);
        }

        public override string ToString() {
            return Type + " " + Index;
        }
    }
}