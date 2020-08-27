wyam build --recipe Blog --theme Trophy --output docs;
Set-Location -Path local-server -PassThru;
dotnet build -c Release;
Set-Location -Path .. -PassThru;
