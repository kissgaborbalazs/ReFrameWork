# ReFrameWork – Hibák és fejlesztési javaslatok

> Prioritások: 🔴 Kritikus | 🟡 Közepes | 🟢 Alacsony  
> A lista csak a **keretrendszer tényleges hibáit** tartalmazza. A sablon jellegű, szándékosan üres workflow-ok (`InitAllApplications`, `CloseAllApplications`, `BussinesExceptionHandling`, `SystemExceptionHandling`, `Process.xaml` üzleti logika stb.) nem szerepelnek itt – ezek kitöltése a fejlesztő feladata.

---

## 🔴 Kritikus hibák

### 1. `0_Framework/CheckWorkingHours.xaml` – Elírás a változónévben (futásidejű hiba)
**Fájl:** `0_Framework/CheckWorkingHours.xaml`  
**Probléma:** A `NonWorkinHours` változónév szerepel a kódban `NonWorkingHours` helyett (hiányzik a `g` betű). Ez futásidejű `NullReferenceException`-t okozhat, ha a változóra hivatkoznak.  
**Javítás:** Változónév javítása `NonWorkingHours`-ra, és az összes referencia frissítése a fájlon belül.

---

### 2. `0_Framework/CheckWorkingHours.xaml` – Éjfelet átlépő időszak nem kezelt
**Fájl:** `0_Framework/CheckWorkingHours.xaml`  
**Probléma:** Az időszak-összehasonlítás `DateTime.Today.Add(start) <= now <= DateTime.Today.Add(end)` logikán alapul. Ha az időszak éjfelet lép át (pl. `22:00-02:00`), az `end` kisebb lesz, mint a `start`, és az összehasonlítás mindig `False`-t ad – a robot éjfélkor újra elindul.  
**Javítás:** Ha `end < start`, a logikát `now >= start || now <= end` alakban kell kezelni.

---

### 3. `0_Framework/CreateEmailBody.xaml` – Megadott sablon útvonal felülíródik
**Fájl:** `0_Framework/CreateEmailBody.xaml`  
**Probléma:** A `GuardFileExists` aktivitás kimenete az alapértelmezett elérési utat rendeli hozzá a `htmlTemplatePath` változóhoz, így a hívó által átadott egyedi sablonútvonal elvész – a workflow mindig az alapértelmezett sablont fogja használni.  
**Javítás:** Az értékadás logikáját javítani kell: csak akkor kell visszaesni az alapértelmezésre, ha az `in_HtmlTemplatePath` argumentum üres vagy null.

---

### 4. `MainMachine.xaml` – AnalyticsData kulcs elírása (KeyNotFoundException)
**Fájl:** `MainMachine.xaml`, `0_Framework/SetTransactionStatus.xaml`  
**Probléma:** A `MainMachine.xaml`-ben az `AnalyticsData` szótárba `"Initilaizing time"` kulccsal kerül az adat, a `SetTransactionStatus.xaml` pedig ugyanígy `"Initilaizing time"`-ot olvas. Ha bármelyik helyen javításra kerül a kulcsnév, de a másik helyen nem, `KeyNotFoundException` keletkezik futásidőben.  
**Javítás:** A kulcsnevet mindkét helyen egységesen `"Initializing time"`-ra kell javítani.

---

## 🟡 Közepes súlyosságú problémák

### 5. `0_Framework/SetTransactionStatus.xaml` – Hardkódolt Orchestrator mappa
**Fájl:** `0_Framework/SetTransactionStatus.xaml`  
**Probléma:** A `SetTransactionStatus` aktivitás `FolderPath` paramétere hardkódolva tartalmazza a `"Processes/Teszt"` értéket. Ha az Orchestrator mappája eltér, a queue státusz frissítése hibás lesz.  
**Javítás:** A mappa elérési útját a `Config.xlsx`-ből kell olvasni (pl. `Config["OrchestratorFolder"]`).

---

### 6. `Templates/ErrorMailTemplate.html` – HTML injection lehetőség
**Fájl:** `Templates/ErrorMailTemplate.html`  
**Probléma:** Az `{exceptionMessage}` és `{stackTrace}` placeholderek tartalma közvetlenül kerül a HTML-be, HTML-escape nélkül. Ha a kivétel üzenete HTML-t vagy JavaScript-et tartalmaz, az az e-mailben megjelenik és lefuthat.  
**Javítás:** Az értékeket HTML-encode-olni kell behelyettesítés előtt (`System.Net.WebUtility.HtmlEncode()`), vagy `<pre>` tagek helyett `<div>`-ben escaped szöveget használni.

---

### 7. Elírások fájlnevekben és kódban
**Érintett helyek:**

| Hely | Jelenlegi | Helyes |
|------|-----------|--------|
| `0_Framework/InitAllAplications.xaml` (fájlnév) | `InitAllAplications` | `InitAllApplications` |
| `0_Framework/BussinesExceptionHandling.xaml` (fájlnév) | `BussinesException` | `BusinessException` |
| `MainMachine.xaml` – LogMessage | `"Starting the initial bot preb"` | `"Starting the initial bot prep"` |
| `1_GEH/BuildErrorDictionary.cs:32` | `exceptionType + " exception" ?? "N/A"` | A `?? "N/A"` soha nem fut le, félrevezető |

> **Fájlnév-elírásoknál:** az összes hivatkozást frissíteni kell (`project.json`, `MainMachine.xaml`, `Main.xaml`).

---

### 8. `0_Framework/InitAllSettings.xaml` – Bemeneti validáció kikommentezve
**Fájl:** `0_Framework/InitAllSettings.xaml`  
**Probléma:** A `Config.xlsx` elérési útjának null/üres ellenőrzése `CommentOut` blokkban van, így soha nem fut le. Ha az argumentum üres, a workflow értelmezhetetlen hibaüzenettel omlik össze.  
**Javítás:** A validációs kódot aktiválni kell (CommentOut eltávolítása), és értelmes hibaüzenetet kell hozzá adni.

---

### 9. `0_Framework/KillAllProcesses.xaml` – Hiányzó null-ellenőrzés
**Fájl:** `0_Framework/KillAllProcesses.xaml`  
**Probléma:** A `GlobalVariables.Config["ProcessesToKill"]` lekérdezésnél nincs ellenőrzés arra, hogy a kulcs létezik-e, és az értéke nem null. Ha a konfig kulcs hiányzik, `KeyNotFoundException`, ha null, `NullReferenceException` keletkezik.  
**Javítás:**
```csharp
if (!GlobalVariables.Config.ContainsKey("ProcessesToKill") 
    || GlobalVariables.Config["ProcessesToKill"] == null) return;
```

---

### 10. `0_Framework/TakeScreenshot.xaml` – Néma hiba mindkét screenshot-nál
**Fájl:** `0_Framework/TakeScreenshot.xaml`  
**Probléma:** A Catch blokkon belüli desktop screenshot is `ContinueOnError="True"` beállítással fut. Ha mindkét kísérlet meghiúsul (pl. érvénytelen elérési út, jogosultsági hiba), a hívó workflow semmit nem tud erről.  
**Javítás:** Error szintű log bejegyzés hozzáadása, ha a fallback screenshot is sikertelen.

---

### 11. `0_Framework/SendEmail.xaml` – Nincs hibakezelés az e-mail küldésnél
**Fájl:** `0_Framework/SendEmail.xaml`  
**Probléma:** A `SendEmail` aktivitás nem TryCatch blokkban fut. Ha az e-mail szerver elérhetetlen, a kivétel a hívó workflow-ig propagálódik, és félrevezető hibaüzenetet produkál a tranzakció feldolgozási hibájaként.  
**Javítás:** TryCatch hozzáadása; e-mail küldési hiba esetén Warning szintű log, de a folyamat ne álljon le.

---

### 12. `1_GEH/BuildErrorDictionary.cs` és `.xaml` – Duplikált implementáció
**Fájlok:** `1_GEH/BuildErrorDictionary.cs`, `1_GEH/BuildErrorDictionary.xaml`  
**Probléma:** Ugyanaz a logika két helyen van implementálva: egyszer XAML `InvokeCode` blokkban, egyszer C# osztályban. A kettő szinkronban tartása karbantartási terhet jelent, és eltérések keletkezhetnek.  
**Javítás:** Csak a C# implementációt megtartani; a XAML `InvokeCode` blokkot törölni, és az osztályt meghívni.

---

## 🟢 Alacsony prioritású fejlesztési javaslatok

### 13. `1_GEH/GEH_Process.xaml` és `GEH_CreateReport.xaml` – Túl nagy fájlok
**Fájlok:** `1_GEH/GEH_Process.xaml` (2794 sor), `1_GEH/GEH_CreateReport.xaml` (2975 sor)  
**Probléma:** A `GEH_Process.xaml` egyszerre felelős a screenshot készítéséért, az Excel riport generálásáért, az e-mail küldéséért és a tranzakció státuszáért. Nehézkes karbantartás, nehezen olvasható.  
**Javítás:** A lépéseket külön workflow-okra bontani, amelyeket a `GEH_Process.xaml` sorban hív meg.

---

### 14. `MainMachine.xaml` – Initialize állapotban keletkező hiba útja nincs dokumentálva
**Fájl:** `MainMachine.xaml`  
**Probléma:** Ha az Initialize állapotban kivétel keletkezik (pl. alkalmazás megnyitása meghiúsul), az közvetlenül a `Main.xaml` külső TryCatch blokkjára propagálódik, megkerülve az állapotgép tranzícióit. Ez helyes működés, de nincs kommentben dokumentálva.  
**Javítás:** Komment hozzáadása az Initialize állapothoz, amely leírja ezt a szándékos kivételterjedési útvonalat.

---

### 15. Vegyes magyar–angol logüzenetek és kommentek
**Érintett fájlok:** Szinte minden workflow  
**Probléma:** A kódban, logüzenetekben és kommentekben keveredik a magyar és az angol nyelv, ami megnehezíti a karbantartást és a csapatmunkát (különösen ha nem magyar anyanyelvű fejlesztő dolgozik a projekten).  
**Javítás:** Egységes nyelvhasználat a logüzenetekben és kommentekben (javasolt: angol). Az e-mail sablonok és a `Config.xlsx` dokumentáció maradhat magyarul, ha az ügyfélnek szól.

---

### 16. `0_Framework/KillAllProcesses.xaml` – Kikommentezett szemétgyűjtő kód
**Fájl:** `0_Framework/KillAllProcesses.xaml`  
**Probléma:** Kikommentezett `GC.Collect()` / `GC.WaitForPendingFinalizers()` hívások találhatók a fájlban. Ezeket nem szabad RPA workflow-ban manuálisan hívni; a jelenlétük félrevezető.  
**Javítás:** A kikommentezett blokkot törölni kell.

---

### 17. `0_Framework/SendEmail.xaml` – Kikommentezett válasz-logolás
**Fájl:** `0_Framework/SendEmail.xaml`  
**Probléma:** A sikeres e-mail küldés utáni válasz (HTTP státusz, e-mail GUID) logolása `CommentOut` blokkban van, és soha nem fut le. Debug üzemmódban hasznos lenne.  
**Javítás:** A logolást aktiválni `Debug` szinttel, feltételhez kötve: `GlobalVariables.Config["Debug"] == true`.

---

### 18. `Templates/BusinessExceptionEmailTemplate.html` – Üres sablon
**Fájl:** `Templates/BusinessExceptionEmailTemplate.html`  
**Probléma:** A fájl `<body>` tagje üres, tartalommal és placeholderekkel nincs feltöltve.  
**Javítás:** Implementálni az `ErrorMailTemplate.html` mintájára, vagy törölni és az `ErrorMailTemplate.html`-t használni üzleti kivételekhez is.

---

### 19. Hiányzó dokumentáció a `Config.xlsx` sémájához
**Probléma:** Nincs dokumentálva, hogy a `Config.xlsx` pontosan milyen lapokat, oszlopokat és kötelező kulcsokat tartalmaz. A fejlesztőknek a kódból kell kikövetkeztetni a szükséges értékeket.  
**Javítás:** A `Config.xlsx`-be egy `Documentation` lapot hozzáadni, amely leírja az összes kulcsot, elfogadott típust és példaértéket.

---

### 20. `0_Framework/CheckWorkingHours.xaml` – Záró időpont befoglaló kezelése
**Fájl:** `0_Framework/CheckWorkingHours.xaml`  
**Probléma:** A jelenlegi összehasonlítás `<=`-t használ a végidőpontnál, így ha a robot pontosan a záró időpontban indul, az blokkolt időszakba esik. Nem dokumentált, hogy ez szándékos-e.  
**Javítás:** Dokumentálni a szándékot kommentben, vagy `<` operátorra váltani, ha exkluzív határt kell alkalmazni.

---

## Összefoglaló

| Prioritás | Darab |
|-----------|-------|
| 🔴 Kritikus | 4 |
| 🟡 Közepes | 8 |
| 🟢 Alacsony | 8 |
| **Összesen** | **20** |
