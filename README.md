# Best Subsplits Comparison

A LiveSplit component that generates a comparison from your best subsplit sections. (Best Homeworlds, for example.)

## Usage

Right click Livesplit, click "Edit Layout", click "+", then click "Other -> Best Subsplits Comparison".  
  
You can tweak a few settings under Layout settings:
- You can rename the comparison. The default is "Best Subsplits"
- If **Ignore comparisons with skipped splits** is checked, segments are rejected for a section unless every split in that section has a segment time.  
  
Once installed, you will have a new comparison named "Best Subsplits" by default. (Name will be different if customized)

## Build

```powershell
dotnet build .\BestSubsplitsComparison.csproj -c Release /p:LsSrcPath="Path\To\Livesplit\src"
```  
  
The DLL is built at:
```text
bin\Release\net481\LiveSplit.BestSubsplitsComparison.dll
```
