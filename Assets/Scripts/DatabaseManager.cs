using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System;

public class DatabaseManager : MonoBehaviour
{
    private const string dbUri = "https://fir-testi-cef89-default-rtdb.europe-west1.firebasedatabase.app/";
    private const string apiKey = "AIzaSyCSmrFI-QDhwv-rI8lsJiKReoIZlFDAksE";
    private const string authUri = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";
    private string token;

    // Muista luoda Unityn hierarchiaan GameObjecti, jossa tämä scripti on
    
    public static DatabaseManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        StartCoroutine(Authenticate());
    }

    // Funktio, jolla voi tuhota tietokannasta kokonaisen taulun.
    public void DeleteTable<T>() => StartCoroutine(_DeleteTable<T>());
    // Funktio, jolla haetaan tietokannasta kaikki tauluun liittyvä tieto.
    public void GetAllData<T>(List<T> dataList, Action onDataReceived = null)
    {
        StartCoroutine(_GetAllData(dataList, onDataReceived));
    }

    // Funktio, jota voi kutsua muista scripteistä lisäämään dataa tietokantaan.
    public void PostData<T>(T data, Action onDataPosted = null)
    {
        StartCoroutine(_PostData(data, onDataPosted));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataList">Lista, johon data kootaan</param>
    /// <param name="entries">Voit rajoittaa haettujen rivien määrää</param>
    /// <param name="onDataReceived">Funktio, joka suoritetaan kun data on onnistuneesti haettu.</param>
    /// <returns></returns>
    /// 

    private IEnumerator _DeleteTable<T>()
    {
        while (string.IsNullOrEmpty(token))
            yield return null;
        string uri = dbUri + typeof(T).ToString() + "/.json?auth=" + token;
        UnityWebRequest req = UnityWebRequest.Delete(uri);
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log(req.error);
        }
        else
        {
            Debug.Log($"Taulu nimeltä {typeof(T)} tuhottu.");
        }
    }
    private IEnumerator _GetAllData<T>(List<T> dataList, Action onDataReceived = null)
    {
        while (string.IsNullOrEmpty(token))
            yield return null;
        string uri = dbUri + typeof(T).ToString() + "/.json?auth=" + token;
        UnityWebRequest req = UnityWebRequest.Get(uri);
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        
        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log(req.error);
        }
        else
        {
            string result = req.downloadHandler.text;
            // Googlen Firebase antaa automaattisesti jokaiselle sinne lähetetylle tiedolle
            // uniikin avaimen. Haetaan tietoa avainten mukaan ja käännetään ne C# -luokaksi
            Dictionary<string, T> jsonData = JsonConvert.DeserializeObject<Dictionary<string, T>>(result);

            if (jsonData == null)
            {
                Debug.Log($"{typeof(T)} nimistä taulua ei löytynyt tietokannasta.");
            }
            else
            {
                foreach (var obj in jsonData)
                {
                    dataList.Add(obj.Value);
                }

                // Jos parametrinä on syötetty myös funktio, niin suoritetaan kyseinen funktio
                if (onDataReceived != null)
                {
                    onDataReceived();
                }
            }

        }
        req.Dispose();
    }

    private IEnumerator _PostData<T>(T data, Action onDataPosted = null)
    {
        // Niin kauan kun meillä ei ole authentikaatiota tietokannan käsittellyyn,
        // odotamme, että Authenticate() metodi on hakenut meille tokenin.
        while (string.IsNullOrEmpty(token))
            yield return null;
        // Rakennetaan osoite Firebasessa sijaitsevaan tietokantaan.
        string uri = dbUri + typeof(T).ToString() + "/.json?auth=" + token;
        // Muutetaan C# data, JSON dataksi
        string jsonData = JsonConvert.SerializeObject(data);

        // Luodaan pyyntö Firebasen serverille. 
        UnityWebRequest req = UnityWebRequest.Post(uri, jsonData);
        // Pyynnön pitää muuttaa tavuiksi (byte), jotta serveri sen ymmärtää.
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
        // Odotetaan niin kauan, kunnes serveri saa pyynnön suoritettua
        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log(req.error);
        }
        else
        {
            if (onDataPosted != null)
            {
                onDataPosted();
            }
        }
        req.Dispose();

    }

    // Google Firebasen authentikaatiosta löytyy lisää tietoa osoitteesta
    // https://firebase.google.com/docs/reference/rest/auth. Esimerkissämme
    // käytämme kohtaa Sign in anonymously
    private IEnumerator Authenticate(Action onAuthentication = null)
    {
        // Määritetään ns. Request Payload lähetettäväksi serverille.
        string postData = "{'returnSecureToken': 'true'}";
        // Luodaan uusi request serverille, ensimmäinen parametri on webbiosoite
        // mihin pyyntö lähetään. Toinen parametri on Request Payload.
        UnityWebRequest req = UnityWebRequest.Post(authUri + apiKey, postData);
        // Vaihetaan pyynnön muoto JSONiin, koska Googlen Firebase niin vaatii.
        req.SetRequestHeader("Content-Type", "application/json");
        // Vaihdetaan pyynnön muoto myös tavuihin, koska Googlen Firebase sitäkin vaatii.
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(postData));
        
        // Odotetaan, että pyyntömme käsitellään serverillä.
        yield return req.SendWebRequest();
        // Käsitellään takaisin tullut data. Virheen sattuessa, näytetään
        // virhe Unityn konsolissa.
        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log(req.error);
        }
        else
        {
            // Haetaan data, joka palautuu tekstinä.
            string result = req.downloadHandler.text;
            // Muutetaan tekstidata JSON muotoon
            Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            // Asetetaan token -muuttujan arvoksi, JSON muodossa olevasta datasta löytyvästä idToken arvosta.
            token = data["idToken"];
            if (onAuthentication != null)
            {
                onAuthentication();
            }
        }

        req.Dispose();
    }
}
