# -*- mode: ruby -*-
# vi: set ft=ruby :

def windows_host?
  Vagrant::Util::Platform.windows?
end

$install_deps = <<-'SHELL'
add-apt-repository ppa:cncf-buildpacks/pack-cli
apt-get update
apt-get install -y openssl pack-cli git curl
curl -L "https://github.com/docker/compose/releases/download/1.29.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
SHELL

Vagrant.configure("2") do |config|

  config.vbguest.auto_update = true

  config.vm.box = "ubuntu/focal64"

  config.vm.network "private_network", ip: "192.168.100.100"

  config.vm.provider "virtualbox" do |vb|
    # Change to true if you want to display the VirtualBox GUI when booting the machine
    # If you don't have SSH or want to use GUI for some other reason
    # login: vagrant
    # password: vagrant
    vb.gui = false
    vb.cpus = 2
    # TODO: Probably can lower to 3Gb
    vb.memory = 4096
    vb.name = "OutOfSchool"
  end

  config.vm.provision "docker", images: [
    "paketobuildpacks/builder:base",
    "gcr.io/paketo-buildpacks/nginx",
    "gcr.io/paketo-buildpacks/dotnet-core"
  ]

  config.vm.provision "shell", inline: $install_deps
  config.vm.provision "build", type: "shell", path: "docker-build.sh", env: {"IN_VAGRANT" => "true"}
  config.vm.provision "compose", type: "shell", path: "docker-compose.sh", env: {"IN_VAGRANT" => "true"}

  config.trigger.before :up do |trigger|
    trigger.name = "Configure SSL"
    if windows_host?
      trigger.run = {path: "create-local-ssl.ps1", args: ["-Domain oos.local"]}
    else
      trigger.run = {path: "create-local-ssl.sh", args: ["oos.local"]}
    end
  end

  config.trigger.after :up do |trigger|
    trigger.name = "Configure hosts"
    trigger.run = {path: windows_host? ? "add-hosts.ps1" : "add-hosts.sh"}
  end

  config.trigger.after :up do |trigger|
    # Restart services after VM was stopped
    trigger.name = "Launch Services"
    trigger.run_remote = {inline: "bash -c 'cd /vagrant; TAG=$(git rev-parse --short HEAD) docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d'"}
  end
  
  config.trigger.after :destroy do |trigger|
    trigger.name = "Clear hosts"
    trigger.run = {path: windows_host? ? "clear-hosts.ps1" : "clear-hosts.sh"}
  end

  config.trigger.before [:halt, :provision] do |trigger|
    trigger.name = "Stop Services"
    trigger.run_remote = {inline: "bash -c 'cd /vagrant; TAG=$(git rev-parse --short HEAD) docker-compose down --remove-orphans'"}
  end
end
