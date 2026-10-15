LGA Studio
==========

⚠️ 주의 / DISCLAIMER (실험 버전)
--------------------------------------------
- 본 프로그램은 테스트 및 실험 목적으로 제공되는 알파/베타 버전입니다.
- 이 프로그램을 사용함으로써 발생하는 모든 결과(데이터 손실, 시스템 오류, 컴퓨터 이상 등)에 대한 책임은 전적으로 사용자 본인에게 있습니다. 개발자는 어떠한 직·간접적 손해에 대해서도 법적 책임을 지지 않습니다.
- 사용 전 중요한 데이터는 반드시 백업하시기 바라며, 본 조건에 동의하는 경우에만 프로그램을 실행하십시오.

- THIS SOFTWARE IS AN EXPERIMENTAL VERSION PROVIDED "AS IS".
- USE AT YOUR OWN RISK. THE DEVELOPER SHALL NOT BE HELD LIABLE FOR ANY DAMAGES, DATA LOSS, OR SYSTEM INSTABILITY RESULTING FROM THE USE OF THIS PROGRAM.


한국어 안내
----------

LGA Studio는 브라우저 알림 권한과 Windows 시작 항목을 확인하고, 사용자가 직접 선택한 항목만 정리하는 Windows 프로그램입니다.

안전하게 시작하기:
1. 앱을 실행하고 Windows 관리자 권한 요청(UAC)을 확인합니다.
2. "안전 진단 한 번에 실행"을 누릅니다. 검사는 읽기 전용이며 설정을 바꾸거나 삭제하지 않습니다.
3. 목록의 항목, 경로, 명령을 주의해서 검토합니다. 의심 표시는 참고용이며 정상 항목일 수도 있습니다.
4. 원치 않는 항목임을 확인한 경우에만 선택하고 차단, 초기화 또는 삭제를 누릅니다.
5. 확인 대화상자를 읽고 동의할 때만 계속합니다. 시작 항목 삭제는 되돌릴 수 없습니다.

브라우저 권한 파일을 변경하기 전에 브라우저를 정상적으로 닫으세요. 앱은 브라우저를 강제 종료하지 않습니다.

빌드 및 실행:
- 빌드 컴퓨터에 .NET 8 SDK를 설치합니다.
- PowerShell에서 프로젝트 폴더로 이동합니다.
- 다음 명령을 실행합니다.

  dotnet restore
  dotnet build -c Release
  dotnet run

단일 실행 파일 게시:
- 다음 명령을 실행합니다.

  dotnet publish -c Release

- 게시 결과: bin\Release\net8.0-windows\win-x64\publish\LGAStudio.exe
- 실행 컴퓨터에는 .NET 런타임을 별도로 설치할 필요가 없습니다.
- 실행하면 Windows 관리자 권한 확인 창이 표시됩니다.

게시된 파일을 쉽게 찾는 방법:
1. 파일 탐색기에서 프로젝트 폴더를 엽니다.
2. bin -> Release -> net8.0-windows -> win-x64 -> publish 순서대로 폴더를 엽니다.
3. 마지막 publish 폴더 안의 LGAStudio.exe를 두 번 클릭합니다.
   net8.0-windows와 win-x64는 폴더 이름입니다. 그대로 차례대로 열면 됩니다.


English Guide
-------------

LGA Studio reviews browser notification permissions and Windows startup entries. Scanning is read-only. Cleanup requires selecting items and confirming the action.

Safe first run:
1. Launch the app and approve the Windows UAC administrator prompt.
2. Click "One-Click Scan (safe)". This only reads and lists items.
3. Carefully review each path, command, and assessment. Suspicious labels are advisory and may be wrong.
4. Select only items you have confirmed are unwanted, then choose Block, Reset, or Remove.
5. Read the confirmation dialog before proceeding. Startup removal is permanent.

Close supported browsers normally before changing browser permission files. The application does not terminate browser processes.

Build with the .NET 8 SDK:
  dotnet restore
  dotnet build -c Release
  dotnet run

Publish a standalone Windows x64 executable:
  dotnet publish -c Release

The output is:
  bin\Release\net8.0-windows\win-x64\publish\LGAStudio.exe

To find it, open the project folder in File Explorer, then open these folders in order:
  bin -> Release -> net8.0-windows -> win-x64 -> publish

Double-click LGAStudio.exe inside the final publish folder. The names net8.0-windows and win-x64 are folder names; simply open them in order.

The target computer does not need the .NET runtime installed. Windows will request administrator approval when launched.


라이선스 / LICENSE (MIT License)
--------------------------------
Copyright (c) 2026 LGA Studio

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.