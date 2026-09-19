# Produção no Ubuntu (Proxmox)

A implantação usa Docker Compose com aplicação ASP.NET Core 8, MySQL 8.4 e Caddy. O Caddy publica 80/443 e gerencia HTTPS; o MySQL fica apenas na rede interna. Banco, certificados e chaves de cookies persistem em volumes Docker.

## 1. Rede e DNS

- Aloque ao container ao menos 2 vCPU, 2 GB de RAM e 20 GB de disco e use IP fixo.
- Aponte o registro DNS `A` do domínio para seu IP público.
- Encaminhe TCP 80 e TCP/UDP 443 para o container. Nunca exponha 3306.
- Restrinja SSH por chave e, de preferência, VPN/Tailscale ou IP de origem.

HTTPS automático requer domínio público e portas 80/443 acessíveis. Para uso só interno, adapte o Caddy ao proxy/TLS da rede.

## 2. Instalar Docker

No Ubuntu 22.04/24.04:

```bash
sudo apt update
sudo apt install -y ca-certificates curl git
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
. /etc/os-release
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu ${UBUNTU_CODENAME:-$VERSION_CODENAME} stable" | sudo tee /etc/apt/sources.list.d/docker.list >/dev/null
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo systemctl enable --now docker
```

## 3. Usuário e repositório

```bash
sudo adduser --disabled-password --gecos "" deploy
sudo usermod -aG docker deploy
sudo install -d -o deploy -g deploy -m 700 /home/deploy/.ssh
sudo install -d -o deploy -g deploy -m 755 /opt/kombfusca
sudo install -d -o root -g deploy -m 750 /etc/kombfusca
sudo -u deploy git clone URL_SSH_DO_REPOSITORIO /opt/kombfusca
sudo chmod +x /opt/kombfusca/deploy/deploy.sh
```

Coloque a chave pública da automação em `/home/deploy/.ssh/authorized_keys`. Se o repositório for privado, configure também uma GitHub Deploy Key somente leitura para que `deploy` consiga executar `git pull`.

## 4. Segredos e dependência externa

```bash
sudo cp /opt/kombfusca/deploy/.env.example /etc/kombfusca/kombfusca.env
sudo chown root:deploy /etc/kombfusca/kombfusca.env
sudo chmod 640 /etc/kombfusca/kombfusca.env
sudo nano /etc/kombfusca/kombfusca.env
```

Gere cada senha com `openssl rand -base64 36`. O admin só é criado quando ainda não existe; depois do primeiro acesso, altere a senha e remova `SEED_ADMIN_EMAIL` e `SEED_ADMIN_PASSWORD` do arquivo.

O projeto chama um segundo serviço em `/scorecounter`, mas ele não está neste repositório. Defina `SCORE_COUNTER_BASE_URL` com a URL real. Se for outro container, conecte-o à rede do Compose. `localhost` dentro do container não aponta para outro serviço.

## 5. Primeiro deploy

```bash
sudo -iu deploy /opt/kombfusca/deploy/deploy.sh
cd /opt/kombfusca
docker compose --env-file /etc/kombfusca/kombfusca.env ps
docker compose --env-file /etc/kombfusca/kombfusca.env logs --tail=200 app
curl -fsS https://SEU_DOMINIO/health
```

A aplicação espera o MySQL, aplica migrations e cria os papéis/admin inicial na inicialização.

## 6. Deploy remoto automático

O workflow `.github/workflows/deploy-production.yml` roda em cada push em `master` (a branch atual deste repositório) e também manualmente. Ele acessa o servidor via SSH, atualiza o Git, faz o build e reinicia os containers. Crie no GitHub um Environment `production` com:

- `SSH_HOST`: host/IP alcançável pelo runner;
- `SSH_PORT`: normalmente `22`;
- `SSH_USER`: `deploy`;
- `SSH_PRIVATE_KEY`: chave privada exclusiva da automação;
- `SSH_KNOWN_HOSTS`: saída de `ssh-keyscan -p 22 HOST`, depois de conferir a fingerprint com `ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key.pub` no servidor.

Se não quiser SSH público, use runner self-hosted ou VPN. Não desative a checagem de `known_hosts`.

## Operação e backup

```bash
# logs
docker compose --env-file /etc/kombfusca/kombfusca.env logs -f --tail=200 app

# backup lógico
docker compose --env-file /etc/kombfusca/kombfusca.env exec -T db \
  sh -c 'exec mysqldump -uroot -p"$MYSQL_ROOT_PASSWORD" --single-transaction --routines --triggers "$MYSQL_DATABASE"' \
  | gzip > "kombfusca-$(date +%F-%H%M).sql.gz"
```

Envie backups para outro equipamento/local e teste a restauração. Snapshots do Proxmox são uma camada adicional, não o único backup. Para rollback de código, reverta o commit em `master`; migrations destrutivas exigem restauração planejada do banco.
