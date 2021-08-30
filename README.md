<a href="https://softserve.academy/"><img src="https://s.057.ua/section/newsInternalIcon/upload/images/news/icon/000/050/792/vnutr_5ce4f980ef15f.jpg" title="SoftServe IT Academy" alt="SoftServe IT Academy"></a>

![](/img/logo.png)

# Out of School

> The platform for choosing an extracurricular activity for your children

[![Build](https://github.com/ita-social-projects/OoS-Backend/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/ita-social-projects/OoS-Backend/actions/workflows/dotnetcore.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?branch=develop&project=ita-social-projects-oos-backend&metric=coverage)](https://sonarcloud.io/dashboard?id=ita-social-projects-oos-backend&branch=develop)
[![Github Issues](https://img.shields.io/github/issues/ita-social-projects/OoS-Backend?style=plastic)](https://github.com/ita-social-projects/OoS-Backend/issues)
[![Pending Pull-Requests](https://img.shields.io/github/issues-pr/ita-social-projects/OoS-Backend?style=plastic)](https://github.com/ita-social-projects/OoS-Backend/pulls)
![GitHub](https://img.shields.io/github/license/ita-social-projects/OoS-Backend?style=plastic)

---

## Table of Contents

- [Frontend](#Frontend)
- [Installation](#Installation)
  - [Required to install](#Required-to-install)
  - [Optional to install](#Optional-to-install)
  - [Clone](#Clone)
  - [Run Dev Environment](#Run-Dev-Environment)
    - [Windows 10](#Windows-10)
    - [Linux](#Linux)
    - [Linux Alternative](#Linux-Alternative)
    - [MacOS](#MacOS)
<!-- - [Usage](#Usage)
  - [How to work with swagger UI](#How-to-work-with-swagger-UI)
  - [How to run tests](#How-to-run-tests)
  - [How to Checkstyle](#How-to-Checkstyle) -->
<!-- - [Documentation](#Documentation)
- [Contributing](#Contributing) -->
- [Contributors](#Contributors)
  <!-- - [git flow](#git-flow)
  - [issue flow](#git-flow)
<!-- - [FAQ](#faq) -->
<!-- - [Support](#support) -->
- [License](#license)

---

## Frontend

Here is the front-end part of our project: [https://github.com/ita-social-projects/OoS-Backend](https://github.com/ita-social-projects/OoS-Backend).

`develop` branch of the back-end corresponds to `develop` branch on the front-end. The same thing with `main` branches.

## Installation

### Required to install
* [ASP.NET Core Runtime 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [MS SQL Server 2019](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Optional to install
* [Opensearch](https://opensearch.org/downloads.html)
* [Opensearch Dashboards](https://opensearch.org/downloads.html)

### Clone

Clone this repo to your local machine using `git clone https://github.com/ita-social-projects/OoS-Backend`

### Run Dev Environment

The application can run as is on your local machine. To run an environment on par with production follow the next steps:

#### Windows 10

To run the environment you need the following tools:
* [VirtualBox 6.1.x](https://www.virtualbox.org/wiki/Downloads)
* [Vagrant](https://www.vagrantup.com/downloads)
* [Chocolatey](https://chocolatey.org/install)
* [PowerShell v3+](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.1)

> Disclaimer: Hyper-V doesn't allow full automation yet. So sticking to VB :(

> Note: Virtual environment created using this script will require 10+ Gb of disk space. Before launching the environment make sure you move the VM location to a disk with enough space according to [VirtualBox Documentation](https://www.virtualbox.org/manual/ch10.html#:~:text=You%20can%20change%20the%20default,8.29%2C%20%E2%80%9CVBoxManage%20setproperty%E2%80%9D.)

> Important: All commands should be executed on your host machine in Powershell, unless stated otherwise. If a command needs to run inside the VM - it's going to be prefixed with `$` (indicating a linux prompt)

##### Additional software

Install OpenSSL with Chocolatey

```powershell
choco install openssl -y
```

##### Change to project folder

All commands should be executed in project folder

```powershell
# For example
cd C:\Oos-Backend
```

##### Start new environment

Run the virtual machine

```powershell
vagrant plugin install vagrant-vbguest
vagrant up
```

> Troubleshooting: Some times due to bugs Windows, Vagrant & VirtualBox the virtual machine may fail during startup. In that case run:
```powershell
vagrant up --provision
```
> Troubleshooting: If it doesn't help and provisioning of a VM does not start destroy and start a new one. This should work.
```powershell
vagrant destroy
# Confirm removing the VM
vagrant up
```

First launch should take around 5-10 minutes. You can go make a cup of tea/coffee.

If everything's fine you can access the application through your browser at:
* [https://oos.local](https://oos.local) - for mock frontend
* [https://oos.local/webapi](https://oos.local/webapi) - for API Server
* [https://oos.local/identity](https://oos.local/identity) - for Identity Server

Additionally, databases are exposed for debugging:
* `oos.local:1433` - MS SQL
* `oos.local:5601` - Opensearch Dashboard
* `oos.local:9200` - Opensearch

##### Update application code

If you've made changes to Web API or Identity applications you can re-start the application stack within VM.

First, SSH into the VM.

```powershell
vagrant ssh
```

Inside the VM you can build one or both applications again and restart services. It's possible as the project working directory is synced with the VM automatically.

First, bring the stack down.

```bash
$ cd /vagrant
$ docker-compose down
```

Then, build the app you've just modified.

For Web API:
```bash
$ TAG=$(git rev-parse --short HEAD)
$ build oos-api:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.WebApi/
```

For Identity:
```bash
$ TAG=$(git rev-parse --short HEAD)
$ pack build oos-auth:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.IdentityServer/
```

Restart the stack:
```bash
$ bash docker-compose.sh
```

##### Stop & Restart the VM

If you want to stop the VM for time being you can run:
```powershell
vagrant halt
```

To restart the VM simply execute:
```powershell
vagrant up
```

> Note: There's a semi-rare occasion when VirtualBox 6.1.26 on Windows corrupts the VM upon stopping. There's currently no 100% working solution.
> You can try to delete `OutOfSchool.vbox` file inside `VirtualBox VMs` and rename `OutOfSchool.vbox-prev` to `OutOfSchool.vbox`. Also you can try to delete `Logs` folder inside `VirtualBox VMs` folder.
> If nothing works - `vagrant destroy` and manualy clear all leftover files in the `VirtualBox VMs` folder and start all over. On Windows you can't have nice things easily.
> It might be something to do with conflicting with Hyper-V, if the problem persists - try [this approach](http://www.hanselman.com/blog/switch-easily-between-virtualbox-and-hyperv-with-a-bcdedit-boot-entry-in-windows-81) of booting without Hyper-V

##### Remove the VM

If you want to completely remove the VM you can run:
```powershell
vagrant destroy
```

#### Linux

> If you don't want to install Docker and build containers on your host machine - use this approach.

To run the environment you need the following tools:
* [VirtualBox 6.1.x](https://www.virtualbox.org/wiki/Downloads)
* [Vagrant](https://www.vagrantup.com/downloads)

> Note: Virtual environment created using this script will require 10+ Gb of disk space. Before launching the environment make sure you move the VM location to a disk with enough space according to [VirtualBox Documentation](https://www.virtualbox.org/manual/ch10.html#:~:text=You%20can%20change%20the%20default,8.29%2C%20%E2%80%9CVBoxManage%20setproperty%E2%80%9D.)

> Important: All commands should be executed on your host machine in your favourite shell (bash, zsh), unless stated otherwise.

##### Additional software

Install additional tools

```bash
sudo apt-get update
sudo apt-get install -y openssl ca-certificates
```

##### Change to project folder

All commands should be executed in project folder

```bash
# For example
cd /home/user/Oos-Backend
```

##### Start new environment

Run the virtual machine

```bash
vagrant plugin install vagrant-vbguest
vagrant up
```

First launch should take around 5-10 minutes. You can go make a cup of tea/coffee.

If everything's fine you can access the application through your browser at:
* [https://oos.local](https://oos.local) - for mock frontend
* [https://oos.local/webapi](https://oos.local/webapi) - for API Server
* [https://oos.local/identity](https://oos.local/identity) - for Identity Server

Additionally, databases are exposed for debugging:
* `oos.local:1433` - MS SQL
* `oos.local:5601` - Opensearch Dashboard
* `oos.local:9200` - Opensearch

##### Update application code

If you've made changes to Web API or Identity applications you can re-start the application stack within VM.

First, SSH into the VM.

```bash
vagrant ssh
```

Inside the VM you can build one or both applications again and restart services. It's possible as the project working directory is synced with the VM automatically.

First, bring the stack down.

```bash
$ cd /vagrant
$ docker-compose down
```

Then, build the app you've just modified.

For Web API:
```bash
$ TAG=$(git rev-parse --short HEAD)
$ pack build oos-api:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.WebApi/
```

For Identity:
```bash
$ TAG=$(git rev-parse --short HEAD)
$ pack build oos-auth:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.IdentityServer/
```

Restart the stack:
```bash
$ bash docker-compose.sh
```

##### Stop & Restart the VM

If you want to stop the VM for time being you can run:
```bash
vagrant halt
```

To restart the VM simply execute:
```bash
vagrant up
```

##### Remove the VM

If you want to completely remove the VM you can run:
```bash
vagrant destroy
```

#### Linux Alternative

> If you don't want to play with VMs as you already use Linux

##### Additional software

Install additional tools

```bash
sudo add-apt-repository ppa:cncf-buildpacks/pack-cli
sudo apt-get update
sudo apt-get install -y \
    openssl \
    pack-cli \
    git \
    curl
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
curl -L "https://github.com/docker/compose/releases/download/1.29.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
```

##### Change to project folder

All commands should be executed in project folder

```bash
# For example
cd /home/user/Oos-Backend
```

##### Initial setup

Setup SSL certificate and create hosts entry

```bash
bash add-hosts.sh 127.0.0.1
bash create-local-ssl.sh oos.local
```

##### Build containers

```bash
bash docker-build.sh
```

##### Launch stack

```bash
# add `sudo` if the command won't work :)
bash docker-compose.sh
```

First launch should take around 2-5 minutes because you need to pull some large images. You can go make a cup of tea/coffee.

If everything's fine you can access the application through your browser at:
* [https://oos.local](https://oos.local) - for mock frontend
* [https://oos.local/webapi](https://oos.local/webapi) - for API Server
* [https://oos.local/identity](https://oos.local/identity) - for Identity Server

Additionally, databases are exposed for debugging:
* `oos.local:1433` - MS SQL
* `oos.local:5601` - Opensearch Dashboard
* `oos.local:9200` - Opensearch

##### Update application code

If you've made changes to Web API or Identity applications you can re-start the application stack.

First, bring the stack down.

```bash
# add `sudo` if the command won't work :)
docker-compose down
```

Then, build the app you've just modified.

For Web API:
```bash
TAG=$(git rev-parse --short HEAD)
pack build oos-api:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.WebApi/
```

For Identity:
```bash
TAG=$(git rev-parse --short HEAD)
pack build oos-auth:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.IdentityServer/
```

Restart the stack:
```bash
# add `sudo` if the command won't work :)
bash docker-compose.sh
```

##### Stop & Restart the stack

If you want to stop the stack for time being you can run:
```bash
# add `sudo` if the command won't work :)
docker-compose down
```

To restart the stack simply execute:
```bash
# add `sudo` if the command won't work :)
bash docker-compose.sh
```

##### Remove the stack and all volumes

If you want to completely remove the VM you can run:
```bash
# add `sudo` if the command won't work :)
docker-compose down -v
```

#### MacOS

> If you don't want to install Docker and build containers on your host machine - use this approach.

To run the environment you need the following tools:
* [VirtualBox 6.1.x](https://www.virtualbox.org/wiki/Downloads)
* [Vagrant](https://www.vagrantup.com/downloads)

> Note: Virtual environment created using this script will require 10+ Gb of disk space. Before launching the environment make sure you move the VM location to a disk with enough space according to [VirtualBox Documentation](https://www.virtualbox.org/manual/ch10.html#:~:text=You%20can%20change%20the%20default,8.29%2C%20%E2%80%9CVBoxManage%20setproperty%E2%80%9D.)

> Important: All commands should be executed on your host machine in your favourite shell (bash, zsh), unless stated otherwise. If a command needs to run inside the VM - it's going to be prefixed with `$` (indicating a linux prompt)

##### Change to project folder

All commands should be executed in project folder

```bash
# For example
cd /Users/user/Oos-Backend
```

##### Start new environment

Run the virtual machine

```bash
vagrant plugin install vagrant-vbguest
vagrant up
```

First launch should take around 5-10 minutes. You can go make a cup of tea/coffee.

If everything's fine you can access the application through your browser at:
* [https://oos.local](https://oos.local) - for mock frontend
* [https://oos.local/webapi](https://oos.local/webapi) - for API Server
* [https://oos.local/identity](https://oos.local/identity) - for Identity Server

Additionally, databases are exposed for debugging:
* `oos.local:1433` - MS SQL
* `oos.local:5601` - Opensearch Dashboard
* `oos.local:9200` - Opensearch

##### Update application code

If you've made changes to Web API or Identity applications you can re-start the application stack within VM.

First, SSH into the VM.

```bash
vagrant ssh
```

Inside the VM you can build one or both applications again and restart services. It's possible as the project working directory is synced with the VM automatically.

First, bring the stack down.

```bash
$ cd /vagrant
$ docker-compose down
```

Then, build the app you've just modified.

For Web API:
```bash
$ TAG=$(git rev-parse --short HEAD)
$ pack build oos-api:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.WebApi/
```

For Identity:
```bash
$ TAG=$(git rev-parse --short HEAD)
$ pack build oos-auth:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.IdentityServer/
```

Restart the stack:
```bash
$ bash docker-compose.sh
```

##### Stop & Restart the VM

If you want to stop the VM for time being you can run:
```bash
vagrant halt
```

To restart the VM simply execute:
```bash
vagrant up
```

##### Remove the VM

If you want to completely remove the VM you can run:
```bash
vagrant destroy
```

> Note: Sometimes the certificate trust is not remove completely. Launch `Keychain Access` app ans go to `System` -> `Certificates` (on screenshot). And delete any certificate named `oos.local`.

![Cert](/img/keychain.png)
<!-- ---

## Usage
### How to work with swagger UI
### How to run tests
### How to Checkstyle

---

## Documentation -->

<!-- ---

## Contributing

### Git flow
> To get started...
#### Step 1

- **Option 1**
    - 🍴 Fork this repo!

- **Option 2**
    - 👯 Clone this repo to your local machine using `https://github.com/ita-social-projects/SOMEREPO.git`

#### Step 2

- **HACK AWAY!** 🔨🔨🔨

#### Step 3

- 🔃 Create a new pull request using <a href="https://github.com/ita-social-projects/SOMEREPO/compare/" target="_blank">github.com/ita-social-projects/SOMEREPO</a>.

### Issue flow

--- -->

## Contributors

> Backend

[![@yfedo](https://avatars.githubusercontent.com/u/69301010?s=100&v=4)](https://github.com/yfedo)
[![@DmyMi](https://avatars.githubusercontent.com/u/24808838?s=100&v=4)](https://github.com/DmyMi)
[![@YehorOstapchuk](https://avatars.githubusercontent.com/u/62392669?s=100&v=4)](https://github.com/YehorOstapchuk)
[![@Elizabeth129](https://avatars.githubusercontent.com/u/45245513?s=100&v=4)](https://github.com/Elizabeth129)
[![@mmmpolishchuk](https://avatars.githubusercontent.com/u/55458556?s=100&v=4)](https://github.com/mmmpolishchuk)
[![@dmitrykiev](https://avatars.githubusercontent.com/u/3800688?s=100&v=4)](https://github.com/dmitrykiev)
[![@SergeyNovitsky](https://avatars.githubusercontent.com/u/10594407?s=100&v=4)](https://github.com/SergeyNovitsky)
[![@h4wk13](https://avatars.githubusercontent.com/u/13885098?s=100&v=4)](https://github.com/h4wk13)
[![@VadymLevkovskyi](https://avatars.githubusercontent.com/u/85107137?s=100&v=4)](https://github.com/VadymLevkovskyi)
[![@v-ivanchuk](https://avatars.githubusercontent.com/u/73526573?s=100&v=4)](https://github.com/v-ivanchuk)
[![@VyacheslavDzhus](https://avatars.githubusercontent.com/u/67432351?s=100&v=4)](https://github.com/VyacheslavDzhus)
[![@OlhaHoliak](https://avatars.githubusercontent.com/u/59091855?s=100&v=4)](https://github.com/OlhaHoliak)
[![@P0linux](https://avatars.githubusercontent.com/u/50420213?s=100&v=4)](https://github.com/P0linux)
[![@Bogdan-Hasanov](https://avatars.githubusercontent.com/u/47710368?s=100&v=4)](https://github.com/Bogdan-Hasanov)
> Frontend

> Bussiness Analytics

> Testers

<!-- TODO: Add FAQ-->
<!-- ---

## FAQ

- **How do I do *specifically* so and so?**
    - No problem! Just do this. -->

<!-- TODO: Add support info -->
<!-- ---

## Support

Reach out to me at one of the following places!

- Website at <a href="http://Website.com" target="_blank">`Website.com`</a>
- Facebook at <a href="https://www.facebook.com/LiubomyrHalamaha/" target="_blank">`Liubomyr Halamaha`</a>
- Insert more social links here. -->

---

## License

![GitHub](https://img.shields.io/github/license/ita-social-projects/OoS-Backend?style=plastic)

- **[MIT license](http://opensource.org/licenses/mit-license.php)**
- Copyright 2021 © <a href="https://softserve.academy/" target="_blank"> SoftServe IT Academy</a>.