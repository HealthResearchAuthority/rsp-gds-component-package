# Introduction
This repo is a container for hosting all common packages and libraries.

# Contributing

For detailed instructions on how to contribute to this project, please read [CONTRIBUTING.md](./docs/CONTRIBUTING.md) 

# Getting Started
1. Clone the repository
1. Make changes to the existing package OR
1. Create a new classlib project
1. Change the project version using the convention Major.Minor.Revision
1. Create a new package by using `dotnet pack` command
1. Publish the package to private NuGet feed
	1. Via CI/CD pipeline
	1. Push the package manually.

# Publish the package via CI/CD pipeline
CI/CD pipeline has been configured to generate and push the package to the Azure Artifacts private feed i.e. rsp-nuget-feed. To trigger the pipeline
Follow the steps below:
1. Clone the repository
1. Make changes to the existing package
1. Commit the changes
1. Push the changes
1. Raise the PR to master

# Publishing a package manually
1. If this is the first time you are publishing a package you need to install the [Azure Artifacts Credentials Provider](https://github.com/microsoft/artifacts-credprovider#azure-artifacts-credential-provider)
1. The `installcredprovider.ps1` has been downloaded in the scripts folder
1. Alternatively you can download it from [here](https://github.com/microsoft/artifacts-credprovider/blob/master/helpers/installcredprovider.ps1)
1. Open the Powershell in elevated mode and run the installcredprovider.ps1
1. The navigate to the directory where the package was generated, usually in the Debug folder e.g. `Rsp.Gds.Component.1.0.6.nupkg`
1. Type the following command to publish the package

	```
	dotnet nuget push .\Rsp.Gds.Component.1.0.6.nupkg --source https://pkgs.dev.azure.com/FutureIRAS/0e030eb0-cb72-4f42-b99a-26e6544271c3/_packaging/rsp-nuget-feed/nuget/v3/index.json -k iras
	```

	You must provide an api-key (-k switch) any string would do

# Consuming the package
1. Make sure you have the rsp-nuget-feed package source configured using the following url
	```
	https://pkgs.dev.azure.com/FutureIRAS/0e030eb0-cb72-4f42-b99a-26e6544271c3/_packaging/rsp-nuget-feed/nuget/v3/index.json
	```
2. In visual studio use the NuGet Package Manager to update the package

# License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details. Please see [HRA's Licensing Terms](https://dev.azure.com/FutureIRAS/Research%20Systems%20Programme/_wiki/wikis/RSP.wiki/84/Licensing-Information) for more details.