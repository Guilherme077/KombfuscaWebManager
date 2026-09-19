#!/usr/bin/env bash
set -Eeuo pipefail

cd /opt/kombfusca
git pull --ff-only
docker compose --env-file /etc/kombfusca/kombfusca.env build --pull app
docker compose --env-file /etc/kombfusca/kombfusca.env up -d --remove-orphans
# Caddyfile is bind-mounted, so Compose does not detect content-only changes.
docker compose --env-file /etc/kombfusca/kombfusca.env restart caddy
docker image prune -f
docker compose --env-file /etc/kombfusca/kombfusca.env ps
