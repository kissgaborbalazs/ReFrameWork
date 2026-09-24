# 6. Tesztelés

[← Tartalom](README.md)

## 6.1 Miért tesztelünk?

Egy robot hibája élesben valódi üzleti kárt okoz: kimaradt vagy duplán rögzített számlát, rossz adatot egy másik rendszerben. A tesztelés célja, hogy **minden kimenetelt** (Success, Business, System, Main) legalább egyszer lássunk működni, mielőtt élesbe kerül.

## 6.2 A meglévő tesztesetek (`5_Test/Main/`)

| Teszteset | Queue item Reference | Várt eredmény |
|---|---|---|
| `Main_Successfull.xaml` | `Successful_Test` | Successful státusz |
| `Main_BusinessException.xaml` | `BusinessException_Test` | Failed – Business, Reason: `[PRJ-BE-001] …` |
| `Main_SystemException.xaml` | `SystemException_Test` | Failed – Application, Reason: `[PRJ-SYS-001] …`, újrapróbálás |
| `Main_MainException.xaml` | `MainException_Test` | Saját Config: `5_Test/Main/Config_MainException.xlsx` (`MaxConsecutiveSystemExceptions = 1`). A tétel rendszerhibát dob, az Initialize állapot `FW-SYS-001`-gyel leáll → Main exception, amit a teszt elkap |

Mindegyik **Given – When – Then** szerkezetű:
- **Given:** a queue kiürítése, és egy teszt tétel hozzáadása.
- **When:** a `Main.xaml` meghívása.
- **Then:** az eredmény ellenőrzése.

> ⚠️ **Ismert hiányosság:** a meglévő tesztek **Then** ága jelenleg üres, vagyis nem ellenőriznek semmit, csak lefuttatják a folyamatot. A teszteket is a `Processes/Teszt` mappára és a `Teszt_queue` queue-ra égették be. Új tesztnél ezt már ne kövesd (lásd 6.4).

A tesztek a `Process.xaml` teszt ágaira építenek (a Reference alapján dobnak hibát). **Élesítés előtt a teszt ágakat a Process.xaml-ből törölni kell**, a teszteseteket pedig ennek megfelelően átírni vagy kivezetni.

## 6.3 Mit tesztelj egy új folyamatnál?

| Szint | Mit? | Hogyan? |
|---|---|---|
| **Unit** | Egy üzleti szabály, számítás, validáció (`3_Business_Logic`) | Coded test case / Test Case workflow közvetlen bemenettel, **queue és UI nélkül** |
| **Komponens** | Egy alkalmazás-művelet (`2_Application_Layer`) | Test Case teszt környezetben, ismert adattal |
| **Integrációs** | A teljes `Main.xaml` egy tétellel | `5_Test/Main` mintájára |
| **Hibaágak** | Minden hibakód, amit a kód dobhat | Olyan bemenet, ami kiváltja, **Then: ellenőrizd a Reason-t** |

## 6.4 Best practice-ek

- ✅ **A Then ágban mindig legyen ellenőrzés** (Verify Expression / Verify Expression With Operator). A teszt, ami nem ellenőriz semmit, csak lefut, és hamis biztonságérzetet ad.

```csharp
// Then – példa: a tétel státuszának és hibakódjának ellenőrzése
// (a tételt Get Queue Items-szel kérd le a Reference alapján)
queueItem.Status == QueueItemStatus.Failed
queueItem.ProcessingException.Reason.StartsWith("[PRJ-BE-001]")
```

- ✅ Minden **hibakódhoz** legyen legalább egy teszteset.
- ✅ A tesztadat legyen **determinisztikus**: fix, ismert bemenet. Ne a „mai” adatokon tesztelj.
- ✅ A teszt ne függjön egy másik teszttől. Mindegyik a saját Given ágában állítsa elő, amire szüksége van.
- ✅ A teszt a mappát és a queue nevét is **konfigurációból** vegye, ne beégetve.
- ✅ Ha egy teszt saját Config fájlt használ (mint a `Config_MainException.xlsx`), **új Config kulcsnál azt is frissítsd.** Különben a teszt rossz ponton, rossz okból bukik el (vagy megy át).
- ✅ Teszteld a **határeseteket**: üres mező, nagyon hosszú szöveg, speciális karakterek (ő, ű, &, <), 0 és negatív szám, dátumformátumok.
- ✅ Teszteld a **retry** működését: `QueueMaxRetryNumber` szerint az utolsó kísérletnél megy-e ki az e-mail.
- ❌ Ne tesztelj éles rendszeren éles adattal.

## 6.5 A hibakezelés ellenőrzése (manuális checklist)

- [ ] Business hibánál a Reason `[KÓD] üzenet`, a Details a javítási javaslat.
- [ ] System hibánál az újrapróbálás megtörténik, és az e-mail csak az utolsó kísérletnél megy ki.
- [ ] Az Excel riportban a jelszó változók értéke `***`.
- [ ] Ha a levelezőszerver elérhetetlen (pl. rossz cím a Configban), a tétel státusza ettől még helyes (nem Successful).
- [ ] Hibás `ErrorCodes` lap (pl. duplikált kód) esetén a robot induláskor `FW-CFG-001`-gyel leáll.
- [ ] Rossz `in_ConfigFilePath` esetén a robot `FW-CFG-002`-vel leáll, és az `in_FallbackAlertEmail` címre e-mail megy.
- [ ] `MaxConsecutiveSystemExceptions` számú egymás utáni rendszerhiba után a robot `FW-SYS-001`-gyel leáll.

## Összefoglaló

- Minden kimenetelt és minden hibakódot tesztelj.
- A Then ág ellenőrizzen, ne csak futtasson.
- Üzleti szabályt UI és queue nélkül, unit szinten tesztelj.
