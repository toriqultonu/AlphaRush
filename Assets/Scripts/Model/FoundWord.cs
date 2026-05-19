[System.Serializable]
public class FoundWord {
    public string word;
    public int startRow, startCol, endRow, endCol;
    public long colorPacked; // store as 0xAARRGGBB
}
