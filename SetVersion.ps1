param([Parameter(Mandatory)] $version);

# Requires the dotnet-setversion tool installed:
#   dotnet tool install -g dotnet-setversion
setversion $version Rucksack/Rucksack.csproj
