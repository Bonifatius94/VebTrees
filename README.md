
# Van Emde Boas Tree / Priority Queue

## About
This project is about implementing the very efficient priority queue data structure
proposed by Peter van Emde Boas, supporting following operations:

| Operation   | Time         |
| ----------- | ------------ |
| IsEmpty     |         O(1) |
| Min         |         O(1) |
| Max         |         O(1) |
| Member      | O(log log u) |
| Successor   | O(log log u) |
| Predecessor | O(log log u) |
| Insert      | O(log log u) |
| Delete      | O(log log u) |

Note: the predecessor operation is not implemented yet.

## Usage
If you haven't done already, install dotnet to your dev machine.
Following commands show how to install dotnet onto Ubuntu 20.04.

```sh
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm -rf packages-microsoft-prod.deb

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0

# official tutorial: https://docs.microsoft.com/de-de/dotnet/core/install/linux-ubuntu#2004-
```

Download the source code by cloning this git repository.

```sh
git clone https://github.com/Bonifatius94/VebTrees
cd VebTrees
```

Now you can run the demo script creating a priority queue, inserting items,
making lookups, getting the successor (e.g. for sorting), deleting items, etc.

```sh
# use a non-optimized binary, working on any system
dotnet run

# use an optimized binary (here: optimized for linux x64 systems)
dotnet run --runtime linux-x64 --configuration Release
```

## License
This software is available under the terms of the MIT license.
