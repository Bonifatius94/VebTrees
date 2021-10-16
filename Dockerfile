FROM mcr.microsoft.com/dotnet/sdk:5.0

WORKDIR /app/src
COPY ./VebTrees.sln ./
COPY ./VebTrees/VebTrees.csproj ./VebTrees/
COPY .VebTrees.Test/VebTrees.Test.csproj ./VebTrees.Test/
RUN dotnet restore --configuration Release --runtime linux-x64

