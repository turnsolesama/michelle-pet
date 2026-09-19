$ErrorActionPreference = 'Stop'
$edition = 'v0.3.7'
$outDir = Join-Path $PSScriptRoot "releases\$edition"
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$exeName = 'MichelePet.exe'
$exe = Join-Path $outDir $exeName
$appIcon = Join-Path $PSScriptRoot 'assets\icon\michele-dorm.ico'
if (-not (Test-Path -LiteralPath $appIcon)) { throw "Missing icon: $appIcon" }
$arguments = @('/nologo','/target:winexe','/optimize+','/codepage:65001',"/out:$exe",'/reference:System.dll','/reference:System.Core.dll','/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll')
$arguments += "/win32icon:$appIcon"
$arguments += "/resource:$appIcon,App.Icon"
foreach ($skin in @('classic','dessert','heart','magic')) {
    foreach ($pose in @('idle','happy','sleep','wall')) {
        $sprite = Join-Path $PSScriptRoot "assets\skins\$skin\$pose.png"
        if (-not (Test-Path -LiteralPath $sprite)) { throw "Missing sprite: $sprite" }
        $arguments += "/resource:$sprite,Skin.$skin.$pose"
    }
}
$arguments += "/resource:$(Join-Path $PSScriptRoot 'assets\skins\alpha-seeds.csv'),AlphaSeeds"
$arguments += "/resource:$(Join-Path $PSScriptRoot 'assets\companion\classic-atlas.png'),Companion.Classic"
$arguments += "/resource:$(Join-Path $PSScriptRoot 'assets\companion\hands-v3.png'),Companion.Hands"
$arguments += "/resource:$(Join-Path $PSScriptRoot 'assets\companion\dorm-body-longhair.png'),Companion.DormBody"
$arguments += "/resource:$(Join-Path $PSScriptRoot 'assets\companion\dorm-hands.png'),Companion.DormHands"
$arguments += (Get-ChildItem (Join-Path $PSScriptRoot 'src') -Filter '*.cs').FullName
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Copy-Item (Join-Path $PSScriptRoot 'README.md') (Join-Path $outDir 'README.md')
Copy-Item (Join-Path $PSScriptRoot 'README.en.md') (Join-Path $outDir 'README.en.md')
New-Item -ItemType Directory -Path (Join-Path $outDir 'docs') -Force | Out-Null
foreach ($guide in @('keyboard-companion.en.md','keyboard-companion-plan.md')) {
    Copy-Item (Join-Path $PSScriptRoot "docs\$guide") (Join-Path $outDir "docs\$guide")
}
@{ version='0.3.7'; languages=@('zh-CN','en-US'); edition=$edition; bytes=(Get-Item $exe).Length; sha256=(Get-FileHash $exe -Algorithm SHA256).Hash } | ConvertTo-Json | Set-Content (Join-Path $outDir 'package.json') -Encoding UTF8
Get-Item $exe | Select-Object FullName,Length
