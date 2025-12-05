# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade ImageChipper\ImageChipper.csproj
4. Upgrade ChipImage\ChipImage.csproj
5. Upgrade PackageTester\PackageTester.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### ImageChipper\ImageChipper.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### ChipImage\ChipImage.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### PackageTester\PackageTester.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`
