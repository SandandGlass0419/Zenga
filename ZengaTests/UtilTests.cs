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

        var result = ZengaStrings.ToString(board);
        
        Assert.That(result, Is.EqualTo("2/2/7/7/1/0/0/0/0 3 3"));
    }

    [Test]
    public void StringToBoard()
    {
        Board? board = ZengaStrings.ToBoard("2/2/7/7/1/0/0/0/0 3 3");

        Assert.That(board, Is.Not.Null);
        Assert.That(board.heightIndex, Is.EqualTo(4));
        Assert.That(board.height, Is.EqualTo(3)); Assert.That(board.width, Is.EqualTo(3));
        Assert.That(board.maxHeight, Is.EqualTo(9));
        Assert.That(board.Tower, Is.EqualTo([2, 2, 7, 7, 1, 0, 0, 0, 0]));
    }
}