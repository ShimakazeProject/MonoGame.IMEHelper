'DX', 'GL', 'XNA' | Foreach-Object {
    dotnet build -c:"Release$_"
}