FROM node:14 as build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY ./ .

ARG API_BASE_URL

RUN npm run build -- --prod

FROM nginx:latest as production-stage
COPY --from=build-stage /app/dist/heating-data-monitor /usr/share/nginx/html
