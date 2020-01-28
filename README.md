# CloudAtlas

## Project
### Build
Backend (interpreter, server and client applications) is written in C#, frontend in JavaScript with help of Vue.js framework.

Project uses open source distribution of [.NET Core](https://dotnet.microsoft.com/download/linux-package-manager/sdk-current).

After installation of `dotnet`, building a project on Ubuntu require running command:
```
dotnet build
```
in main project directory.

### Structure

Project is divided into five applications, it is best to run each one in its own directory:
 - **Interpreter** is an independent console application, interpreter of SQL-stylized queries. The additional argument is a path to the file describing all of the ZMIs (by default, it searches for `zmis.txt` file):
 ```bash
 cd Interpreter
 dotnet run [zmiFileName]
 ```
 
 - **CloudAtlasAgent** is server application divided into modules and wrapper for the interpreter. By default running on `127.0.0.1:5000` (node communication module) oraz `127.0.0.1:5001` (RPC module). It needs two arguments to run - name of the ZMI and path to file with public RSA key for verifying query signatures. `-i` flag marks path to configuration file (`random.ini` and `roundrobin.ini` are both example configuration files). `dotnet run -- --help` provides description of all the argument flags. Standard run:
 ```bash
 cd CloudAtlasAgent
 dotnet run -- -k ../QuerySigner/rsa.pub -n "/uw/violet07"
 ```

 - **CloudAtlasClient** is a client application communicating with **Agent** on behalf of RPC calls and also serving as HTTP server for user interface frontend. Similarly as in case of **Agent** it is possible to show additional run options with `dotnet run -- --help` command.
 **Client** uses [Nancy](http://nancyfx.org/) miniframework (inspired by both Sinatra and Flask) to serve webpage and provide REST API with data downloaded from **Agent** instance. One-page webpage written with Vue.js framework, its sources can be found in `FrontEnd` directory.
 ```bash
 cd CloudAtlasClient
 dotnet run
 ```

 - **Fetcher** is second client application, updating associated **Agent** with data about machine it works on. To fetch data it runs shell script `fetch.sh` (tested on Ubuntu 16.04, does not work on Mac OS X). Running **Fetcher** require providing it with path to initialization file and name of associated ZMI associated:
 ```bash
 cd Fetcher
 dotnet run -i sample.ini -n "/uw/violet07"
 ```
With `-c` flag fetcher can be provided with path to fallback contacts file, contacts to which **Agent** will try to connect when without any other.

 - **QuerySigner** signs the queries **Client** provides:
 ```bash
 dotnet run -- -k rsa.private
 ```

A few project files can be found in `Shared` directory -- the basic library used by multiple applications.

