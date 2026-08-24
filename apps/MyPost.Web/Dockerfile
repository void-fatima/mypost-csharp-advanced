FROM node:24-alpine AS build
WORKDIR /app
COPY apps/MyPost.Web/package.json apps/MyPost.Web/package-lock.json ./
RUN npm ci
COPY apps/MyPost.Web .
ARG VITE_API_URL=/api/v1
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

FROM nginx:1.28-alpine
COPY apps/MyPost.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
