# 🤖 Build APK – Escape The Truth (Android)
*Wersja: MVP | Zrzut: 2025-04-10 19:46:31Z*

## Środowisko
– Android Studio (Electric Eel lub nowszy)  
– Firebase SDK  
– Unity/Flutter (w zależności od wersji silnika)

## Kroki
1. Otwórz projekt w Android Studio
2. Sprawdź konfigurację `build.gradle`
3. Ustaw `minSdkVersion` na 21 lub wyższy
4. Podłącz Firebase (`google-services.json`)
5. Wybierz `Build > Build Bundle(s) / APK(s) > Build APK`
6. Plik znajdziesz w `app/build/outputs/apk/release/app-release.apk`

## Uwagi
– Wersja testowa nie zawiera reklam ani zakupów  
– Zintegrowano system runiczny i lokalizację misji  
– Link-13 wymaga testu VoIP na emulatorze z mikrofonem
