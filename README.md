# GEH MPRT QUEUE CSharp – ReFrameWork

UiPath RPA projekt sablon, globális kivételkezelővel (GEH) és Orchestrator queue-alapú tranzakciófeldolgozással.

---

## Tartalomjegyzék

- [Áttekintés](#áttekintés)
- [Könyvtárszerkezet](#könyvtárszerkezet)
- [Architektúra](#architektúra)
- [Konfiguráció](#konfiguráció)
- [Telepítés és indítás](#telepítés-és-indítás)
- [Bemeneti argumentumok](#bemeneti-argumentumok)
- [Kivételkezelés](#kivételkezelés)
- [Fejlesztési irányelvek](#fejlesztési-irányelvek)
- [Függőségek](#függőségek)

---

## Áttekintés

| Tulajdonság        | Érték                              |
|--------------------|------------------------------------|
| Projekt verzió     | 2.0.6                              |
| UiPath Studio      | 24.10.9.0                          |
| Kifejezési nyelv   | C#                                 |
| Célplatform        | Windows (unattended)               |
| Futtatási mód      | Orchestrator-vezérelt, queue-alapú |
| Sablon             | Igen (`isTemplate: true`)          |

A ReFrameWork egy újrafelhasználható folyamatsablon, amelyre konkrét RPA folyamatok építhetők. Az architektúra szétválasztja a keretrendszer logikáját (inicializálás, tranzakciókezelés, kivételkezelés) az üzleti logikától (`Process.xaml`). Az új folyamatok fejlesztésekor elegendő kizárólag a `Process.xaml` fájlt és a `2_Application_Layer`, `3_Business_Logic` könyvtárakat kitölteni.

---

## Könyvtárszerkezet

```
ReFrameWork/
│
├── Main.xaml                        # Fő belépési pont
├── Main.xaml.json                   # Main argumentum-metaadatok
├── MainMachine.xaml                 # Állapotgép (tranzakciós feldolgozási hurok)
├── Process.xaml                     # Egyedi tranzakció üzleti logikája ← IDE ÍRD A KÓDOT
├── project.json                     # Projekt konfiguráció és függőségek
│
├── 0_Framework/                     # Keretrendszer workflow-ok (ne módosítsd)
│   ├── InitAllSettings.xaml         # Konfig és credential szótár feltöltése Excel-ből
│   ├── CheckWorkingHours.xaml       # Munkaidőn kívüli időszakok ellenőrzése
│   ├── CreateQueueItem.xaml         # Queue item létrehozása (nem tranzakciós módhoz)
│   ├── InitAllAplications.xaml      # Alkalmazások megnyitása és beléptetése
│   ├── CloseAllApplications.xaml    # Alkalmazások bezárása
│   ├── KillAllProcesses.xaml        # Folyamatok kényszer-leállítása
│   ├── BussinesExceptionHandling.xaml  # Üzleti kivétel kezelése
│   ├── SystemExceptionHandling.xaml    # Rendszer kivétel kezelése
│   ├── SetTransactionStatus.xaml    # Queue item státusz frissítése Orchestratorban
│   ├── SendEmail.xaml               # E-mail küldés MPRT aktivitásokkal
│   ├── CreateEmailBody.xaml         # HTML e-mail törzs összeállítása sablon alapján
│   └── TakeScreenshot.xaml          # Képernyőkép készítése hibadokumentációhoz
│
├── 1_GEH/                           # Globális kivételkezelő
│   ├── GEH_Main.xaml                # UiPath GEH belépési pont (hibainformáció gazdagítás)
│   ├── GEH_Process.xaml             # Kivételkezelési folyamat (screenshot, riport, email)
│   ├── GEH_CreateReport.xaml        # Excel hibajelentés generálása
│   ├── BuildErrorDictionary.xaml    # Hibainformáció szótár összeállítása
│   └── BuildErrorDictionary.cs      # C# segédosztály a hibainformáció-szótárhoz
│
├── 2_Application_Layer/             # Alkalmazásréteg workflow-ok (fejlesztendő)
├── 3_Business_Logic/                # Üzleti logika workflow-ok (fejlesztendő)
│
├── 4_Reusable_Modules/              # Újrafelhasználható segédmodulok
│   └── PreviewDataTable.cs          # DataTable előnézeti segédeszköz
│
├── 5_Test/                          # Tesztelési workflow-ok
│   └── Main/
│       ├── Main_Successfull.xaml    # Sikeres futás teszt
│       ├── Main_BusinessException.xaml  # Üzleti kivétel teszt
│       ├── Main_SystemException.xaml    # Rendszer kivétel teszt
│       ├── Main_MainException.xaml      # Főfolyamat kivétel teszt
│       └── Config_MainException.xlsx    # Tesztkonfiguráció
│
├── Configuration/
│   └── Config.xlsx                  # Konfiguráció és credential adatok
│
└── Templates/
    ├── MailTemplate.html                   # Sikeres futás e-mail sablon
    ├── ErrorMailTemplate.html              # Hiba e-mail sablon
    ├── BusinessExceptionEmailTemplate.html # Üzleti kivétel e-mail sablon
    └── GEH_ReportTemplate.xlsx             # Excel hibajelentés sablon
```

---

## Architektúra

### Végrehajtási folyamat

```
Main.xaml
 │
 ├─ [1] CheckWorkingHours   → megáll, ha munkaidőn kívül vagyunk
 ├─ [2] InitAllSettings     → Config.xlsx betöltése a GlobalVariables szótárakba
 ├─ [3] CreateQueueItem     → queue item(ek) létrehozása (ha nem tranzakciós mód)
 └─ [4] MainMachine.xaml (állapotgép)
         │
         ├─ ÁLLAPOT: Initialize
         │   ├─ Első futás: kivétel mappa cleanup, KillAllProcesses, InitAllApplications
         │   └─ Visszatérés: MaxConsecutiveSystemExceptions ellenőrzése
         │
         ├─ ÁLLAPOT: Get Transaction Data
         │   ├─ Stop signal ellenőrzése (Orchestrator)
         │   ├─ Tranzakciókorlát ellenőrzése (in_Limit)
         │   └─ Következő queue item lekérése
         │
         ├─ ÁLLAPOT: Process Transaction
         │   ├─ Process.xaml meghívása
         │   ├─ Kivétel típus meghatározása (Business / System)
         │   └─ SetTransactionStatus (Sikeres / Business / System hiba)
         │
         └─ ÁLLAPOT: End Process (végállapot)
             └─ CloseAllApplications
```

### Kivételkezelési rétegek

| Réteg | Fájl | Szerepe |
|-------|------|---------|
| Global Exception Handler | `1_GEH/GEH_Main.xaml` | UiPath GEH belépési pont; exception.Data gazdagítása kontextussal |
| Process Exception Handler | `1_GEH/GEH_Process.xaml` | Screenshot, Excel riport, hibaértesítő e-mail küldése |
| Business Exception | `0_Framework/BussinesExceptionHandling.xaml` | Üzleti hiba esetén egyedi kezelés (fejlesztendő) |
| System Exception | `0_Framework/SystemExceptionHandling.xaml` | Rendszer hiba esetén egyedi kezelés, újrapróbálkozás logika |
| Main catch block | `Main.xaml` | GEH_Process + KillAllProcesses + Workflow terminálás |

---

## Konfiguráció

### Config.xlsx kötelező mezők

A `Configuration/Config.xlsx` fájl két lapot tartalmaz: **Settings** és **Assets** (credentialekhez).

| Kulcs | Típus | Leírás |
|-------|-------|--------|
| `AutomationName` | String | Folyamat neve (e-mailekben és logokban jelenik meg) |
| `ExceptionFolder` | String | Hibaképek és riportok mentési útvonala |
| `TransactionalProcess` | Boolean | `True`: queue-ból olvassa az elemeket; `False`: maga hozza létre |
| `ProcessesToKill` | String | Vesszővel elválasztott folyamatnevek (pl. `chrome.exe,notepad.exe`) |
| `MaxConsecutiveSystemExceptions` | Int | Egymást követő rendszer kivételek maximuma leállás előtt |
| `MailSubject` | String | E-mail tárgy sablonszöveg (pl. `{result} - {automationName} - {reference}`) |
| `Debug` | Boolean | `True`: e-mailek nem kerülnek elküldésre |

### GlobalVariables

Az `InitAllSettings.xaml` futtatása után a következő globális szótárak érhetők el:

- `GlobalVariables.Config` – `Dictionary<string, object>` – általános beállítások
- `GlobalVariables.Credentials` – `Dictionary<string, PSCredential>` – titkosított hitelesítők
- `GlobalVariables.MaxQueueItem` – `Int32` – maximális feldolgozandó elemek száma (Main.xaml állítja be)

---

## Telepítés és indítás

### Előfeltételek

- UiPath Studio 24.10 vagy újabb
- UiPath Orchestrator hozzáférés (queue-ok kezeléséhez)
- MPRT.Activities csomag (1.0.11) – belső NuGet feed szükséges
- MPRT.Excel csomag (1.0.1)

### Beállítási lépések

1. **Nyisd meg a projektet** UiPath Studio-ban (`project.json` megnyitásával).
2. **Töltsd fel a függőségeket** a `Manage Packages` panelen keresztül (belső MPRT feed).
3. **Másold le a `Config.xlsx` sablont** a `Configuration/` mappába és töltsd ki a kötelező mezőket.
4. **Hozd létre a queue-t** az Orchestratorban és írd be a nevét a `Config.xlsx`-be.
5. **Konfiguráld az e-mail sablonokat** a `Templates/` mappában.
6. **Implementáld az üzleti logikát** a `Process.xaml`-ben és a `2_Application_Layer/`, `3_Business_Logic/` könyvtárakban.
7. **Töltsd ki** az `InitAllApplications.xaml` és `CloseAllApplications.xaml` fájlokat a konkrét alkalmazásokhoz.
8. **Futtasd a teszteseteket** a `5_Test/Main/` könyvtárból.

### Orchestrator Job argumentumok

| Argumentum | Alapértelmezett | Leírás |
|------------|-----------------|--------|
| `in_ConfigFilePath` | `Configuration\Config.xlsx` | Konfig fájl elérési útja |
| `in_Limit` | `1` | Max feldolgozandó queue elemek száma (0 = korlátlan) |
| `in_StopAtLimit` | `True` | Megálljon-e a limit elérésekor |
| `in_NonWorkingHours` | *(üres)* | Munkaidőn kívüli időszakok (pl. `14:00-15:00,17:00-17:30`) |

---

## Kivételkezelés

### Kivétel típusok

| Típus | Osztály | Kezelés | Queue státusz |
|-------|---------|---------|---------------|
| Üzleti kivétel | `BusinessRuleException` | `BussinesExceptionHandling.xaml` meghívása, továbblép a következő elemre | `Failed – Business Exception` |
| Rendszer kivétel | `SystemException` | `SystemExceptionHandling.xaml` meghívása, `ConsecutiveSystemExceptions` növelése, visszaugrik az Initialize állapotra | `Failed – Application Exception` |
| Főfolyamat kivétel | Bármely nem kezelt kivétel | GEH_Process + KillAllProcesses + Terminate | – |

### Maximális egymást követő rendszer kivételek

Ha `ConsecutiveSystemExceptions >= Config["MaxConsecutiveSystemExceptions"]`, az Initialize állapot kivételt dob, és a folyamat leáll. Ez megakadályozza a végtelen újrapróbálkozást rendszerhiba esetén.

---

## Fejlesztési irányelvek

### Mit szabad és kell módosítani

- `Process.xaml` – **az üzleti logika egyetlen helye**
- `0_Framework/InitAllAplications.xaml` – alkalmazások megnyitása és bejelentkezés
- `0_Framework/CloseAllApplications.xaml` – alkalmazások bezárása
- `0_Framework/BussinesExceptionHandling.xaml` – üzleti kivétel specifikus kezelés
- `0_Framework/SystemExceptionHandling.xaml` – rendszer kivétel specifikus kezelés (pl. screenshot mentés)
- `2_Application_Layer/` – alkalmazásréteg workflow-ok
- `3_Business_Logic/` – üzleti logika segédworkflow-ok
- `Templates/` – e-mail sablonok testreszabása

### Mit NE módosíts

- `MainMachine.xaml` – állapotgép logikája (csak indokolt esetben, megfontoltan)
- `Main.xaml` – főfolyamat vezérlése
- `1_GEH/` – globális kivételkezelő (csak keretrendszer-szintű változtatáshoz)

### Kivétel dobása `Process.xaml`-ből

```csharp
// Üzleti kivétel (várt hiba, pl. hibás bemeneti adat):
throw new BusinessRuleException("A reference nem található a rendszerben.");

// Rendszer kivétel (váratlan technikai hiba):
throw new SystemException("Nem sikerült csatlakozni az adatbázishoz.");
```

A keretrendszer automatikusan kezeli a kivételt, és beállítja a megfelelő queue item státuszt.

---

## Függőségek

| Csomag | Verzió | Szerepe |
|--------|--------|---------|
| `MPRT.Activities` | 1.0.11 | Egyedi RPA aktivitások (e-mail, queue, riport) |
| `MPRT.Excel` | 1.0.1 | Excel olvasás/írás aktivitások |
| `UiPath.System.Activities` | 25.8.0 | Alapvető rendszer aktivitások |
| `UiPath.UIAutomation.Activities` | 25.10.13 | UI automatizálási aktivitások |
| `UiPath.Mail.Activities` | 2.3.10 | E-mail aktivitások (fallback) |
| `UiPath.Testing.Activities` | 25.4.1 | Tesztelési aktivitások |
