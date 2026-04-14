using Zenga;

namespace ZengaTests;

public class UtilTests
{
    [SetUp]
    public void Setup() {}

    [Test]
    public void BoardToString()
    {
        Board board = new(height: 3)
        {
            Tower = [2, 2, 7, 7, 1, 0, 0, 0, 0]
        };

        var result = ZengaStrings.BoardToString(board);
        
        Assert.That(result, Is.EqualTo("2/2/7/7/1/0/0/0/0 3 3 W"));
    }

    [Test]
    public void StringToBoard()
    {
        Board? board = ZengaStrings.StringToBoard("2/2/7/7/1/0/0/0/0 3 3 W");

        Assert.That(board, Is.Not.Null);
        Assert.That(board.HeightIndex, Is.EqualTo(4));
        Assert.That(board.Height, Is.EqualTo(3)); Assert.That(board.Width, Is.EqualTo(3));
        Assert.That(board.maxHeight, Is.EqualTo(9));
        Assert.That(board.side, Is.EqualTo(Side.W));
        Assert.That(board.Tower, Is.EqualTo([2, 2, 7, 7, 1, 0, 0, 0, 0]));
    }
}