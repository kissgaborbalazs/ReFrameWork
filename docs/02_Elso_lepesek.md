# 2. Első lépések – új folyamat a sablonból

[← Tartalom](README.md)

## 2.1 Előfeltételek

- UiPath Studio 24.10 vagy újabb, **C#** kifejezési nyelvvel
- Hozzáférés az Orchestratorhoz (a folyamat mappájához és queue-jához)
- Hozzáférés a belső MPRT NuGet feedhez (`MPRT.Activities`, `MPRT.Excel`)

## 2.2 Új projekt létrehozása

1. Studio → **New Project** → válaszd a `GEH MPRT QUEUE CSharp` sablont. Ha nem jelenik meg, nyisd meg ezt a repót, és a `project.json`-ból indulj ki.
2. Adj a projektnek beszédes nevet, például `Szamlafeldolgozas_SAP`.
3. **Manage Packages** → ellenőrizd, hogy minden függőség települt-e (piros felkiáltójel = hiányzó csomag).
4. Nyisd meg a `Main.xaml`-t, és töltsd ki a fő Sequence annotációját: folyamat neve, leírása, érintett alkalmazások, felelős.

## 2.3 Orchestrator előkészítése

1. Hozd létre a folyamat **mappáját** (vagy kérd meg az adminisztrátort).
2. Hozd létre a **queue**-t. Jegyezd fel a **Max # Retries** értékét (tipikusan 1–3), mert ezt a Configba is be kell írni.
3. Hozd létre a szükséges **asseteket** (credential, URL, e-mail címek).

## 2.4 Config.xlsx kitöltése

A `Configuration/Config.xlsx` **Settings** lapján a piros kulcsok kötelezők. A minimum:

| Kulcs | Példa | Megjegyzés |
|---|---|---|
| `AutomationName` | `Számlafeldolgozás SAP` | Megjelenik az e-mailekben |
| `RobotName` | `RPA Robot 01` | Az e-mailek aláírása |
| `OrchestratorQueueFolder` | `Processes/Szamla` | Üresen hagyva a job saját mappája |
| `OrchestratorQueueName` | `Szamla_queue` | |
| `QueueMaxRetryNumber` | `2` | **Egyezzen** a queue Max # Retries értékével! |
| `ExceptionSubFolder` | `Szamlafeldolgozas` | A kivétel-fájlok almappája |
| `*ExceptionEmailAddresses` | `rpa-team@ceg.hu` | Vesszővel elválasztva |

A teljes lista a [3. fejezetben](03_Konfiguracio.md) található.

## 2.5 Az alkalmazások megnyitása és bezárása

- `0_Framework/InitAllAplications.xaml`: nyisd meg és léptesd be a folyamat alkalmazásait. A tényleges lépéseket a `2_Application_Layer/`-ben lévő workflow-kból hívd, például `2_Application_Layer/SAP/SAP_Login.xaml`.
- `0_Framework/CloseAllApplications.xaml`: zárd be az alkalmazásokat, lehetőleg rendezetten (kijelentkezés, ablak bezárása).
- `Config.xlsx` → `ProcessesToKill`: sorold fel a folyamatneveket (pl. `saplogon,chrome`). Ezeket a keretrendszer indításkor és Main hiba esetén kilövi.

> **Miért kell mindkettő?** A `CloseAllApplications` a „szép” bezárás, normál befejezéskor fut. A `KillAllProcesses` a „vészfék”: indításkor és súlyos hibánál biztosítja, hogy ne maradjon beragadt ablak.

## 2.6 Az üzleti logika megírása

1. Nyisd meg a `Process.xaml`-t.
2. **Töröld a teszt `IfElseIf` blokk Else ágát**, és a saját logikádat tedd a helyére. A teszt ágak (`BusinessException_Test`, `SystemException_Test`, `MainException_Test`) fejlesztés közben maradhatnak, **élesítés előtt töröld őket** (lásd `todo.md` 1. pont).
3. A logikát bontsd kis, újrahasznosítható workflow-kra:

```
Process.xaml
 ├─ 2_Application_Layer/SAP/SAP_KeresSzamla.xaml      (in_SzamlaSzam → out_Talalat)
 ├─ 3_Business_Logic/SzamlaValidacio.cs               (üzleti szabályok, coded workflow)
 └─ 2_Application_Layer/SAP/SAP_RogzitSzamla.xaml
```

4. Az üzleti szabálysértéseket **hibakóddal** dobd:

```csharp
// Throw activity – Exception mezője:
Err.Business("PRJ-BE-001", in_TransactionItem.Reference)
```

5. Vedd fel a kódot a `Config.xlsx` **ErrorCodes** lapjára:

| Code | Message | Remedy |
|---|---|---|
| `PRJ-BE-001` | `A(z) {0} számla nem található a SAP-ban` | `Ellenőrizd a számlaszámot a forrásrendszerben, majd tedd vissza a tételt a queue-ba` |

## 2.7 Első futtatás

1. Tegyél egy teszt tételt a queue-ba (Orchestrator → Queues → Add item, vagy a `5_Test` teszteseteivel).
2. Studióban futtasd a `Main.xaml`-t **Debug** módban.
3. Ellenőrizd:
   - a logban a `SetTransactionStatus` üzeneteit;
   - az Orchestratorban a tétel státuszát, és hiba esetén a Reason és Details mezőt.

## Összefoglaló

- Orchestrator (mappa, queue, assetek) → Config.xlsx → Init/Close → Process.xaml → hibakódok → teszt.
- A `QueueMaxRetryNumber` mindig egyezzen a queue beállításával.
- A teszt ágakat élesítés előtt töröld a `Process.xaml`-ből.
