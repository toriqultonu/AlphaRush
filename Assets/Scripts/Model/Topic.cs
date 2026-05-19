using System.Collections.Generic;

[System.Serializable]
public class Topic {
    public string id;
    public string name;
    public string icon;           // emoji
    public long accentColor;      // 0xAARRGGBB
    public List<string> wordPool;
    public int unlockStarsRequired;
}
