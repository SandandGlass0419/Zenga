namespace Zenga;

public abstract class Balancing
{
    public Balance[] currentBalance { get; set; }
    
    public int height;
    public int width;
    
    public Balancing(int height, int width)
    {
        currentBalance = new Balance[height * width];
        this.height = height;
        this.width = width;
    }

    public abstract void Reset();
    public abstract Balance Calculate(Board board);
}

public record struct Balance(decimal value, Axis axis)
{
    public static readonly Balance Default = new(0, Axis.NONE);
}

public class CoGBalancing : Balancing
{
    public decimal endBlockCoG;
    public decimal batchMass;
    public (decimal x, decimal z) batchCoG = (0, 0);

    public decimal GetBatchCoG(Axis axis)
    {
        return axis == Axis.Z ? batchCoG.z : batchCoG.x;
    }

    public void SetBatchCoG(Axis axis, decimal value)
    {
        if (axis == Axis.Z)
            batchCoG.z = value;
        else
            batchCoG.x = value;
    }

    public Balance MaxBalance
    {
        get;
        set
        {
            if (Math.Abs(value.value) >= Math.Abs(field.value) || value == Balance.Default)
            {
                field = value;
            }
        }
    } = Balance.Default;
    
    public CoGBalancing(int height, int width) : base(height, width)
    {
        this.endBlockCoG = (width - 1) / 2m;
    }

    public override void Reset()
    {
        currentBalance = new Balance[height * width];
        batchMass = 0;
        batchCoG = (0, 0);
        MaxBalance = Balance.Default;
    }

    public override Balance Calculate(Board board)
    {
        Axis axis = (Axis)(board.heightIndex % 2);

        UpdateBatchCoG(board.Tower[board.heightIndex], axis);
        axis.RefCycle();
        
        for (int i = board.heightIndex - 1; i >= 0; i--)
        {
            Balance bal = GetBalance(board.Tower[i], axis);
            
            currentBalance[i] = bal;
            MaxBalance = bal;
            
            UpdateBatchCoG(board.Tower[i], axis);

            axis.RefCycle();
        }

        return MaxBalance;
    }

    public Balance GetBalance(byte supportLayer, Axis axis)
    {
        var range = GetSupportRange(supportLayer);
        decimal center = (range.Item1 + range.Item2) / 2;
        decimal rangeLen = (range.Item2 - range.Item1) / 2;

        if (rangeLen <= 0) return Balance.Default;
        
        decimal balanceValue = (GetBatchCoG(axis) - center) / rangeLen;

        return new(balanceValue, axis);
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
        SetBatchCoG(axis, BatchCoGFront(layer, axis));

        axis.RefCycle();
        
        SetBatchCoG(axis, BatchCoGSide(layer, axis));

        batchMass += GetLayerMass(layer);
    }
    
    public decimal BatchCoGFront(byte layer, Axis axis)
    {
        decimal layerMass = GetLayerMass(layer);
        decimal layerCoG = GetLayerCoG(layer, layerMass);

        return (GetBatchCoG(axis) * batchMass + layerCoG * layerMass) / (batchMass + layerMass);
    }

    public decimal BatchCoGSide(byte layer, Axis axis)
    {
        decimal layerMass = GetLayerMass(layer);
        
        return (GetBatchCoG(axis) * batchMass) / (batchMass + layerMass); // layerCoG = 0
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