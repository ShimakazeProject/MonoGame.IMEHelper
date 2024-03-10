
if ($null -EQ $IsWindows -AND 'Desktop' -EQ $PSEdition ) {
    $Script:IsWindows = $true
}

dotnet pack --graph --configuration 'UniversalGLRelease'
if ($IsWindows) {
    @('WindowsDXRelease', 'WindowsGLRelease', 'WindowsXNARelease') | ForEach-Object {
        $Configuration = $PSItem
        $Platform = 'Any CPU'
        if ($Configuration -EQ 'WindowsXNARelease') {
            $Platform = 'x86'
        }

        dotnet build --graph --configuration $Configuration -property:Platform=$Platform
        dotnet pack --graph --configuration $Configuration -property:Platform=$Platform
    }
}