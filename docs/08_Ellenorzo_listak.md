# 8. Ellenőrző listák

[← Tartalom](README.md)

Másold be a megfelelő listát a pull request leírásába, és pipáld ki.

## 8.1 Új folyamat indítása

- [ ] Orchestrator mappa, queue (Max # Retries feljegyezve), assetek létrehozva
- [ ] `Config.xlsx`: minden piros kulcs kitöltve, `QueueMaxRetryNumber` = Max # Retries
- [ ] `Main.xaml` fő annotációja kitöltve (név, leírás, alkalmazások, felelős)
- [ ] `InitAllAplications` / `CloseAllApplications` implementálva, `ProcessesToKill` kitöltve
- [ ] Hibakódok felvéve az `ErrorCodes` lapra (Code, Message, Remedy)

## 8.2 Mielőtt reviewra adod (pull request)

**Kód**
- [ ] Minden új workflow-nak van annotációja (feladat, argumentumok, kivételek)
- [ ] Argumentumok `in_` / `out_` / `io_` prefixszel, beszédes változónevek
- [ ] Nincs beégetett környezetfüggő érték (mappa, queue, URL, e-mail, útvonal)
- [ ] Nincs üres Catch, és nincs indokolatlan `Continue On Error = True`
- [ ] Minden kivétel `Err.Business` / `Err.System`, a kódok szerepelnek az `ErrorCodes` lapon
- [ ] Üzleti vs. rendszerhiba helyesen választva („sikerülne 5 perc múlva?”)
- [ ] Nincs jelszó, token vagy személyes adat logban, üzenetben, Configban
- [ ] A jelszót tartalmazó változók neve tartalmazza a `password`/`token`/`secret` szót
- [ ] Nincs kikommentezett kód (`CommentOut`), nincs felesleges `Delay`
- [ ] Selectorok stabilak (nincs `idx`, változó rész wildcarddal)

**Minőség**
- [ ] Workflow Analyzer hiba nélkül fut
- [ ] A módosított XAML-ek Studióban megnyitva, validálva, mentve
- [ ] Tesztek lefutnak, és a Then ágak ellenőriznek
- [ ] Minden új hibakódhoz van teszteset
- [ ] `README` / `docs` frissítve, ha új Config kulcs vagy viselkedés került be

## 8.3 Élesítés előtt

- [ ] A `Process.xaml` teszt ágai (`*_Test` referenciák) **törölve**
- [ ] `Debug` = False; a teszt e-mail címek éles címekre cserélve (assetben)
- [ ] Orchestrator: `in_FallbackAlertEmail` kitöltve, Unique Reference beállítva, Alerts beállítva
- [ ] A queue Max # Retries = Config `QueueMaxRetryNumber`
- [ ] `MaxConsecutiveSystemExceptions` kitöltve (ne legyen végtelen újrapróbálás)
- [ ] Az üzleti oldal ismeri a Business hiba e-maileket, és tudja, mi a teendő (Remedy szövegek átnézve velük)
- [ ] Az első éles futás kis limittel, felügyelettel

## 8.4 Reviewer szempontok

- Érthető-e a workflow egy új fejlesztőnek annotáció és nevek alapján?
- Mi történik, ha **ez a lépés** hibázik? Helyes-e a státusz, eljut-e a hiba a keretrendszerig?
- Van-e olyan eset, amikor a tétel Successful lesz, pedig a munka nem történt meg?
- Mi történik, ha a tételt **kétszer** dolgozzuk fel (retry után)? Idempotens-e a folyamat: nem rögzít-e duplán?
- Érthető-e a hibaüzenet és a javítási javaslat egy **nem fejlesztő** számára?
