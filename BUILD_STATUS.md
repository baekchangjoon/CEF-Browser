# CEF Browser 빌드 상태

## 완료된 작업

### ✅ 빌드 시스템
- .NET SDK를 사용한 빌드 시스템 구성
- dotnet CLI를 사용한 빌드 스크립트 업데이트
- CefSharp 버전을 121.3.70으로 업데이트

### ✅ 프로젝트 수정
- AssemblyInfo 중복 문제 해결 (GenerateAssemblyInfo=false)
- PlatformTarget x86 설정
- UserDataPath를 CefCommandLineArgs로 변경 (CefSharp 121 호환성)

### ✅ 빌드 성공
- 메인 브라우저 프로젝트 빌드 완료
- 테스트 프로젝트 빌드 완료
- 모든 테스트 통과 (12/12)

### ✅ 실행 파일 위치
```
CEF-Browser\bin\x86\Release\net48\CEF-Browser.exe
```

## 남은 작업

### ⚠️ Installer 프로젝트
Installer 프로젝트는 WiX Toolset이 필요합니다. 다음 중 하나를 선택하세요:

1. **WiX Toolset 설치** (권장)
   - 다운로드: https://wixtoolset.org/releases/
   - 설치 후 Installer 프로젝트 빌드 가능

2. **WixSharp.wix.bin 패키지 사용**
   - `WixSharp.bin` 대신 `WixSharp.wix.bin` 패키지 사용 고려
   - 또는 Installer 프로젝트를 별도로 빌드

## 빌드 및 실행 방법

### 빌드
```powershell
.\build.ps1
```

### 실행
```powershell
# 기본 실행
.\run.ps1

# URL 지정
.\run.ps1 -Url https://www.google.com

# User data directory 지정
.\run.ps1 -UserDataDir C:\MyData -Url https://www.naver.com
```

### 명령줄에서 직접 실행
```cmd
CEF-Browser\bin\x86\Release\net48\CEF-Browser.exe
CEF-Browser\bin\x86\Release\net48\CEF-Browser.exe https://www.google.com
CEF-Browser\bin\x86\Release\net48\CEF-Browser.exe --user-data-dir C:\MyData https://www.naver.com
```

## 테스트 실행
```powershell
dotnet test CEF-Browser.Tests\CEF-Browser.Tests.csproj -c Release
```

## 구현된 기능 확인

- ✅ CEF-Browser.exe 기본 실행
- ✅ CEF-Browser.exe <url> URL 지정 실행
- ✅ CEF-Browser.exe --user-data-dir <path> 사용자 데이터 디렉토리 지정
- ✅ CDP 지원 (포트 9222)
- ⚠️ Installer (WiX Toolset 필요)

## 다음 단계

1. 실행 파일 테스트 (google, naver 접속 확인)
2. CDP 연결 테스트
3. Installer 빌드 (WiX Toolset 설치 후)
