
if ($null -EQ $IsWindows -AND 'Desktop' -EQ $PSEdition ) {
    $Script:IsWindows = $true
}

@('Debug', 'Release') | ForEach-Object {
    $ConfigurationSuffix = $PSItem
    dotnet build --graph --configuration "UniversalGL$ConfigurationSuffix"
    dotnet pack --graph --configuration "UniversalGL$ConfigurationSuffix"
    if ($IsWindows) {
        @("WindowsDX$ConfigurationSuffix", "WindowsGL$ConfigurationSuffix", "WindowsXNA$ConfigurationSuffix") | ForEach-Object {
            $Configuration = $PSItem
            $Platform = 'Any CPU'
            if ($Configuration -EQ "WindowsXNA$ConfigurationSuffix") {
                $Platform = 'x86'
            }
    
            dotnet build --graph --configuration $Configuration -property:Platform=$Platform
            dotnet pack --graph --configuration $Configuration -property:Platform=$Platform
        }
    }
}