# 9. Szótár

[← Tartalom](README.md)

| Fogalom | Jelentés |
|---|---|
| **Asset** | Az Orchestratorban tárolt beállítás (szöveg, szám, bool) vagy hitelesítő adat (credential). Környezetenként eltérő lehet |
| **Business Exception** (üzleti kivétel) | Olyan hiba, amit az adat vagy egy üzleti szabály okoz. Újrapróbálással nem javul. Típusa `BusinessRuleException` |
| **Coded workflow** | C# nyelven írt workflow (`.cs`), `CodedWorkflow` ősosztállyal és `[Workflow] Execute` metódussal |
| **Config.xlsx** | A folyamat beállításait tartalmazó Excel (Settings, Assets, ErrorCodes lap) |
| **ErrorCatalog** | A hibakódok központi katalógusa: kód → üzenet + javítási javaslat |
| **Err** | Kivétel-factory: `Err.Business(kód, …)`, `Err.System(kód, …)` |
| **ErrorPolicy** | Hibakezelési döntések egy helyen, pl. `IsLastAttempt` (utolsó kísérlet-e) |
| **GEH** (Global Exception Handler) | UiPath funkció: minden kivételnél lefut egy kijelölt workflow (`1_GEH/GEH_Main.xaml`) |
| **GEH_Process** | A hibadokumentáció elkészítése: Excel riport, képernyőkép, e-mail |
| **Idempotens** | Egy művelet többszöri végrehajtása ugyanazt az eredményt adja, mint az egyszeri. Retry miatt fontos |
| **Main exception** | Tranzakción kívül (inicializálás, lekérdezés) keletkező hiba. A robot leáll |
| **Max # Retries** | A queue beállítása: rendszerhiba után legfeljebb ennyiszer próbálja újra a tételt az Orchestrator |
| **Queue / queue item** | Orchestrator feladatsor, illetve egy feladat (tranzakció) benne |
| **Reason / Details / Output** | A queue item mezői. Hibánál: Reason = `[KÓD] üzenet`, Details = javítási javaslat, Output = `ErrorCode` |
| **Remedy** | Javítási javaslat. A hibakódhoz tartozó teendő |
| **RetryNo** | A queue item kísérletszáma, 0-tól indul |
| **Retry Scope** | Activity, amely egy lépést feltételig újrapróbál. Rövid, lokális újrapróbálásra való |
| **Selector** | A UI elem azonosítására szolgáló XML leírás |
| **System Exception** (rendszerkivétel) | Technikai, jellemzően átmeneti hiba. A tételt az Orchestrator újrapróbálja |
| **Tranzakció** | Egy queue item feldolgozása a `Process.xaml`-ben |
