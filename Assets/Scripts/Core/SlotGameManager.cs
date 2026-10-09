using System.Collections;
using UnityEngine;

public class SlotGameManager : MonoBehaviour {
    [SerializeField] private SlotReel[] reels;
    [SerializeField] private SlotRNG rng;

    private bool isPlaying;

    public void Spin() {
        if (isPlaying) return;

        StartCoroutine(SpinRoutine);
    }

    private IEnumerator SpinRoutine() {
        isPlaying = true;

        SymbolData[] result = rng.GenerateResult(reels.Length);

        isPlaying = false;
    }
}
