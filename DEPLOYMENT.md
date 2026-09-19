# Produção no Ubuntu (Proxmox)

A implantação usa Docker Compose com aplicação ASP.NET Core 8, MySQL 8.4 e Caddy. O HTTPS termina no Cloudflare Tunnel; o Caddy recebe HTTP apenas pela rede privada e encaminha para a aplicação. O MySQL fica apenas na rede interna. Banco e chaves de cookies persistem em volumes Docker.

## 1. Rede e Cloudflare Tunnel

- Aloque ao container ao menos 2 vCPU, 2 GB de RAM e 20 GB de disco e use IP fixo.
- Publique o hostname no Cloudflare Tunnel apontando para `http://IP_PRIVADO_DO_CAKWEB:80`.
- Configure `HTTP Host Header` no tunnel com o mesmo valor de `DOMAIN`.
- Permita TCP 80 apenas do container `cloudflared` para o container da aplicação.
- Não encaminhe 80/443 no roteador e nunca exponha 3306.
- Restrinja SSH por chave e, de preferência, VPN/Tailscale ou IP de origem.

O trecho privado entre os containers usa HTTP; o tráfego do navegador até o Cloudflare continua protegido por HTTPS. Para criptografar também a rede interna, use um Cloudflare Origin Certificate ou uma PKI interna em vez de ACME público.

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

Gere cada senha com `openssl rand -base64 36`. Preencha também `SEED_ADMIN_FULL_NAME`. O admin só é criado quando ainda não existe; depois do primeiro acesso, altere a senha e remova `SEED_ADMIN_EMAIL`, `SEED_ADMIN_PASSWORD` e `SEED_ADMIN_FULL_NAME` do arquivo.

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

Se o volume `data_protection_keys` já tiver sido criado por uma versão anterior como `root`, corrija sua propriedade uma única vez e reinicie a aplicação:

```bash
docker exec -u root kombfusca-app-1 chown -R pwuser:pwuser /home/pwuser/.aspnet/DataProtection-Keys
docker restart kombfusca-app-1
```

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
