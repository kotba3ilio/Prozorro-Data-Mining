FROM node:20-alpine AS build
WORKDIR /app

ARG VITE_API_BASE_URL=
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

COPY src/ProzorroDataMining.Web/package.json ./
COPY src/ProzorroDataMining.Web/pnpm-lock.yaml ./

RUN corepack enable && corepack prepare pnpm@9.15.4 --activate && pnpm install --frozen-lockfile

COPY src/ProzorroDataMining.Web ./
RUN pnpm run build

FROM nginx:1.27-alpine AS runtime
COPY src/ProzorroDataMining.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80