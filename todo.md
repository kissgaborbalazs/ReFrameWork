# ReFrameWork – Hibák és fejlesztési javaslatok

> Prioritások: 🔴 Kritikus | 🟡 Közepes | 🟢 Alacsony

---

## 🔴 Kritikus hibák

### 1. `Process.xaml` – Teszt kód éles kódbázisban van
**Fájl:** `Process.xaml`  
**Probléma:** Az egész folyamat tényleges üzleti logikája egy teszt `IfElseIf` blokkon belül van (`Else` ágban). A blokk felső részén lévő feltételek (`"BusinessException_Test"`, `"SystemException_Test"`, `"MainException_Test"` referenciák) véletlenül valódi queue item által is aktiválhatók.  
**Magyar annotáció is jelzi:** `"Teszt szekvencia. Business és System exception tesztelésre. Az éles kódból törlendő!!!"`  
**Javítás:** A teszt `IfElseIf` blokkot teljes egészében el kell távolítani. A tényleges üzleti logikát (e-mail küldés) a közvetlen szekvenciába kell áthelyezni.

---

### 2. `Process.xaml` – Hardkódolt e-mail cím és küldő neve
**Fájl:** `Process.xaml`  
**Probléma:** Az e-mail küldésnél két érték hardkódolva van:
- `in_SendTo`: `"RPA_Developers@posta.hu"` 
- `in_From`: `"Template teszt"`  

**Javítás:** Mindkét értéket a `Config.xlsx`-ből kell olvasni (pl. `Config["NotificationEmail"]`, `Config["EmailSenderName"]`).

---

### 3. `0_Framework/CloseAllApplications.xaml` – Üres Try blokk (nem működő kód)
**Fájl:** `0_Framework/CloseAllApplications.xaml`  
**Probléma:** A TryCatch aktivitás `Try` ága üres – nem tartalmaz egyetlen aktivitást sem. Az alkalmazások bezárása nem történik meg, a workflow csak loggol és továbblép.  
**Javítás:** A Try blokkot fel kell tölteni a konkrét alkalmazásbezáró logikával.

---

### 4. `0_Framework/CheckWorkingHours.xaml` – Elírás a változónévben (futásidejű hiba)
**Fájl:** `0_Framework/CheckWorkingHours.xaml`  
**Probléma:** A `NonWorkinHours` változónév szerepel a kódban `NonWorkingHours` helyett (hiányzik a `g` betű). Ez futásidejű `NullReferenceException`-t okozhat, ha a változóra hivatkoznak.  
**Javítás:** Változónév javítása `NonWorkingHours`-ra, és az összes referencia frissítése.

---

### 5. `0_Framework/CheckWorkingHours.xaml` – Éjfelet átlépő időszak nem kezelt
**Fájl:** `0_Framework/CheckWorkingHours.xaml`  
**Probléma:** Az időszak-összehasonlítás `DateTime.Today.Add(start) <= now <= DateTime.Today.Add(end)` logikán alapul. Ha az időszak éjfelet lép át (pl. `22:00-02:00`), az `end` kisebb lesz, mint a `start`, és az összehasonlítás mindig `False`-t ad.  
**Javítás:** Ha `end < start`, a logikát `now >= start || now <= end` alakban kell kezelni.

---

### 6. `0_Framework/CreateEmailBody.xaml` – Útvonal felülíródik validáció esetén
**Fájl:** `0_Framework/CreateEmailBody.xaml`  
**Probléma:** A `GuardFileExists` aktivitás kimenete (az alapértelmezett elérési út) felülírja a `htmlTemplatePath` változót, így a hívó által megadott egyedi sablon elérési útja elvész, ha az alapértelmezett sablonra esik vissza.  
**Javítás:** Az értékadás logikáját javítani kell: csak akkor kell visszaesni az alapértelmezésre, ha a megadott útvonal üres vagy null.

---

### 7. `0_Framework/SetTransactionStatus.xaml` – Hardkódolt Orchestrator mappa
**Fájl:** `0_Framework/SetTransactionStatus.xaml`  
**Probléma:** A `SetTransactionStatus` aktivitás `FolderPath` paramétere hardkódolva tartalmazza a `"Processes/Teszt"` értéket. Ha az Orchestrator mappája eltér, a queue státusz frissítése sikertelen lesz.  
**Javítás:** A mappa elérési útját a `Config.xlsx`-ből kell olvasni (pl. `Config["OrchestratorFolder"]`).

---

### 8. `0_Framework/CreateQueueItem.xaml` – Hardkódolt queue név és mappa
**Fájl:** `0_Framework/CreateQueueItem.xaml`  
**Probléma:** A queue neve (`"Teszt_queue"`), az Orchestrator mappa (`"Processes/Teszt"`) és az item referencia prefix (`"Teszt_item_"`) mind hardkódoltak.  
**Javítás:** Mindhárom értéket konfigurációból kell olvasni (`Config["QueueName"]`, `Config["OrchestratorFolder"]`).

---

## 🟡 Közepes súlyosságú problémák

### 9. `Templates/ErrorMailTemplate.html` – HTML injection lehetőség
**Fájl:** `Templates/ErrorMailTemplate.html`  
**Probléma:** Az `{exceptionMessage}` és `{stackTrace}` placeholderek tartalma közvetlenül kerül a HTML-be, HTML-escape nélkül. Ha a kivétel üzenete HTML vagy JavaScript kódot tartalmaz, az megjelenik (és lefuthat) az e-mailben.  
**Javítás:** Az értékeket HTML-encode-olni kell behelyettesítés előtt, vagy a `<pre>` tageket `innerText`-re cserélni.

---

### 10. Elírások fájlnevekben és kódban
**Érintett fájlok/helyek:**

| Hely | Jelenlegi | Helyes |
|------|-----------|--------|
| `0_Framework/InitAllAplications.xaml` | `InitAllAplications` | `InitAllApplications` |
| `0_Framework/BussinesExceptionHandling.xaml` | `BussinesException` | `BusinessException` |
| `MainMachine.xaml` – AnalyticsData kulcs | `"Initilaizing time"` | `"Initializing time"` |
| `MainMachine.xaml` – LogMessage | `"Starting the initial bot preb"` | `"Starting the initial bot prep"` |
| `1_GEH/BuildErrorDictionary.cs:32` | `exceptionType + " exception" ?? "N/A"` | `"N/A"` (a `?? "N/A"` soha nem fut le) |

> **Megjegyzés:** A fájlnév-elírásokat óvatosan kell javítani – az összes hivatkozást frissíteni kell (`project.json`, `MainMachine.xaml`, `Main.xaml`).

---

### 11. `0_Framework/InitAllSettings.xaml` – Validáció kikommentezve
**Fájl:** `0_Framework/InitAllSettings.xaml`  
**Probléma:** A `Config.xlsx` elérési útjának null/üres ellenőrzése `CommentOut` blokkban van, így soha nem fut le. Ha az argumentum üres, a workflow értelmezhetetlen hibával fog összeomlani.  
**Javítás:** A validációs kódot aktiválni kell (CommentOut eltávolítása), és értelmes hibaüzenetet kell hozzá írni.

---

### 12. `0_Framework/KillAllProcesses.xaml` – Hiányzó null ellenőrzés
**Fájl:** `0_Framework/KillAllProcesses.xaml`  
**Probléma:** A `GlobalVariables.Config["ProcessesToKill"]` lekérdezésnél nincs null ellenőrzés. Ha a konfig kulcs hiányzik vagy null értékű, `NullReferenceException` keletkezik.  
**Javítás:**
```csharp
// Mielőtt .ToString().Split() hívás:
if (!GlobalVariables.Config.ContainsKey("ProcessesToKill") || GlobalVariables.Config["ProcessesToKill"] == null)
    return;
```

---

### 13. `0_Framework/TakeScreenshot.xaml` – Néma hiba az összes screenshot készítésekor
**Fájl:** `0_Framework/TakeScreenshot.xaml`  
**Probléma:** A Catch blokkon belüli desktop screenshot is `ContinueOnError="True"` beállítással fut. Ha mindkét screenshot készítés meghiúsul (pl. érvénytelen elérési út), a hívó workflow nem értesül a hibáról.  
**Javítás:** Legalább log (Error szintű) bejegyzést kell hozzáadni, ha a fallback desktop screenshot is sikertelen.

---

### 14. `1_GEH/GEH_Process.xaml` – Túl nagy fájl (2794 sor)
**Fájl:** `1_GEH/GEH_Process.xaml`  
**Probléma:** A fájl egyidejűleg felelős a screenshot készítéséért, az Excel riport generálásáért, az e-mail küldéséért és a tranzakció státuszáért. Ez Single Responsibility Principle (SRP) megsértése, nehézkes karbantartással.  
**Javítás:** A fő lépéseket külön workflow-okra bontani, amelyeket a `GEH_Process.xaml` sorban meghív. Hasonlóan a `GEH_CreateReport.xaml` is 2975 sorral túl nagy.

---

### 15. `1_GEH/BuildErrorDictionary.cs` és `.xaml` – Duplikált implementáció
**Fájl:** `1_GEH/BuildErrorDictionary.cs`, `1_GEH/BuildErrorDictionary.xaml`  
**Probléma:** Ugyanaz a logika két helyen van implementálva: egyszer XAML `InvokeCode` blokkban, egyszer C# osztályban. A kettő szinkronban tartása karbantartási terhet jelent.  
**Javítás:** Csak a C# implementációt kell megtartani; a XAML `InvokeCode` blokkot törölni, és az osztályt meghívni.

---

### 16. `0_Framework/SendEmail.xaml` – Nincs hibakezelés az e-mail küldésnél
**Fájl:** `0_Framework/SendEmail.xaml`  
**Probléma:** A `SendEmail` aktivitás nem Try-Catch blokkban fut. Ha az e-mail szerver elérhetetlen, a kivétel a hívó workflow-ig propagálódik, ami félrevezető hibaüzenetet produkál.  
**Javítás:** TryCatch hozzáadása; e-mail küldési hiba esetén logolni (Warning szinten), de nem szabad a folyamatot leállítani.

---

### 17. `MainMachine.xaml` – Nincs átmenet Initialize → End Process hibára
**Fájl:** `MainMachine.xaml`  
**Probléma:** Ha az Initialize állapotban kivétel keletkezik (pl. `InitAllApplications` meghibásodik), az kivételt dob, ami a `Main.xaml` külső TryCatch blokkjára propagálódik. Ez megfelelően kezelt, de az állapotgép belső logikájában nem dokumentált útvonal.  
**Javítás:** Az Initialize állapotban is legyen TryCatch, vagy dokumentálni kell a tervezett viselkedést kommentben.

---

## 🟢 Alacsony prioritású fejlesztési javaslatok

### 18. `Templates/BusinessExceptionEmailTemplate.html` – Üres sablon
**Fájl:** `Templates/BusinessExceptionEmailTemplate.html`  
**Probléma:** A fájl csak `<head>` stílusokat tartalmaz, `<body>` üres. Üzleti kivétel esetén nem küldődik informatív e-mail.  
**Javítás:** Implementálni a sablont az `ErrorMailTemplate.html` mintájára, vagy törölni és az `ErrorMailTemplate.html`-t használni üzleti kivételekre is.

---

### 19. Vegyes magyar–angol kód és kommentek
**Érintett fájlok:** Szinte minden workflow  
**Probléma:** A kódban, logüzenetekben és kommentekben keveredik a magyar és az angol nyelv. Ez megnehezíti a kód olvashatóságát és a csapatmunkát.  
**Javítás:** Egységes nyelvet kell választani (javasolt: angol) minden log üzenethez és kommenthez. A Config.xlsx-ben és e-mail sablonokban a magyar maradhat, ha az ügyfél számára szól.

---

### 20. `0_Framework/KillAllProcesses.xaml` – Kikommentezett szemétgyűjtő kód
**Fájl:** `0_Framework/KillAllProcesses.xaml`  
**Probléma:** A fájlban kikommentezett `GC.Collect()` / `GC.WaitForPendingFinalizers()` hívások találhatók. Ezeket nem szabad RPA workflow-ban manuálisan hívni, és a jelenlétük félrevezető.  
**Javítás:** A kikommentezett blokkot törölni kell.

---

### 21. `0_Framework/SendEmail.xaml` – Kikommentezett logolás
**Fájl:** `0_Framework/SendEmail.xaml`  
**Probléma:** A sikeres e-mail küldés utáni válasz logolása `CommentOut` blokkban van, így nem fut le. Az e-mail Guid és válasz státusz debugging szempontból hasznos lenne.  
**Javítás:** A logolást aktiválni `Debug` szinttel, feltétel: `GlobalVariables.Config["Debug"] == true`.

---

### 22. `0_Framework/InitAllAplications.xaml` és `CloseAllApplications.xaml` – Üres implementációk
**Fájlok:** `0_Framework/InitAllAplications.xaml`, `0_Framework/CloseAllApplications.xaml`  
**Probléma:** Mindkét workflow csak belépési/kilépési logot tartalmaz. Sablon jellegű fájlok, amelyek implementációra várnak.  
**Javítás:** Az adott folyamathoz szükséges alkalmazásnyitó és -záró logikát implementálni kell. Javasolt: az alkalmazáslistát konfigurációból olvasni.

---

### 23. `0_Framework/BussinesExceptionHandling.xaml` és `SystemExceptionHandling.xaml` – Üres implementációk
**Fájlok:** `0_Framework/BussinesExceptionHandling.xaml`, `0_Framework/SystemExceptionHandling.xaml`  
**Probléma:** Mindkét workflow csak logot tartalmaz. A sablon komment (`// Replace this block with workflow logic`) jelzi, hogy fejlesztés szükséges.  
**Javítás:** Üzleti és rendszer kivételekre specifikus kezelési logika implementálása (pl. értesítés a business teamenek, screenshot mentés, retry logika).

---

### 24. Hiányzó dokumentáció a `Config.xlsx` sémájához
**Probléma:** Nincs dokumentálva, hogy a `Config.xlsx` pontosan milyen lapokat, oszlopokat és kötelező kulcsokat tartalmaz. A fejlesztők csak a kódból tudják kikövetkeztetni.  
**Javítás:** A `Config.xlsx`-et egy második lappal (pl. `Documentation`) kell kiegészíteni, amely leírja az összes kulcsot, típust és példaértéket.

---

### 25. `0_Framework/CheckWorkingHours.xaml` – Határidős eset kezelése
**Fájl:** `0_Framework/CheckWorkingHours.xaml`  
**Probléma:** A jelenlegi összehasonlítás `<=` (inclusive) végidőpontig – ha pontosan a végidőpontban fut a robot, az blokkolt időszakba esik. Nem dokumentált, hogy ez szándékos-e.  
**Javítás:** Dokumentálni a szándékot, vagy `<` operátorra váltani, ha az exkluzív határt kell alkalmazni.

---

## Összefoglaló

| Prioritás | Darab |
|-----------|-------|
| 🔴 Kritikus | 8 |
| 🟡 Közepes | 9 |
| 🟢 Alacsony | 8 |
| **Összesen** | **25** |
