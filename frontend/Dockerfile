FROM node:14 as build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY ./ .

ARG API_BASE_URL
# Angular doesn't use environment args as environment args (makes sense right) so edit env-file instead
RUN sed -i 's@API_BASE_URL_PLACEHOLDER@'"${API_BASE_URL}"'@' /app/src/environments/environment.prod.ts

RUN npm run build -- --prod

FROM nginx:latest as production-stage
COPY --from=build-stage /app/dist/heating-data-monitor /usr/share/nginx/html
