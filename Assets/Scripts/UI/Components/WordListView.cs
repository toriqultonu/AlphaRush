using System.Collections.Generic;
using UnityEngine;

// Spawns one WordChipView per target word into `container`; MarkFound relays
// to the matching chip (strikethrough + tint per §14.2).
public class WordListView : MonoBehaviour {
    [SerializeField] Transform container;
    [SerializeField] WordChipView chipPrefab;

    readonly List<WordChipView> chips = new();

    public virtual void SetWords(IList<string> words) {
        Clear();
        if (container == null || chipPrefab == null || words == null) return;
        for (int i = 0; i < words.Count; i++) {
            var chip = Instantiate(chipPrefab, container);
            chip.Bind(words[i], AppColors.ChipColors[i % AppColors.ChipColors.Length]);
            chips.Add(chip);
        }
    }

    public virtual void MarkFound(string word, Color color) {
        var chip = chips.Find(c => c != null && c.Word == word);
        if (chip != null) chip.MarkFound(color);
    }

    public virtual void Clear() {
        foreach (var c in chips) if (c != null) Destroy(c.gameObject);
        chips.Clear();
    }
}
