using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class HighscoreManager : MonoBehaviour
{
    private List<HighScore> highscores = new List<HighScore>();
    [SerializeField]
    private Transform addScorePanel, showScorePanel, scorePrefab;
    [SerializeField]
    private InputField nameField, scoreField;
    private float dbUpdateInterval = 10f, dbLastUpdated;

    public void Start()
    {
        DatabaseManager.Instance.GetAllData<HighScore>(highscores, EsimerkkiHakuFunktiot);
    }

    public void EsimerkkiHakuFunktiot()
    {
        // Tällä komennolla etsitään listasta käyttäjä, jonka nimi on Mikko.
        HighScore entry = highscores.Where(o => o.name == "Mikko").FirstOrDefault();
        Debug.Log($"{entry.name}, {entry.score}, {entry.date}");

        // Tällä komennolla etsitään kaikki käyttäjät, jonka pistemäärä on yli tuhat;
        List<HighScore> betterthanthousand = highscores.FindAll(o => o.score > 1000);
        foreach (HighScore a in betterthanthousand)
        {
            Debug.Log($"{a.name}, {a.score}, {a.date}");
        }
    }

    public void AddScore()
    {
        showScorePanel.gameObject.SetActive(false);
        addScorePanel.gameObject.SetActive(true);
    }

    public void SubmitScore()
    {
        HighScore newScore = new HighScore()
        {
            name = nameField.text,
            score = int.Parse(scoreField.text),
            date = DateTime.Now.ToString()
        };
        DatabaseManager.Instance.PostData<HighScore>(newScore);
        nameField.text = "";
        scoreField.text = "";
    }

    public void ShowScores()
    {
        addScorePanel.gameObject.SetActive(false);
        RectTransform[] rects = showScorePanel.GetComponentsInChildren<RectTransform>();
        for (int i = 1; i < rects.Length; i++ )
        {
            Destroy(rects[i].gameObject);
        }
        // Haetaan kaikki highscore tietueet tietokannasta, ja järjestään
        // ne System.Linq kirjastosta löytyvällä komennolla isoimmasta
        // pistemäärästä pienimpään


        showScorePanel.gameObject.SetActive(true);

        // Highscores lista päivitetään tietokannasta aina vain, jos edellisestä
        // päivityksestä on kulunut dbUpdateIntervalin osoittama aika,
        // joka on oletuksena 10 sekuntia.
        if (highscores == null || Time.time > dbLastUpdated)
        {
            dbLastUpdated = Time.time + dbUpdateInterval;
            // MUISTA AINA ALUSTAA LISTA, JOHON HALUAT HAKEA DATAN
            highscores = new List<HighScore>();
            DatabaseManager.Instance.GetAllData<HighScore>(highscores, ShowScoreList);
        }
        else
        {
            ShowScoreList();
        }
    }

    public void ShowScoreList()
    {
        // Järjestetään highscores lista parhaimmasta alhaisimpaan.
        highscores = highscores.OrderByDescending(o => o.score).ToList();

        for (int i = 0; i < highscores.Count; i++)
        {
            Transform score = Instantiate(scorePrefab, showScorePanel);
            Text[] texts = score.GetComponentsInChildren<Text>();
            texts[0].text = $"{i + 1}.";
            texts[1].text = $"{highscores[i].name}";
            texts[2].text = $"{highscores[i].score}";
            texts[3].text = $"{highscores[i].date}";
            score.position = new Vector2(showScorePanel.position.x,
                                         showScorePanel.position.y - i * 50);
        }
    }
}

public class HighScore
{
    public string name;
    public int score;
    public string date;
}
