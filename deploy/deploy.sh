#!/usr/bin/env bash
set -Eeuo pipefail

cd /opt/kombfusca
git pull --ff-only
docker compose --env-file /etc/kombfusca/kombfusca.env build --pull app
docker compose --env-file /etc/kombfusca/kombfusca.env up -d --remove-orphans
docker image prune -f
docker compose --env-file /etc/kombfusca/kombfusca.env ps

