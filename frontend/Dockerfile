FROM alpine:latest as build-stage
WORKDIR /app
COPY ./dist/heating-data-monitor .

ARG API_BASE_URL

# replace the API_BASE_URL_PLACEHOLDER in the already compiled js
RUN find . -type f -name "*.js" -exec sed -i 's@API_BASE_URL_PLACEHOLDER@'"${API_BASE_URL}"'@g' {} +

FROM nginx:latest as production-stage
COPY --from=build-stage /app /usr/share/nginx/html
