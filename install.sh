#!/bin/bash

GIT_URL="https://github.com/ivancolomer/api-reac-android/"
NGROK_KEY=""
TELEGRAM_KEY=""
read -p "NGROK_KEY: " NGROK_KEY
read -p "TELEGRAM_KEY: " TELEGRAM_KEY

# Initialize sudo access
sudo -v

sudo adduser --disabled-password --gecos "Full name,Room number,Work phone,Home" reac
sudo usermod -a -G adm,dialout,cdrom,sudo,audio,video,plugdev,games,users,input,netdev,gpio,i2c,spi reac
sudo usermod --password $(openssl passwd -1 SemesterProject) reac
sudo su - reac

cd $HOME
mkdir onboot
cd $HOME/onboot

curl -sL https://deb.nodesource.com/setup_10.x | sudo bash -

sudo apt-get -qq update && sudo apt-get -yq upgrade && sudo apt-get -yq dist-upgrade
sudo apt-get install -yq curl wget git libunwind8 gettext apt-transport-https mariadb-server openssh-server ufw nodejs npm node-semver zip

sudo npm install -g localtunnel

sudo wget https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-linux-arm.zip
sudo unzip ngrok-stable-linux-arm.zip
sudo rm ngrok-stable-linux-arm.zip
sudo chmod +x ngrok
./ngrok authtoken $NGROK_KEY

sudo tee "/etc/sudoers.d/010_pi-nopasswd" > /dev/null 2>&1 << EOM
pi ALL=(ALL) PASSWD: ALL
reac ALL=(ALL) PASSWD: ALL
reac ALL=(ALL) NOPASSWD: /usr/local/bin/lt*
EOM

sudo ufw allow 22
sudo ufw allow 3000
sudo ufw allow 8080
sudo ufw allow 8081
sudo ufw allow 8082
sudo ufw allow 8083
sudo ufw allow 8084
sudo ufw allow ssh
sudo ufw enable
sudo ufw status

sudo ufw limit ssh/tcp

sudo tee /tmp/my_script.conf > /dev/null <<EOT
[Unit]
Description=My Script Service
Wants=network-online.target
After=network-online.target

[Service]

Type=simple
User=reac
WorkingDirectory=/home/reac
ExecStart=/home/reac/onboot/startup.sh

[Install]
WantedBy=multi-user.target

EOT
sudo env SYSTEMD_EDITOR="cp /tmp/my_script.conf" systemctl edit --force --full my_script.service
sudo systemctl enable my_script.service
sudo systemctl start my_script.service

sudo tee startup.sh > /dev/null <<EOT
#!/bin/bash

#CONSTANT_VARIABLES
HOME_DIR="/home/reac/onboot/"
TELEGRAM_API_KEY="$TELEGRAM_KEY"
EOT

sudo tee -a startup.sh > /dev/null <<'EOF'
CLIENTS=("719697313")

CLIENTS_JOINED=""

prepare_clients () {
    for client in "${CLIENTS[@]}"; do
        CLIENTS_JOINED="${CLIENTS_JOINED}-c ${client} "
    done
    CLIENTS_JOINED="${CLIENTS_JOINED:0:-1}"
}

send_telegram () {
    if [ ${#CLIENTS_JOINED} -eq 0 ]; then
        prepare_clients
    fi

    "$HOME_DIR"telegram -M -t $TELEGRAM_API_KEY $CLIENTS_JOINED "$1"
}

send_telegram "*Machine rebooted!*"$'\n\n'"Getting new IP address..."

pkill ngrok
pkill node
nohup "$HOME_DIR"ngrok tcp 22 > /dev/null &
pid=$!

while true; do
    url=$(curl http://localhost:4040/api/tunnels | jq ".tunnels[0].public_url")
    [[ ${#url} -le 6 ]] || break
    sleep 1
done

parsed_url="${url:7:-1}"

send_telegram "${parsed_url%:*}"
send_telegram "${parsed_url#*:}"

wait $pid

EOF

[ ! -d "$HOME/.ssh" ] && sudo mkdir $HOME/.ssh

sudo tee -a $HOME/.ssh/authorized_keys > /dev/null <<'EOF'
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQDoP3NfahrHlFy8yiBbM4CHr+CsrdNDLM1SiGU4uGbe1Y1ICKEag128l0pJAPTwncVrIT2+AGQRZ/rGyw0xDC2SR4B5NMNydOcEHgmu98QtvgroRQOQYjybGo/2nPU6edTnvlgZdFGF/GQAD60CBKmPpRuaUd+mBiOaq3ZFdY3Ip4OX09zS7VgtZxny9WLCYpkkgbbyUW0XkpJChYilrPtm5nUvPIca6LFrOs/fXARjU6MT/pALRvGi8cJWqcm33kgkigjyRjqsWXB3NNWhMnyaoVLpBrZgIpCy3PRwLePlD+qnBf4Gdruzq7TS4ayhjEJY08t86yYOQoXkznj9qd7T reac@raspberrypi
EOF

wget https://raw.githubusercontent.com/ivancolomer/api-reac-android/master/telegram
sudo chmod +x telegram.sh

wget https://raw.githubusercontent.com/ivancolomer/api-reac-android/master/sshd_config

wget https://raw.githubusercontent.com/ivancolomer/api-reac-android/master/database.sql

sudo mysql_secure_installation

echo "Please enter root user MySQL password!"
echo "Note: password will be hidden when typing"

echo -n Password: 
read -s password
echo

sudo mysql -uroot -p${password} -e "CREATE DATABASE reac /*\!40100 DEFAULT CHARACTER SET utf8 */;"
sudo mysql -uroot -p${password} -e "CREATE USER reac_user@localhost IDENTIFIED BY 'SemesterProject';"
sudo mysql -uroot -p${password} -e "GRANT ALL PRIVILEGES ON reac.* TO 'reac_user'@'localhost';"
sudo mysql -uroot -p${password} -e "FLUSH PRIVILEGES;"

sudo mysql -uroot -p${password} reac < database.sql

echo "FINISHED, remember to copy /etc/ssh/sshd_config and mv the downloaded here to there"
