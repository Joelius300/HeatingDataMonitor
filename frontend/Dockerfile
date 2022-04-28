FROM node:14 as build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY ./ .

ARG API_BASE_URL
# TODO maybe a simple sed invokation to update the prod env file with this arg

RUN npm run build -- --prod

FROM nginx:latest as production-stage
COPY --from=build-stage /app/dist/heating-data-monitor /usr/share/nginx/html
