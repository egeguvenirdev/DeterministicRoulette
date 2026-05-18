using System.Collections.Generic;

/// <summary>
/// Computes valid inside-bet options for a given roulette number based on board position.
/// Board layout (3 rows x 12 cols):
///   Row 2 (top):    3,  6,  9, ..., 36
///   Row 1 (mid):    2,  5,  8, ..., 35
///   Row 0 (bottom): 1,  4,  7, ..., 34
/// </summary>
public static class RouletteNeighborCalculator
{
    public class BetOption
    {
        public BetType betType;
        public List<int> targetNumbers;
        public string label;
    }

    private static int Row(int n) => (n - 1) % 3;
    private static int Col(int n) => (n - 1) / 3;
    private static int Num(int row, int col) => col * 3 + row + 1;
    private static bool Valid(int n) => n >= 1 && n <= 36;

    public static List<BetOption> GetOptions(int number)
    {
        var options = new List<BetOption>();

        if (number == 0)
        {
            options.Add(Straight(0));
            return options;
        }

        if (!Valid(number))
        {
            return options;
        }

        int r = Row(number);
        int c = Col(number);

        // Straight
        options.Add(Straight(number));

        // Splits
        if (c < 11) AddSplit(options, number, Num(r, c + 1)); // right neighbor
        if (c > 0)  AddSplit(options, number, Num(r, c - 1)); // left neighbor
        if (r < 2)  AddSplit(options, number, Num(r + 1, c)); // upper neighbor
        if (r > 0)  AddSplit(options, number, Num(r - 1, c)); // lower neighbor

        // Street (all 3 numbers in same column)
        int s1 = Num(0, c), s2 = Num(1, c), s3 = Num(2, c);
        options.Add(new BetOption
        {
            betType = BetType.Street,
            targetNumbers = new List<int> { s1, s2, s3 },
            label = $"Street {s1}-{s2}-{s3}"
        });

        // Corners: number can be in up to 4 different 2x2 blocks
        TryCorner(options, r,     c);      // number is bottom-left of block
        TryCorner(options, r - 1, c);      // number is top-left of block
        TryCorner(options, r,     c - 1);  // number is bottom-right of block
        TryCorner(options, r - 1, c - 1);  // number is top-right of block

        // SixLine (2 adjacent columns, 6 numbers total)
        if (c < 11) AddSixLine(options, c, c + 1);
        if (c > 0)  AddSixLine(options, c - 1, c);

        return options;
    }

    private static BetOption Straight(int number)
    {
        return new BetOption
        {
            betType = BetType.Straight,
            targetNumbers = new List<int> { number },
            label = $"Straight {number}"
        };
    }

    private static void AddSplit(List<BetOption> options, int a, int b)
    {
        int lo = a < b ? a : b;
        int hi = a < b ? b : a;
        options.Add(new BetOption
        {
            betType = BetType.Split,
            targetNumbers = new List<int> { lo, hi },
              label = $"Split {lo}-{hi}"
        });
    }

    // blR, blC = bottom-left cell of a 2x2 corner block
    private static void TryCorner(List<BetOption> options, int blR, int blC)
    {
        if (blR < 0 || blR > 1 || blC < 0 || blC > 10)
        {
            return;
        }

        int n1 = Num(blR,     blC);
        int n2 = Num(blR + 1, blC);
        int n3 = Num(blR,     blC + 1);
        int n4 = Num(blR + 1, blC + 1);
        options.Add(new BetOption
        {
            betType = BetType.Corner,
            targetNumbers = new List<int> { n1, n2, n3, n4 },
                label = $"Corner {n1}-{n2}-{n3}-{n4}"
        });
    }

    private static void AddSixLine(List<BetOption> options, int c1, int c2)
    {
        var nums = new List<int>
        {
            Num(0, c1), Num(1, c1), Num(2, c1),
            Num(0, c2), Num(1, c2), Num(2, c2)
        };
        nums.Sort();
        options.Add(new BetOption
        {
            betType = BetType.SixLine,
            targetNumbers = nums,
                label = $"SixLine {string.Join("-", nums)}"
        });
    }
}
