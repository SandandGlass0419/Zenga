using Zenga;

namespace ZengaTests;

public class BoardTests
{
    [SetUp]
    public void Setup() {}

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

    [Test]
    public void GetHeightIndex()
    {
        Board board = new(height: 3)
        {
            Tower = [0, 1, 2, 1, 0, 0, 1, 1, 0]
        };

        int result = board.GetHeightIndex();

        Assert.That(result, Is.EqualTo(7), $"got {result}");
    }

    [Test]
    public void RemoveBlockHeightIndexUpdate()
    {
        Board board = new();
        board.InitPos();
        
        board.RemoveBlock(new(0, 16));

        board.RemoveBlock(new((byte)0b111, 2));
        
        board.RemoveBlock(new((byte)0b111, 17));

        Assert.That(board.HeightIndex, Is.EqualTo(16));
    }

    [Test]
    public void PlaceBlockHeightIndexUpdate()
    {
        Board board = new();
        board.InitPos();
        
        board.PlaceBlock(new(1, 18));
        
        board.PlaceBlock(new(0, 18));
        
        board.PlaceBlock(new((byte)0b111, 19));
        
        Assert.That(board.HeightIndex, Is.EqualTo(19));
    }

    [Test]
    public void ApplyMoveRefill()
    {
        Board board = new();
        board.InitPos();

        Move move = new(new((byte)0b101, 3), new((byte)0b001, 3));
        board.ApplyMove(move);

        Assert.That(board.Tower[3], Is.EqualTo((byte)0b011));
    }
    
    [Test]
    public void ApplyMoveStack()
    {
        Board board = new();
        board.InitPos();

        Move move = new(new((byte)0b101, 3), new((byte)0b001, 18));
        board.ApplyMove(move);

        Assert.That(board.Tower[3], Is.EqualTo((byte)0b010));
        Assert.That(board.Tower[18], Is.EqualTo((byte)0b001));
    }

    [Test]
    public void UndoMoveRefill()
    {
        Board board = new();
        board.InitPos();

        Move move = new(new((byte)0b101, 3), new(0, 18));
        board.ApplyMove(move);
        
        board.UndoMove(move);

        Assert.That(board.Tower[3], Is.EqualTo(0b111));
        Assert.That(board.Tower[18], Is.EqualTo(0));
    }
    
    [Test]
    public void UndoMoveStack()
    {
        Board board = new();
        board.InitPos();

        Move move = new(new((byte)0b101, 3), new(0, 3));
        board.ApplyMove(move);
        
        board.UndoMove(move);

        Assert.That(board.Tower[3], Is.EqualTo(0b111));
    }
}