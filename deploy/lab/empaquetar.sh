#!/usr/bin/env bash
# Arma el zip para el PaaS del laboratorio (kind "dotnet"): un único csproj en la raíz que compila el host
# y referencia las capas, la web ya compilada en wwwroot/ y un appsettings SIN secretos
# (Jwt:SecretKey, la cuenta del lab y la clave de IA viven en
#  /opt/storage/tenants/alicelabs/sites/notes/secrets/notes.json; la conexión a la BD la inyecta el PaaS
#  en ConnectionStrings__Postgres).
# Uso: deploy/lab/empaquetar.sh <ruta/web (NoteAdmin.Fontend)> <salida.zip>
set -euo pipefail
API="$(cd "$(dirname "$0")/../.." && pwd)"
WEB="$(cd "${1:?ruta al front (NoteAdmin.Fontend)}" && pwd)"
OUT="$(realpath -m "${2:?zip de salida}")"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

(cd "$WEB" && npx ng build --configuration production >/dev/null)
# Solo el host y sus capas (sin el servidor MCP ni las pruebas)
git -C "$API" archive HEAD src/AdminServices.Domain src/AdminServices.Application src/AdminServices.Infrastructure \
  src/AdminServices.Presentation src/AdminServices.WebApi Directory.Build.props Directory.Packages.props nuget.config global.json \
  | tar -x -C "$TMP"
rm -f "$TMP"/src/AdminServices.WebApi/appsettings*.json "$TMP"/src/AdminServices.WebApi/*.csproj
cp "$API/deploy/lab/notes.csproj" "$API/deploy/lab/appsettings.json" "$TMP/"
cp -r "$WEB/dist/NotesAdmin/browser" "$TMP/wwwroot"
rm -f "$OUT" && (cd "$TMP" && zip -qr "$OUT" .)
echo "$OUT"
