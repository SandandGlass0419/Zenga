namespace Zenga;

public abstract class Balancing
{
    public decimal[] Balance { get; set; }
    
    public readonly int height;
    public readonly int width;
    
    public Balancing(int height, int width)
    {
        Balance = new decimal[height * width];
        this.height = height;
        this.width = width;
    }

    public abstract void Reset();
    public abstract Tuple<decimal, Axis> Calculate(Board board);
    //public abstract decimal GetLegalMoves();
    //public abstract decimal Update(Move move);
}

public class CoGBalancing : Balancing
{
    private readonly decimal endBlockCoG;
    private decimal batchMass;
    private decimal[] batchCoG = [0, 0];

    public CoGBalancing(int height, int width) : base(height, width)
    {
        this.endBlockCoG = (width - 1) / 2m;
    }

    public override void Reset()
    {
        Balance = new decimal[height * width];
        batchMass = 0;
        batchCoG = new decimal[] { 0, 0 };
    }

    public override Tuple<decimal, Axis> Calculate(Board board)
    {
        Tuple<decimal, Axis> maxBal = new(0, Axis.NONE);
        
        Axis axis = (Axis)(board.heightIndex % 2);

        UpdateBatchCoG(board.Tower[board.heightIndex], axis);
        axis = axis.Cycle();
        
        for (int i = board.heightIndex - 1; i >= 0; i--)
        {
            decimal bal = GetBalance(board.Tower[i], axis);
            Balance[i] = bal;
            
            maxBal = Math.Abs(bal) > Math.Abs(maxBal.Item1) ? new(bal, axis) : maxBal;
            
            UpdateBatchCoG(board.Tower[i], axis);

            axis = axis.Cycle();
        }

        return maxBal;
    }

    public decimal GetBalance(byte supportLayer, Axis axis)
    {
        var range = GetSupportRange(supportLayer);
        decimal center = (range.Item1 + range.Item2) / 2;
        decimal rangeLen = (range.Item2 - range.Item1) / 2;

        return (batchCoG[(int)axis] - center) / rangeLen;
    }

    public Tuple<decimal, decimal> GetSupportRange(byte supportLayer)
    {
        decimal min = 0, max = 0;
        bool foundMax = false;
        
        for (int i = 0; i < 8; i++)
        {
            if ((supportLayer >> i) % 2 != 1) continue;

            if (!foundMax)
            {
                max = width / 2m - i;
                foundMax = true;
            }
            
            min = width / 2m - i - 1;
        }

        return new(min, max);
    }

    public void UpdateBatchCoG(byte layer, Axis axis)
    {
        batchCoG[(int)axis] = BatchCoGFront(layer, axis);
        
        batchCoG[(int)axis.Cycle()] = BatchCoGSide(layer, axis.Cycle());

        batchMass += GetLayerMass(layer);
    }
    
    public decimal BatchCoGFront(byte layer, Axis axis)
    {
        decimal layerMass = GetLayerMass(layer);
        decimal layerCoG = GetLayerCoG(layer, layerMass);

        return (batchCoG[(int)axis] * batchMass + layerCoG * layerMass) / (batchMass + layerMass);
    }

    public decimal BatchCoGSide(byte layer, Axis axis)
    {
        decimal layerMass = GetLayerMass(layer);
        
        return (batchCoG[(int)axis] * batchMass) / (batchMass + layerMass); // layerCoG = 0
    }

    public decimal GetLayerMass(byte layer)
    {
        int mass = 0;

        for (int i = 0; i < 8; i++)
        {
            mass += (layer >> i) % 2 == 1 ? 1 : 0;
        }

        return mass;
    }

    public decimal GetLayerCoG(byte layer, decimal mass)
    {
        decimal CoGSum = 0;
        
        for (int i = 0; i < width; i++)
        {
            CoGSum += (layer >> i) % 2 == 1 ? endBlockCoG - i : 0;
        }

        return CoGSum / mass;
    }
}