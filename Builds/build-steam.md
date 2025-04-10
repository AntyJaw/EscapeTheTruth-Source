# 🖥️ Build EXE – Escape The Truth (Steam/PC)
*Wersja: MVP | Zrzut: 2025-04-10 19:46:31Z*

## Środowisko
– Unity (2022+ lub wyższy) lub inny silnik  
– Steamworks SDK zintegrowany przez Steam API  
– Windows 10+

## Kroki
1. Skonfiguruj projekt i Player Settings
2. Zdefiniuj folder `StreamingAssets` dla zasobów gry
3. W Unity: `File > Build Settings > PC, Mac & Linux Standalone`
4. Wybierz `Target Platform: Windows`
5. Zaznacz `x86_64` i kliknij `Build`
6. Wypakuj build do folderu `builds/exe`

## Uwagi
– Steam wymaga konta developerskiego + App ID  
– VoIP LINK-13 wymaga NAT traversal lub port-forward  
– Rekomendowana rozdzielczość: 1920x1080
