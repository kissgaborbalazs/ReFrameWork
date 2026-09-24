# ReFrameWork – Fejlesztői kézikönyv

Ez a kézikönyv a `GEH MPRT QUEUE CSharp` UiPath projektsablon (ReFrameWork) használatát mutatja be. Elsősorban **junior fejlesztőknek** készült: lépésről lépésre halad, példákat ad, és minden fejezet végén összefoglalja a legfontosabb szabályokat.

> **Tipp:** ha most találkozol először a sablonnal, olvasd el sorban az 1–4. fejezetet. A többi fejezet referencia: akkor nyisd meg, amikor szükséged van rá.

## Tartalom

| # | Fejezet | Mikor olvasd? |
|---|---------|---------------|
| 1 | [Architektúra – hogyan működik a keretrendszer](01_Architektura.md) | Első nap |
| 2 | [Első lépések – új folyamat a sablonból](02_Elso_lepesek.md) | Első nap |
| 3 | [Konfiguráció – Config.xlsx és argumentumok](03_Konfiguracio.md) | Első nap, és minden új beállításnál |
| 4 | [Hibakezelés és hibakódok](04_Hibakezeles.md) | Első héten – **a legfontosabb fejezet** |
| 5 | [Fejlesztési szabályok és best practice-ek](05_Fejlesztesi_szabalyok.md) | Első héten, utána referencia |
| 6 | [Tesztelés](06_Teszteles.md) | Mielőtt kiadod a munkádat reviewra |
| 7 | [Üzemeltetés és hibaelhárítás](07_Uzemeltetes_Hibaelharitas.md) | Élesítéskor és hibakereséskor |
| 8 | [Ellenőrző listák](08_Ellenorzo_listak.md) | Review előtt, élesítés előtt |
| 9 | [Szótár](09_Szotar.md) | Ha egy fogalom ismeretlen |

## A három legfontosabb szabály

1. **Az üzleti logika a `Process.xaml`-be és a `2_Application_Layer` / `3_Business_Logic` mappákba kerül.** A `Main.xaml`, a `MainMachine.xaml` és az `1_GEH` mappa a keretrendszer része, ezekhez csak indokolt esetben, reviewval nyúlj.
2. **Hibát mindig hibakóddal dobj:** `throw Err.Business("PRJ-BE-001", ...)`. A hibaüzenet és a javítási javaslat a katalógusból jön (lásd [4. fejezet](04_Hibakezeles.md)).
3. **Semmit ne égess be a kódba**, ami környezetenként változhat: mappa, queue név, e-mail cím, URL, felhasználó. Ezek helye a `Config.xlsx` vagy az Orchestrator asset.

## Kapcsolódó fájlok

- [`../README.md`](../README.md) – a projekt rövid áttekintése
- [`../todo.md`](../todo.md) – ismert hibák és fejlesztési javaslatok
