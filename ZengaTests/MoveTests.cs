using Zenga;

namespace ZengaTests;

public class MoveTests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase(1)]
    [TestCase(0)]
    [TestCase(-1)]
    public void CreateBlockmoves(int value)
    {
        BlockMove blockMove = new(value, 0);
        
        Assert.ByVal(blockMove.movingBlock, Is.EqualTo((int)Math.Pow(2, value)), "should be 2^value");
    }

    [TestCase(0b0, 0b0)]
    [TestCase(0b0, 0b1)]
    [TestCase(0b1, 0b0)]
    [TestCase(0b1, 0b1)]
    public void PlaceCombinationSingle(byte tower, byte block)
    {
        BlockMove blockMove = new(block, 0);

        var result = blockMove.PlaceTo(tower);

        bool pass = false;
        switch (tower, block)
        {
            case (0b0, 0b0):
                pass = result == 0b0; break;
            case (0b0, 0b1):
                pass = result == 0b1; break;
            case (0b1, 0b0):
                pass = result == 0b1; break;
            case (0b1, 0b1):
                pass = result == 0b1; break;
        }
        
        Assert.That(pass, Is.True, $"result: {result}");
    } 

    [TestCase(0b101, 0b010)]
    [TestCase(0b010, 0b101)]
    [TestCase(0b111, 0b111)]
    [TestCase(0b011, 0b110)]
    public void PlaceCombinationMultiple(byte tower, byte block)
    {
        BlockMove blockMove = new(block, 0);

        var result = blockMove.PlaceTo(tower);

        bool pass = false;
        switch (tower, block)
        {
            case (0b101, 0b010):
                pass = result == 0b111; break;
            case (0b010, 0b101):
                pass = result == 0b111; break;
            case (0b111, 0b111):
                pass = result == 0b111; break;
            case (0b011, 0b110):
                pass = result == 0b111; break;
        }
        
        Assert.That(pass, Is.True, $"result: {result}");
    }
}