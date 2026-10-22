# syntax=docker/dockerfile:1
# Build context: frontend/. Builds the Angular app and serves it with Caddy.

# ---- Build: production configuration, environment.ts (apiBaseUrl "/api", same origin) ----
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration production

# ---- Serve: static files from /srv; /api/* is proxied to the API (see Caddyfile) ----
FROM caddy:2.11-alpine
COPY Caddyfile /etc/caddy/Caddyfile
COPY --from=build /app/dist/track-management/browser /srv
