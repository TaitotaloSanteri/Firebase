# Firabase
Googlen Firebase tietokantapalvelun käyttö Unityn kanssa. 

Kopioi Assets/Plugins/ kansiosta löytyvä .DLL tiedosto oman projektisi vastaavaan kansioon.
Kopioi DatabaseManager.cs tiedosto omaan projektiisi Assets kansioon

Luo projektiisi uusi DataBaseManager -gameobject, johon liität DataBaseManager.cs tiedoston

Googlen Firebase on ns. NoSQL -tietokanta, joka toimii JSON-struktuurilla. DatabaseManager.cs
tiedostosta löytyy käytännössä kaksi käskyä, joita käyttämällä tietokantaa voidaan käyttää
**Lisää taulukkoon tietoa:**

`DataManager.Instance.PostData<OmaLuokka>(data);`
(data -parametri on tässä tapauksess OmaLuokka -kuuluva objekti

**Hae taulukosta kaikki tiedot**:

`DataManager.Instance.GetAllData<OmaLuokka>(dataList)
(dataList -parametri on tässä tapauksessa C# -List tyyppinen muuttuja)

Korvaa **OmaLuokka** sana yllä olevissa esimerkeissä omalla luokallasi, esim:

`DataManager.Instance.PostData<HighScore>(data);`

```
public class HighScore{
  public string name; 
  public int score; 
  public string date; 
}
