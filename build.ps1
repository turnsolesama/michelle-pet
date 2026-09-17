$ErrorActionPreference = 'Stop'
$outDir = Join-Path $PSScriptRoot 'releases\v0.3.2'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$exe = Join-Path $outDir 'MichellePet.exe'
$arguments = @('/nologo','/target:winexe','/optimize+','/codepage:65001',"/out:$exe",'/reference:System.dll','/reference:System.Core.dll','/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll')
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
$arguments += (Get-ChildItem (Join-Path $PSScriptRoot 'src') -Filter '*.cs').FullName
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Copy-Item (Join-Path $PSScriptRoot 'README.md') (Join-Path $outDir '使用说明.md')
New-Item -ItemType Directory -Path (Join-Path $outDir 'docs') -Force | Out-Null
Copy-Item (Join-Path $PSScriptRoot 'docs\keyboard-companion-plan.md') (Join-Path $outDir 'docs\keyboard-companion-plan.md')
@{ version='0.3.2'; bytes=(Get-Item $exe).Length; sha256=(Get-FileHash $exe -Algorithm SHA256).Hash; published=$false } | ConvertTo-Json | Set-Content (Join-Path $outDir 'package.json') -Encoding UTF8
Get-Item $exe | Select-Object FullName,Length
