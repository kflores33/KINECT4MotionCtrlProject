using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI NameLabel;
    public TextMeshProUGUI ScoreLabel;
    public TextMeshProUGUI rankLabel;

    public void Setup(Ranking.NameAndScore s, int rank)
    {
        rankLabel.text = $"{rank}.";
        NameLabel.text = s.Name;
        ScoreLabel.text = $"{s.Score:N0}";
    }

}
