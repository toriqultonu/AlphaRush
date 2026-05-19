using System.Collections.Generic;
using UnityEngine;

// Stub. Real impl in §13 will instantiate WordChipView children, color-fill on MarkFound, etc.
public class WordListView : MonoBehaviour {
    readonly HashSet<string> found = new();

    public virtual void SetWords(IList<string> words) { /* TODO: spawn chips */ }

    public virtual void MarkFound(string word, Color color) {
        found.Add(word);
        // TODO: locate chip for `word`, animate color + scale.
    }

    public virtual void Clear() {
        found.Clear();
        // TODO: destroy chip children.
    }
}
