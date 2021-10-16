
# use the official Microsoft build image for .NET 5
FROM mcr.microsoft.com/dotnet/sdk:5.0

# restore NuGet packages (caching)
WORKDIR /app/src
COPY ./VebTrees.sln ./
COPY ./VebTrees/VebTrees.csproj ./VebTrees/
COPY ./VebTrees.Test/VebTrees.Test.csproj ./VebTrees.Test/
RUN dotnet restore --configuration Release --runtime linux-x64

# copy the source code
COPY ./ ./

# run build + test + package steps
RUN dotnet publish -o /app/bin --configuration Release \
    --runtime linux-x64 --no-restore
