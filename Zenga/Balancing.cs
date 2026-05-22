namespace Zenga;

public abstract class Balancing
{
    public decimal[] Bal { get; set; }
    public decimal ZBal = 0;
    public int Width;

    public Balancing(Board board)
    {
        this.Bal = new decimal[board.Height];
        this.Width = board.Width;
    }

    public Balancing(int height, int width)
    {
        Bal = new decimal[height];
        this.Width = width;
    }
    
    public abstract decimal Calculate(Board board);
    public abstract decimal Update(Move move);
    //public abstract decimal GetLegalMoves();
}

public class WeightBalancing : Balancing
{
    public decimal endBlockCoG;
    private int  
    
    public WeightBalancing(Board board) : base(board)
    {
        this.endBlockCoG = (Width - 1) / (decimal)2;
    }
    public WeightBalancing(int height, int width) : base(height, width) {}

    public override decimal Calculate(Board board)
    {
        throw new NotImplementedException();
    }

    public override decimal Update(Move move)
    {
        throw new NotImplementedException();
    }

    public decimal Aggregate()
    {
        throw new NotImplementedException();
    }

    public decimal GetLayerCoG(byte layer, int mass)
    {
        decimal CoGSum = 0;
        
        for (int i = 0; i < Width; i++)
        {
            CoGSum += (layer >> i) % 2 == 1 ? endBlockCoG - i : 0;
        }

        return CoGSum / mass;
    }
}