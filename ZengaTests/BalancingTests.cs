using Zenga;

namespace ZengaTests;

public class CoGTests
{
    public Board board = new();
    public CoGBalancing balancing;

    [SetUp]
    public void Setup()
    {
        balancing = new(3, 3);
    }

    [TestCase(0b1011)]
    public void GetMassTest(byte layer)
    {
        var mass = balancing.GetLayerMass(layer);

        Assert.That(mass, Is.EqualTo(3m));
    }

    [TestCase(0b101)]
    public void GetCoGTest(byte layer)
    {
        balancing = new(18, 3);
        var CoG = balancing.GetLayerCoG(layer, balancing.GetLayerMass(layer));
        
        Assert.That(CoG, Is.EqualTo(0));
    }

    [TestCase(0b100, Axis.X, 0b011)]
    public void UpdateCoGTest(byte layer, Axis axis, byte layer2)
    {
        balancing.UpdateBatchCoG(layer, axis);
        axis.RefCycle();
        balancing.UpdateBatchCoG(layer2, axis);
        
        Assert.Pass();
    }

    [Test]
    public void ResetTest()
    {
        balancing.MaxBalance = new(10, Axis.Z);
        balancing.batchCoG = (100, -100);
        balancing.batchMass = 10000;
        
        balancing.Reset();
        
        Assert.Pass();
    }
    
    [TestCase(0b100, 0b011, 0b100)]
    public void BalanceTest(byte layer, byte layer1, byte layer2)
    {
        balancing.UpdateBatchCoG(layer, Axis.X);

        var balance1 = balancing.GetBalance(layer1, Axis.Z);
        balancing.UpdateBatchCoG(layer1, Axis.Z);

        var balance2 = balancing.GetBalance(layer2, Axis.X);
        balancing.UpdateBatchCoG(layer2, Axis.X);
        
        Assert.Pass();
    }

    [TestCase(new byte[] {5,5,7,7,7,7,7,7,7})]
    [TestCase(new byte[] {5, 6, 2, 5, 3, 3, 1, 0, 0})]
    [TestCase(new byte[] {0b101, 0b011, 0b101, 0b010, 0, 0, 0, 0, 0})]
    public void CalculateTest(byte[] tower)
    {
        Board testBoard = new(height: 3)
        {
            Tower = tower // height * width
        };

        //CoGBalancing testBalance = new(testBoard.Height, testBoard.Width);
        var maxbal = balancing.Calculate(testBoard);
        
        Assert.Pass();
    }
}