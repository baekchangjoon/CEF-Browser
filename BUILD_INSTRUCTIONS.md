# CEF Browser 빌드 및 테스트 가이드

## 프로젝트 구조

```
CEF-Browser/
├── CEF-Browser/              # 메인 브라우저 애플리케이션
│   ├── Program.cs            # 진입점 및 명령줄 파싱
│   ├── MainForm.cs           # 메인 브라우저 폼
│   ├── MainForm.Designer.cs  # UI 디자인
│   ├── CommandLineParser.cs  # 명령줄 인자 파싱
│   ├── NavigationService.cs  # URL 네비게이션 서비스
│   └── Extensions.cs         # 확장 메서드
├── CEF-Browser.Installer/    # WiX 설치 프로젝트
│   └── Installer.cs          # 설치 스크립트
└── CEF-Browser.Tests/        # 단위 테스트
    ├── CommandLineParserTests.cs
    └── NavigationServiceTests.cs
```

## 빌드 방법

### Visual Studio 사용
1. `CEF-Browser.sln` 파일을 Visual Studio에서 엽니다
2. NuGet 패키지 복원 (자동 또는 수동)
3. 솔루션 빌드 (Release 구성 권장)

### 명령줄 사용
```batch
build.bat
```

## 테스트 실행

### Visual Studio Test Explorer
1. Visual Studio에서 Test > Test Explorer 열기
2. 모든 테스트 실행

### NUnit Console Runner
```batch
nunit3-console.exe CEF-Browser.Tests\bin\Release\CEF-Browser.Tests.dll
```

## 구현된 기능

### ✅ 명령줄 인자 지원
- `CEF-Browser.exe` - 기본 실행
- `CEF-Browser.exe <url>` - URL로 직접 실행
- `CEF-Browser.exe --user-data-dir <path>` - 사용자 데이터 디렉토리 지정

### ✅ Chrome DevTools Protocol (CDP)
- 기본 포트: 9222
- `chrome://inspect`에서 연결 가능

### ✅ 설치 프로그램
- WiX 기반 MSI 설치 프로그램
- 바탕화면 바로가기 자동 생성
- 시작 메뉴 바로가기 생성

## 테스트 커버리지

### CommandLineParserTests
- URL 파싱 테스트
- user-data-dir 파싱 테스트
- 다양한 인자 조합 테스트

### NavigationServiceTests
- URL 정규화 테스트
- 프로토콜 자동 추가 테스트
- 빈 값 처리 테스트

## 주의사항

1. **플랫폼**: x86 (32비트)로 설정되어 있습니다
2. **.NET Framework**: 4.8 필요
3. **CefSharp**: NuGet 패키지 자동 다운로드
4. **아이콘**: 현재 아이콘 파일 없음 (선택사항)

## 다음 단계

1. Visual Studio에서 프로젝트 빌드
2. 테스트 실행하여 모든 테스트 통과 확인
3. Release 빌드 후 실행 파일 테스트
4. Installer 프로젝트 빌드하여 MSI 생성
