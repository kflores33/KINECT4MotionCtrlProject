using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Ranking : MonoBehaviour
{
    public static Ranking instance;

    public int MaxScoreCount = 10;
    [System.Serializable]
    public struct NameAndScore
    {
        public int Score;
        public string Name;
    }
    private List<NameAndScore> _topScores = new List<NameAndScore>();
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }


    public void Start()
    {
        // load the scores!
        string scoreFilePath = Path.Combine(Application.persistentDataPath, "scores.txt");

        if (!File.Exists(scoreFilePath))
        {
            File.WriteAllText(scoreFilePath, "");
        }

        string[] fileLines = File.ReadAllLines(scoreFilePath);

        for (int i = 0; i < fileLines.Length; i += 2)
        {
            if (i + 1 >= fileLines.Length)
                break;

            NameAndScore theScore = new NameAndScore();
            theScore.Name = fileLines[i];
            bool didParse = int.TryParse(fileLines[i + 1], out int parsedScore);
            if (didParse)
            {
                theScore.Score = parsedScore;

                _topScores.Add(theScore);
            }
        }
        // sort the scores just to be sure.
        _topScores.Sort(_compareScores);
    }
    private int _compareScores(NameAndScore a, NameAndScore b)
    {
        return b.Score.CompareTo(a.Score);
    }
    public int AcceptNewScore(NameAndScore newScore)
    {
        // where does this score fit in our list?
        int newPlace = -1;
        Debug.Log("Svae?");

        for (int i = 0; i < _topScores.Count; i++)
        {
            NameAndScore thisScore = _topScores[i];
            if (newScore.Score > thisScore.Score)
            {
                // THEY MADE IT, WOW!
                // let's insert them into the list
                _topScores.Insert(i, newScore);
                newPlace = i;

                // did this now make the list have more than the maximum score count?
                if (_topScores.Count > MaxScoreCount)
                {
                    _topScores.RemoveAt(_topScores.Count - 1); //remove bottom score
                }
                break;
            }
        }
        // catch the scenario where we didn't surpass any score, BUT
        // there is room for us in the list!
        if (newPlace < 0 && _topScores.Count < MaxScoreCount)
        {
            _topScores.Add(newScore);
            newPlace = _topScores.Count - 1;
        }
        return newPlace;
    }
    public int GetScoreCount()
    {
        return _topScores.Count;
    }
    public NameAndScore GetScoreAt(int idx)
    {
        return _topScores[idx];
    }
    public void SaveScores()
    {
        string fileContent = "";

        for (int i = 0; i < _topScores.Count; i++)
        {
            NameAndScore thisScore = _topScores[i];

            fileContent += $"{thisScore.Name}\n{thisScore.Score}\n";
        }

        string scoreFilePath = Path.Combine(Application.persistentDataPath, "scores.txt");
        Debug.Log($"we're gonna save the scores to: {scoreFilePath}");

        File.WriteAllText(scoreFilePath, fileContent);
    }
    public int GetMyRank(int score)
    {
        for (int i = 0; i < _topScores.Count; i++)
        {
            if (score > _topScores[i].Score)
            {
                return i; //this should be the position that the player will be at
            }
        }

        // add me to the botom of the list if the list isnt full yet
        if (_topScores.Count < MaxScoreCount)
        {
            return _topScores.Count;
        }

        return -1; //if i didnt get a high score at all and the lsit is full, give back a -1
    }
}
