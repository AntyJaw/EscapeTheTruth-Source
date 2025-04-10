# 🍎 Build IPA – Escape The Truth (iOS)
*Wersja: MVP | Zrzut: 2025-04-10 19:46:31Z*

## Środowisko
– Xcode 14+  
– Firebase SDK (zintegrowane przez Swift Package Manager)  
– Konto Apple Developer (do testowania IPA)

## Kroki
1. Otwórz projekt w Xcode
2. Sprawdź plik `Info.plist` i ustaw `NSCameraUsageDescription`, `NSLocationWhenInUseUsageDescription`
3. Dodaj `GoogleService-Info.plist` do projektu
4. Wybierz urządzenie (np. „Generic iOS Device”)
5. Z menu: `Product > Archive`
6. Kliknij `Distribute App` > `Ad Hoc` lub `Development`
7. Zapisz plik `.ipa` do lokalizacji roboczej

## Uwagi
– Wersja nie zawiera AppStore entitlements  
– Runy i system AR wymagają kamer fizycznych
