using Zenga;

namespace ZengaTests;

public class BoardTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void InitPos()
    {
        Board board = new Board();
        
        board.InitPos();
        
        Assert.Pass();
    }

    [TestCase(0b0, 0b0)]
    [TestCase(0b0, 0b1)]
    [TestCase(0b1, 0b0)]
    [TestCase(0b1, 0b1)]
    [TestCase(0b101, 0b011)]
    public void ValidatePlace(byte tower, byte block)
    {
        Board board = new Board();

        bool result = board.ValidatePlace(tower, block);

        bool pass = false;
        switch (tower, block)
        {
            case (0b0, 0b0):
                pass = result; break;
            case (0b0, 0b1):
                pass = result; break;
            case (0b1, 0b0):
                pass = result; break;
            case (0b1, 0b1):
                pass = !result; break;
            case (0b101, 0b011):
                pass = !result; break;
        }
        
        Assert.That(pass, Is.True);
    }
    
    [TestCase(0b0, 0b0)]
    [TestCase(0b0, 0b1)]
    [TestCase(0b1, 0b0)]
    [TestCase(0b1, 0b1)]
    [TestCase(0b101, 0b011)]
    public void ValidateRemove(byte tower, byte block)
    {
        Board board = new Board();

        bool result = board.ValidateRemove(tower, block);

        bool pass = false;
        switch (tower, block)
        {
            case (0b0, 0b0):
                pass = result; break;
            case (0b0, 0b1):
                pass = !result; break;
            case (0b1, 0b0):
                pass = result; break;
            case (0b1, 0b1):
                pass = result; break;
            case (0b101, 0b011):
                pass = !result; break;
        }
        
        Assert.That(pass, Is.True);
    }
}