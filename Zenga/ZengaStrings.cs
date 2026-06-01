namespace Zenga;

// lay1(decimal)/lay2/lay3/.../layn start_width start_height side

public partial class ZengaStrings
{
    public static string BoardToString(Board board)
    {
        string[] fields = new string[3];

        fields[0] = string.Join('/', board.Tower);

        fields[1] = board.Height.ToString();
        fields[2] = board.Width.ToString();

        return string.Join(' ', fields);
    }
}

public partial class ZengaStrings
{
    public static Board? StringToBoard(string strBoard)
    {
        var splitFields = strBoard.Split(' ');

        if (splitFields.Length != 3) return null;
        
        if (!ValidDimention(splitFields[1], out int height)) return null;
        if (!ValidDimention(splitFields[2], out int width)) return null;
        
        Board board = new(height, width);
        
        var splitLayers = splitFields[0].Split('/');
        if (!ValidLayers(splitLayers, board)) return null;

        board.heightIndex = board.GetHeightIndex();

        return board;
    }

    private static bool ValidDimention(string strDimSize, out int dimSize)
    {
        dimSize = 0;
        if (!int.TryParse(strDimSize, out int result)) return false;

        if (result < 0) return false;

        dimSize = result;
        return true;
    }

    private static bool ValidLayers(string[] strLayers, Board board)
    {
        byte maxBlock = (byte)((1 << board.Width) - 1);

        if (strLayers.Length > board.maxHeight) return false;
        
        for (int i = 0; i < strLayers.Length; i++)
        {
            if (!byte.TryParse(strLayers[i], out byte result)) return false;
            if (result > maxBlock) return false;

            board.Tower[i] = result;
        }
        
        return true;
    }
}