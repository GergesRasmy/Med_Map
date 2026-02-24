#1. Prerequisites
Before they start, ensure they have the following installed:
SDK: .NET 10.0 SDK
IDE: Visual Studio 2026
Database: SQL Server.

#2. The Setup Process
Ask them to run these commands in their terminal inside the project folder:
all dotnet commands written here ==> Go to View > Terminals 
run this command=> dotnet tool install --global dotnet-ef
a)Clone the Repository:
--git clone <https://github.com/GergesRasmy/Med_Map>--
b)Restore Dependencies:
This downloads all the NuGet packages.
--dotnet restore--
c)Update the Database:
If you used Entity Framework (EF) Core, they need to create the local database schema.
--dotnet ef database update--

#3. calling the API
	http://localhost:5136/api/"controller name"/"action name"/
	Content-Type: application/json
	check the customer account controller for more details about the available endpoints and their expected APIs and Parameters.

