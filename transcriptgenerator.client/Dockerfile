# Build Stage
FROM node:20 AS build
WORKDIR /app
COPY transcriptgenerator.client/package*.json ./
RUN npm install
COPY transcriptgenerator.client/ .
RUN npx vite build --config vite.config.docker.js

# Production Stage
FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
