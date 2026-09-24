# 7. Üzemeltetés és hibaelhárítás

[← Tartalom](README.md)

## 7.1 Élesítés (deploy)

1. **Előfeltétel:** a [8. fejezet](08_Ellenorzo_listak.md) élesítés előtti listája kipipálva.
2. Studio → **Publish** (verziószám emelése: hibajavítás → patch, új funkció → minor).
3. Orchestrator:
   - Process létrehozása / frissítése a megfelelő mappában.
   - **Job argumentumok:** `in_Limit`, `in_StopAtLimit`, `in_NonWorkingHours`, és **`in_FallbackAlertEmail` (kötelezően kitöltve)**.
   - Queue: **Max # Retries** = Config `QueueMaxRetryNumber`. **Unique Reference** bekapcsolása, ha a tétel referenciája egyedi (így nem kerülhet be duplán).
   - Assetek létrehozása a PROD értékekkel (credential, e-mail címek, `ExceptionMainFolder`).
   - Trigger (idő- vagy queue-alapú).
4. **Első éles futás:** kis limit (`in_Limit = 1..5`), felügyelettel.

## 7.2 Napi felügyelet

| Mit nézz? | Hol? | Mire figyelj? |
|---|---|---|
| Job státusz | Orchestrator → Jobs | Faulted / Stopped jobok |
| Queue | Orchestrator → Queues | Failed tételek aránya, beragadt In Progress tételek |
| Hibakódok | Queue export → `Reason` / `Output.ErrorCode` | Melyik kód a leggyakoribb |
| E-mailek | A `*ExceptionEmailAddresses` postafiókok | Business: az üzleti oldal teendője. System/Main: fejlesztői teendő |
| Kivétel-fájlok | `ExceptionFolder` | Riportok, screenshotok. A robot induláskor törli az egy hónapnál régebbieket |

> **Tipp – Pareto:** exportáld havonta a queue-t, és számold meg a Failed tételeket hibakódonként. Ha egy kód a hibák nagy részét adja, azt érdemes a forrásnál megszüntetni (üzleti folyamat vagy adatminőség), nem a robotban.

Orchestrator **Alerts**: állíts be értesítést a Faulted jobokra. Ez akkor is jelez, ha a robot semmilyen e-mailt nem tudott küldeni.

## 7.3 Hibaelhárítás hibakód szerint

| Kód / tünet | Valószínű ok | Teendő |
|---|---|---|
| `FW-CFG-002` | Rossz `in_ConfigFilePath`; a Config.xlsx nyitva van; hiányzó asset vagy nincs rá jogosultság | Ellenőrizd az útvonalat, zárd be a fájlt, ellenőrizd az Assets lap asseteit az Orchestratorban |
| `FW-CFG-001` | Hibás `ErrorCodes` lap | A Reason megmondja, mi a baj: hiányzó oszlop, duplikált kód vagy üres Message |
| `FW-SYS-001` | Egymás után túl sok rendszerhiba: az alkalmazás elérhetetlen, vagy megváltozott a felülete | Nézd meg az előző tételek riportjait és screenshotjait. Ellenőrizd az alkalmazást kézzel |
| `FW-SYS-000` | Nem kódolt technikai hiba (pl. SelectorNotFound, Timeout) | Riport: activity neve, workflow, stack trace. **Utána vegyél fel rá kódot**, és kezeld a hibát (4.6) |
| `FW-BE-000` | Kód nélküli üzleti hiba | Az üzenet alapján kezeld, és a kódban cseréld `Err.Business`-re |
| `PRJ-…` | Projektszintű hiba | A Details mező tartalmazza a teendőt |
| A tétel Successful, de a munka nem történt meg | Elnyelt kivétel (üres Catch, `Continue On Error`) | Keresd meg a Catch-et vagy a Continue On Error-t az adott lépésben (4.6) |
| Nem jön hiba e-mail | `ShouldSend*` = False; rossz címzett; rendszerhibánál még nem az utolsó kísérlet; a levelezés hibázott | Nézd meg a logban a `GEH_Process failed` vagy `Skipped sending` üzenetet, és a `QueueMaxRetryNumber` értékét |
| Túl sok rendszerhiba e-mail | A `QueueMaxRetryNumber` kisebb, mint a queue Max # Retries | Igazítsd össze a kettőt |
| A robot nem indul munkaidőn kívül | `in_NonWorkingHours` | A log tartalmazza a leállás okát |

## 7.4 Hogyan olvasd a hibariportot (Excel)?

Az `Exceptions_Reports` mappában tételenként és kísérletenként külön fájl készül. A legfontosabb adatok:
- **Activity neve és típusa:** melyik lépés hibázott.
- **Workflow fájl:** melyik XAML-ben.
- **Arguments / Variables:** a változók értékei a hiba pillanatában. A jelszavak `***`, az 500 karakternél hosszabb értékek csonkolva.
- **Stack trace:** fejlesztőknek, a pontos hívási lánc.

## Összefoglaló

- Élesítéskor: `in_FallbackAlertEmail` kitöltve, `QueueMaxRetryNumber` = Max # Retries, Unique Reference.
- Felügyelet: Failed arány, hibakódonkénti statisztika, Orchestrator Alerts.
- Minden `FW-SYS-000` egy lehetőség: vegyél fel rá kódot és javítási javaslatot.
